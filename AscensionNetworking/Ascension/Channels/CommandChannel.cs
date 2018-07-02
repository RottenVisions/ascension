using System.Collections.Generic;
using UnityEngine;

namespace Ascension.Networking
{
    public class CommandChannel : Channel
    {
        private int PingFrames
        {
            get
            {
                return
                    Mathf.CeilToInt((connection.SockConn.AliasedPing * Core.Config.commandPingMultiplier) /
                                    Core.FrameDeltaTime);
            }
        }

        private Dictionary<NetworkId, EntityProxy> IncommingProxiesByNetworkId
        {
            get { return connection.entityChannel._incommingDict; }
        }

        private Dictionary<NetworkId, EntityProxy> OutgoingProxiesByNetworkId
        {
            get { return connection.entityChannel._outgoingDict; }
        }

        public CommandChannel()
        {
        }

        public override void Pack(Packet packet)
        {
            int pos = packet.Position;

            PackResult(packet);
            PackInput(packet);

            packet.Stats.CommandBits = packet.Position - pos;
        }

        public override void Read(Packet packet)
        {
            int startPtr = packet.Position;

            ReadResult(packet);
            ReadInput(packet);

            packet.Stats.CommandBits = packet.Position - startPtr;
        }


        private bool EntityHasUnsentState(Entity entity)
        {
            var it = entity.CommandQueue.GetIterator();

            while (it.Next())
            {
                if (it.val.Flags & CommandFlags.SEND_STATE)
                {
                    return true;
                }
            }
            return false;
        }

        private void PackResult(Packet packet)
        {
            foreach (EntityProxy proxy in OutgoingProxiesByNetworkId.Values)
            {
                Entity entity = proxy.Entity;

                // four conditions have to hold
                // 1) Entity must exist locally (not null)
                // 2) The connection must be the controller
                // 3) The entity must exist remotely
                // 4) The entity has to have unsent states
                if ((entity != null) && ReferenceEquals(entity.Controller, connection) &&
                    connection.entityChannel.ExistsOnRemote(entity) && EntityHasUnsentState(entity))
                {
                    NetAssert.True(entity.IsOwner);

                    int proxyPos = packet.Position;
                    int cmdWriteCount = 0;

                    packet.WriteBool(true);
                    packet.WriteNetworkId(proxy.NetworkId);

                    var it = entity.CommandQueue.GetIterator();

                    while (it.Next())
                    {
                        if (it.val.Flags & CommandFlags.HAS_EXECUTED)
                        {
                            if (it.val.Flags & CommandFlags.SEND_STATE)
                            {
                                int cmdPos = packet.Position;

                                packet.WriteBool(true);
                                packet.WriteTypeId(it.val.ResultObject.Meta.TypeId);
                                packet.WriteUShort(it.val.Sequence, Command.SEQ_BITS);
                                packet.WriteToken(it.val.ResultObject.Token);

                                it.val.PackResult(connection, packet);

                                if (packet.Overflowing)
                                {
                                    packet.Position = cmdPos;
                                    break;
                                }
                                else
                                {
                                    cmdWriteCount += 1;

                                    it.val.Flags &= ~CommandFlags.SEND_STATE;
                                    it.val.Flags |= CommandFlags.SEND_STATE_PERFORMED;
                                }
                            }
                        }
                    }

                    // we wrote too much or nothing at all
                    if (packet.Overflowing || (cmdWriteCount == 0))
                    {
                        packet.Position = proxyPos;
                        break;
                    }
                    else
                    {
                        // stop marker for states
                        packet.WriteStopMarker();
                    }

                    // dispose commands we dont need anymore
                    while ((entity.CommandQueue.Count > 1) &&
                           (entity.CommandQueue.First.Flags & CommandFlags.SEND_STATE_PERFORMED))
                    {
                        entity.CommandQueue.RemoveFirst().Free();
                    }
                }
            }

            // stop marker for proxies
            packet.WriteStopMarker();
        }

        private void ReadResult(Packet packet)
        {
            while (packet.CanRead())
            {
                if (packet.ReadBool() == false)
                {
                    break;
                }

                NetworkId netId = packet.ReadNetworkId();
                EntityProxy proxy = IncommingProxiesByNetworkId[netId];
                Entity entity = proxy.Entity;

                while (packet.CanRead())
                {
                    if (packet.ReadBool() == false)
                    {
                        break;
                    }

                    TypeId typeId = packet.ReadTypeId();
                    ushort sequence = packet.ReadUShort(Command.SEQ_BITS);
                    IMessageRider resultToken = packet.ReadToken();

                    Command cmd = null;

                    if (entity != null)
                    {
                        var it = entity.CommandQueue.GetIterator();

                        while (it.Next())
                        {
                            int dist = NetMath.SeqDistance(it.val.Sequence, sequence, Command.SEQ_SHIFT);
                            if (dist > 0)
                            {
                                break;
                            }
                            if (dist < 0)
                            {
                                it.val.Flags |= CommandFlags.DISPOSE;
                            }
                            if (dist == 0)
                            {
                                cmd = it.val;
                                break;
                            }
                        }
                    }

                    if (cmd)
                    {
                        cmd.ResultObject.Token = resultToken;
                        cmd.Flags |= CommandFlags.CORRECTION_RECEIVED;

                        if (cmd.Meta.SmoothFrames > 0)
                        {
                            cmd.BeginSmoothing();
                        }

                        cmd.ReadResult(connection, packet);
                    }
                    else
                    {
                        cmd = Factory.NewCommand(typeId);
                        cmd.ReadResult(connection, packet);
                        cmd.Free();
                    }
                }
                // remove all disposable commands
                if (entity != null)
                {
                    while ((entity.CommandQueue.Count > 1) && (entity.CommandQueue.First.Flags & CommandFlags.DISPOSE))
                    {
                        entity.CommandQueue.RemoveFirst().Free();
                    }
                }
            }
        }

        private void PackInput(Packet packet)
        {
            foreach (EntityProxy proxy in IncommingProxiesByNetworkId.Values)
            {
                Entity entity = proxy.Entity;

                if (entity && entity.HasControl && (entity.CommandQueue.Count > 0))
                {
                    int proxyPos = packet.Position;
                    packet.WriteContinueMarker();
                    packet.WriteNetworkId(proxy.NetworkId);

                    var redundancy = Mathf.Min(entity.CommandQueue.Count, Core.Config.commandRedundancy);

                    // if we are sending the entire command queue, then make sure we're not sending a command we already received a correction for
                    if ((entity.CommandQueue.Count == redundancy) &&
                        (entity.CommandQueue.First.Flags & CommandFlags.CORRECTION_RECEIVED))
                    {
                        redundancy -= 1;
                    }

                    var cmd = entity.CommandQueue.Last;

                    // go to first command
                    for (int i = 0; i < (redundancy - 1); ++i)
                    {
                        cmd = entity.CommandQueue.Prev(cmd);
                    }

                    // write all commands into the packet
                    for (int i = 0; i < redundancy; ++i)
                    {
                        //NetLog.Debug("PACK | cmd.Frame: {0}, Network.Frame: {1}", cmd.ServerFrame, Core.Frame);

                        int cmdPos = packet.Position;

                        packet.WriteContinueMarker();
                        packet.WriteTypeId(cmd.Meta.TypeId);
                        packet.WriteUShort(cmd.Sequence, Command.SEQ_BITS);
                        packet.WriteIntVB(cmd.ServerFrame);
                        packet.WriteToken(cmd.InputObject.Token);

                        cmd.PackInput(connection, packet);
                        cmd = entity.CommandQueue.Next(cmd);

                        if (packet.Overflowing)
                        {
                            packet.Position = cmdPos;
                            break;
                        }
                    }

                    // overflowing, reset before this proxy and break
                    if (packet.Overflowing)
                    {
                        packet.Position = proxyPos;
                        break;
                    }
                    else
                    {
                        // stop marker for commands
                        packet.WriteStopMarker();
                    }
                }
            }

            // stop marker for proxies
            packet.WriteStopMarker();
        }

        private void ReadInput(Packet packet)
        {
            int maxFrame = Core.Frame;
            int minFrame = maxFrame - (Core.Config.commandDelayAllowed + PingFrames);

            while (packet.ReadStopMarker())
            {
                NetworkId netId = packet.ReadNetworkId();
                EntityProxy proxy = null;

                if (OutgoingProxiesByNetworkId.ContainsKey(netId))
                {
                    proxy = OutgoingProxiesByNetworkId[netId];
                }

                while (packet.ReadStopMarker())
                {
                    Command cmd = Factory.NewCommand(packet.ReadTypeId());
                    cmd.Sequence = packet.ReadUShort(Command.SEQ_BITS);
                    cmd.ServerFrame = packet.ReadIntVB();
                    cmd.InputObject.Token = packet.ReadToken();
                    cmd.ReadInput(connection, packet);

                    // no proxy or entity
                    if (!proxy || !proxy.Entity)
                    {
                        continue;
                    }

                    Entity entity = proxy.Entity;

                    // remote is not controller
                    if (ReferenceEquals(entity.Controller, connection) == false)
                    {
                        continue;
                    }

                    // sequence is old
                    if (NetMath.SeqDistance(cmd.Sequence, entity.CommandSequence, Command.SEQ_SHIFT) <= 0)
                    {
                        continue;
                    }

                    // put on command queue
                    entity.CommandQueue.AddLast(cmd);
                    entity.CommandSequence = cmd.Sequence;
                }
            }
        }
    }
}
