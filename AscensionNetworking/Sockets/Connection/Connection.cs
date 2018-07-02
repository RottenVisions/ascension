using System;
using UnityEngine;
using System.Collections.Generic;
using System.Net;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class Connection : NetObject
    {
        private Channel[] channels;
        private SocketConnection sockConn;
        public bool isReady;
        public string address;

        //Simulation vars
        int framesToStep;
        int packetsReceived;
        int packetCounter;

        int remoteFrameDiff;
        int remoteFrameActual;
        int remoteFrameEstimated;
        bool remoteFrameAdjust;

        int bitsSecondIn;
        int bitsSecondInAcc;

        int bitsSecondOut;
        int bitsSecondOutAcc;

        public EventChannel eventChannel;
        public SceneLoadChannel sceneLoadChannel;
        public EntityChannel entityChannel;
        public CommandChannel commandChannel;
        public List<Entity> controlling;
        public EntityList controllingList;

        public RingBuffer<PacketStats> packetStatsIn;
        public RingBuffer<PacketStats> packetStatsOut;

        public bool canReceiveEntities = true;
        public SceneLoadState remoteSceneLoading;

        public Channel[] Channels { get { return channels; } }

        public int RemoteFrame
        {
            get { return remoteFrameEstimated; }
        }

        /// <summary>
        /// Returns true if the remote computer on the other end of this connection is loading a map currently, otherwise false
        /// </summary>
        public bool IsLoadingMap
        {
            get
            {
                return
                  (remoteSceneLoading.Scene != Core.LocalSceneLoading.Scene) ||
                  (remoteSceneLoading.State != SceneLoadState.STATE_CALLBACK_INVOKED);
            }
        }

        public EntityLookup ScopedTo
        {
            get { return entityChannel._outgoingLookup; }
        }

        public EntityLookup SourceOf
        {
            get { return entityChannel._incommingLookup; }
        }

        public EntityList HasControlOf
        {
            get { return controllingList; }
        }

        public SocketConnection SockConn
        {
            get { return sockConn; }
        }

        /// <summary>
        /// A data token that was passed by the client when initiating a connection
        /// </summary>
        public IMessageRider ConnectToken
        {
            get;
            set;
        }

        /// <summary>
        /// A data token that was passed by the server when accepting the connection
        /// </summary>
        public IMessageRider AcceptToken
        {
            get;
            set;
        }

        /// <summary>
        /// A data token that was passed by the server when disconnecting a connection
        /// </summary>
        public IMessageRider DisconnectToken
        {
            get;
            set;
        }

        /// <summary>
        /// The round-trip time on the network
        /// </summary>
        public float PingNetwork
        {
            get { return SockConn.NetworkPing; }
        }

        /// <summary>
        /// The round-trip time on the network
        /// </summary>
        public int DejitterFrames
        {
            get { return remoteFrameActual - remoteFrameEstimated; }
        }

        public float PingAliased
        {
            get { return SockConn.NetworkPing; }
        }

        /// <summary>
        /// The round-trip time across the network, including processing delays and acks
        /// </summary>
        public int RemoteFrameLatest
        {
            get { return remoteFrameActual; }
        }

        public int RemoteFrameDiff
        {
            get { return remoteFrameDiff; }
        }

        /// <summary>
        /// How many bits per second we are receiving in
        /// </summary>
        public int BitsPerSecondIn
        {
            get { return bitsSecondIn; }
        }

        /// <summary>
        /// How many bits per second we are sending out
        /// </summary>
        public int BitsPerSecondOut
        {
            get { return bitsSecondOut; }
        }

        public long ConnectionId
        {
            get { return SockConn.ConnectionInfo.Peer.ConnectId; }
        }

        /// <summary>
        /// Remote end point of this connection
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return SockConn.RemoteEndPoint; }
        }

        /// <summary>
        /// User assignable object which lets you pair arbitrary data with the connection
        /// </summary>
        public object UserData
        {
            get;
            set;
        }

        public void SetCanReceiveEntities(bool value)
        {
            canReceiveEntities = value;
        }

        /// <summary>
        /// The ratio as a percentage between 0 and 1 (normalized) of the amount of data being transferred versus the
        /// maximum amount of data that can be transferred. This is used in helping to throttle send rate
        /// </summary>
        public float FillRatio
        {
            get
            {
                //Use reduced packet size here because the actual data being sent isn't the full packet size (subtract ~120)
                float maxDataTransmission = Core.SockInt.ReducedPacketSize * Core.NetworkRate;
                //Get the highest rate, whether download of upload
                float highestRate = Mathf.Max(bitsSecondIn, bitsSecondOut);
                //Normalize
                return NetMath.Clamp(highestRate / maxDataTransmission, 0, 1f);
            }
        }

        /// <summary>
        /// The ratio of data being sent (the number out of the simulation FPS). If we have a fill ratio of 0.9
        /// and a simulation rate (FPS) of 60 - this value will be 52, because 52 is 90% of 60.
        /// </summary>
        public int SendRateRatio
        {
            get
            {
                float r = FillRatio;

                if (r < 0.25f)
                {
                    return 1;
                }

                r = r - 0.25f;
                r = r / 0.75f;

                return Mathf.Clamp((int)(r * 60), 1, 60);
            }
        }

        /// <summary>
        /// The send rate we should adjust to based upon the amount of data being sent
        /// </summary>
        public int ThrottledSendRate
        {
            get
            {
                int rate = Mathf.RoundToInt((float)Core.FramesPerSecond / SendRateRatio);
                //Catch the rate on the client being the same as the current FPS, if this is the case
                //Set it to be the default
                if (rate == Core.FramesPerSecond)
                    return Core.LocalSendRate;
                return rate;
            }
        }

        public Connection(SocketConnection sConn)
        {
            UserData = sConn.UserToken;

            controlling = new List<Entity>();
            controllingList = new EntityList(controlling);

            sockConn = sConn;
            sockConn.UserToken = this;

            channels = new Channel[] {
                sceneLoadChannel = new SceneLoadChannel(),
                commandChannel = new CommandChannel(),
                eventChannel = new EventChannel(),
                entityChannel = new EntityChannel()
            };

            remoteFrameAdjust = false;
            remoteSceneLoading = SceneLoadState.DefaultRemote();

            packetStatsOut = new RingBuffer<PacketStats>(Core.Config.framesPerSecond);
            packetStatsOut.Autofree = true;

            packetStatsIn = new RingBuffer<PacketStats>(Core.Config.framesPerSecond);
            packetStatsIn.Autofree = true;

            // set channels connection
            for (int i = 0; i < channels.Length; ++i)
            {
                channels[i].connection = this;
            }
        }

        public ExistsResult ExistsOnRemote(AscensionEntity entity)
        {
            return entityChannel.ExistsOnRemote(entity.AEntity, false);
        }

        public ExistsResult ExistsOnRemote(AscensionEntity entity, bool allowMaybe)
        {
            return entityChannel.ExistsOnRemote(entity.AEntity, allowMaybe);
        }

        /// <summary>
        /// Disconnect this connection
        /// </summary>
        public void Disconnect()
        {
            Disconnect(null);
        }

        /// <summary>
        /// Disconnect this connection with custom data
        /// </summary>
        /// <param name="token">A data token</param>
        public void Disconnect(IMessageRider token)
        {
            SockConn.Disconnect(token);
        }

        public int GetSkippedUpdates(AscensionEntity en)
        {
            return entityChannel.GetSkippedUpdates(en.AEntity);
        }

        /// <summary>
        /// Reference comparison between two connections
        /// </summary>
        /// <param name="obj">The object to compare</param>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        /// <summary>
        /// A hash code for this connection
        /// </summary>
        public override int GetHashCode()
        {
            return SockConn.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[Connection {0}]", SockConn.RemoteEndPoint);
        }

        public void DisconnectedInternal()
        {
            for (int i = 0; i < channels.Length; ++i)
            {
                channels[i].Disconnected();
            }

            if (UserData != null)
            {
                if (UserData is IDisposable)
                {
                    (UserData as IDisposable).Dispose();
                }

                UserData = null;
            }
        }

        public bool StepRemoteEntities()
        {
            if (framesToStep > 0)
            {
                framesToStep -= 1;
                remoteFrameEstimated += 1;

                foreach (EntityProxy proxy in entityChannel._incommingDict.Values)
                {
                    if (proxy.Entity.HasPredictedControl || proxy.Entity.IsFrozen)
                    {
                        continue;
                    }

                    proxy.Entity.Simulate();
                }
            }

            return framesToStep > 0;
        }

        public void AdjustRemoteFrame()
        {
            if (packetsReceived == 0)
            {
                return;
            }

            if (Core.Config.disableDejitterBuffer)
            {
                if (remoteFrameAdjust)
                {
                    framesToStep = Mathf.Max(0, remoteFrameActual - remoteFrameEstimated);
                    remoteFrameEstimated = remoteFrameActual;
                    remoteFrameAdjust = false;
                }
                else
                {
                    framesToStep = 1;
                }

                return;
            }

            int rate = Core.RemoteSendRate;
            int delay = Core.LocalInterpolationDelay;
            int delayMin = Core.LocalInterpolationDelayMin;
            int delayMax = Core.LocalInterpolationDelayMax;

            bool useDelay = delay >= 0;

            if (remoteFrameAdjust)
            {
                remoteFrameAdjust = false;

                // if we apply delay
                if (useDelay)
                {
                    // first packet is special!
                    if (packetsReceived == 1)
                    {
                        remoteFrameEstimated = remoteFrameActual - delay;
                    }

                    // calculate frame diff (actual vs. estimated)
                    remoteFrameDiff = remoteFrameActual - remoteFrameEstimated;

                    // if we are *way* off
                    if ((remoteFrameDiff < (delayMin - rate)) || (remoteFrameDiff > (delayMax + rate)))
                    {
                        int oldFrame = remoteFrameEstimated;
                        int newFrame = remoteFrameActual - delay;

                        NetLog.Debug("FRAME RESET: {0}", remoteFrameDiff);

                        remoteFrameEstimated = newFrame;
                        remoteFrameDiff = remoteFrameActual - remoteFrameEstimated;

                        // call into channels to notify that the frame reset
                        //for (int i = 0; i < _channels.Length; ++i) {
                        //  _channels[i].RemoteFrameReset(oldFrame, newFrame);
                        //}
                    }
                }
            }

            if (useDelay)
            {
                // drifted to far behind, step two frames
                if (remoteFrameDiff > delayMax)
                {
                    NetLog.Debug("FRAME FORWARD: {0}", remoteFrameDiff);
                    framesToStep = 2;
                    remoteFrameDiff -= framesToStep;
                }

                // drifting to close to 0 delay, stall one frame
                else if (remoteFrameDiff < delayMin)
                {
                    NetLog.Debug("FRAME STALL: {0}", remoteFrameDiff);
                    framesToStep = 0;
                    remoteFrameDiff += 1;
                }

                // we have not drifted, just step one frame
                else
                {
                    framesToStep = 1;
                }
            }
            else
            {
                remoteFrameEstimated = remoteFrameActual - (rate - 1);
            }
            //Debug.Log(string.Format("{0}, {1}, {2}, {3}", remoteFrameActual, remoteFrameAdjust, remoteFrameDiff, remoteFrameEstimated));
        }

        public bool Send(out Packet sentPacket)
        {
            bool sent;
            try
            {
                Packet packet = Core.AllocatePacket();
                packet.Frame = Core.Frame;
                packet.Number = ++packetCounter;
                packet.Type = (byte)NetworkMsg.Data;

                packet.UserToken = packet;
                //Write signature & frame
                packet.WriteByte(packet.Type);
                packet.WriteIntVB(packet.Frame);

                for (int i = 0; i < channels.Length; ++i)
                {
                    channels[i].Pack(packet);
                }

                NetAssert.False(packet.Overflowing);

                sockConn.Send(packet);

                sent = true;
                sentPacket = packet;
                sentPacket.Set(packet);
                //SocketLog.Info("Sending packet of length {0}", packet.ActualSize);

                bitsSecondOutAcc += packet.Position;
                packetStatsOut.Enqueue(packet.Stats);
            }
            catch (Exception exn)
            {
                NetLog.Exception(exn);
                throw;
            }

            return sent;
        }

        public void PacketReceived(Packet Packet)
        {
            try
            {
                using (Packet packet = Core.AllocatePacket())
                {
                    packet.Set(Packet); //This copies the values into the newly acquired packet
                    //Read signature & frame
                    packet.Type = packet.ReadByte();
                    packet.Frame = packet.ReadIntVB();

                    if (packet.Frame > remoteFrameActual)
                    {
                        remoteFrameAdjust = true;
                        remoteFrameActual = packet.Frame;
                    }

                    //OLD method
                    //bitsSecondInAcc += packet.ActualSize;
                    bitsSecondInAcc += packet.Position;
                    packetsReceived += 1;

                    for (int i = 0; i < channels.Length; ++i)
                    {
                        channels[i].Read(packet);
                    }
                    //for (int i = 0; i < channels.Length; ++i)
                    //{
                    //    channels[i].ReadDone();
                    //}

                    packetStatsIn.Enqueue(packet.Stats);

                    NetAssert.False(Packet.Overflowing);

                    //SocketLog.Info("Received packet of length {0}", packet.ActualSize);
                }
            }
            catch (Exception exn)
            {
                NetLog.Exception(exn);
                NetLog.Error("exception thrown while unpacking data from {0}, disconnecting", sockConn.RemoteEndPoint);
                Disconnect();
            }
        }

        public void PacketDelivered(Packet packet)
        {
            try
            {
                for (int i = 0; i < channels.Length; ++i)
                {
                    channels[i].Delivered(packet);
                }
            }
            catch (Exception exn)
            {
                NetLog.Exception(exn);
                NetLog.Error("exception thrown while handling delivered packet to {0}", sockConn.RemoteEndPoint);
            }
        }

        public void PacketLost(Packet packet)
        {
            for (int i = 0; i < channels.Length; ++i)
            {
                channels[i].Lost(packet);
            }
            try
            {

            }
            catch (Exception exn)
            {
                NetLog.Exception(exn);
                NetLog.Error("exception thrown while handling lost packet to {0}", sockConn.RemoteEndPoint);
            }
        }

        public static implicit operator bool(Connection cn)
        {
            return cn != null;
        }

        public void SwitchPerfCounters()
        {
            bitsSecondOut = bitsSecondOutAcc;
            bitsSecondOutAcc = 0;

            bitsSecondIn = bitsSecondInAcc;
            bitsSecondInAcc = 0;
        }

        public void AddBitsPerSecondIn(int bitsAdded)
        {
            bitsSecondInAcc += bitsAdded;
        }

        public void AddBitsPerSecondOut(int bitsAdded)
        {
            bitsSecondOutAcc += bitsAdded;
        }
    }
}
