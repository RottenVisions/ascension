using System.Collections.Generic;
using UnityEngine;

namespace Ascension.Networking
{
    public struct EventUnreliable
    {
        public class PriorityComparer : IComparer<EventUnreliable>
        {
            public static readonly PriorityComparer Instance = new PriorityComparer();

            PriorityComparer()
            {

            }

            int IComparer<EventUnreliable>.Compare(EventUnreliable x, EventUnreliable y)
            {
                return y.Priority.CompareTo(x.Priority);
            }
        }

        public bool Skipped;
        public Event NetworkEvent;
        public float Priority;

        public static EventUnreliable Wrap(Event ev)
        {
            EventUnreliable r;

            r.NetworkEvent = ev;
            r.Priority = 0;
            r.Skipped = false;

            return r;
        }
    }

    public struct EventReliable
    {
        public Event NetworkEvent;
        public uint Sequence;

        public static EventReliable Wrap(Event ev)
        {
            return Wrap(ev, 0);
        }

        public static EventReliable Wrap(Event ev, uint sequence)
        {
            EventReliable r;

            r.NetworkEvent = ev;
            r.Sequence = sequence;

            return r;
        }
    }

    public class EventChannel : Channel
    {
        List<EventUnreliable> unreliableSend;
        EventReliableSendBuffer reliableOrderedSend;
        EventReliableRecvBuffer reliableOrderedRecv;

        public EventChannel()
        {
            unreliableSend = new List<EventUnreliable>(256);
            reliableOrderedSend = new EventReliableSendBuffer(Event.RELIABLE_WINDOW_BITS, Event.RELIABLE_SEQUENCE_BITS);
            reliableOrderedRecv = new EventReliableRecvBuffer(Event.RELIABLE_WINDOW_BITS, Event.RELIABLE_SEQUENCE_BITS);
        }

        public void Queue(Event ev)
        {
            if (ev.Reliability == ReliabilityModes.Unreliable)
            {
                // push on unreliable send queue
                unreliableSend.Add(EventUnreliable.Wrap(ev));

                // incr refs!
                ev.IncrementRefs();
            }
            else
            {
                if (reliableOrderedSend.TryEnqueue(EventReliable.Wrap(ev)))
                {
                    ev.IncrementRefs();
                }
                else
                {
                    NetLog.Warn("The reliable-ordered event queue for {0} is full, disconnecting", connection);
                    connection.Disconnect();
                }
            }
        }

        public override void Delivered(Packet packet)
        {
            // set events as delivered
            for (int i = 0; i < packet.ReliableEvents.Count; ++i)
            {
                reliableOrderedSend.SetDelivered(packet.ReliableEvents[i]);
            }

            EventReliable reliable;

            while (reliableOrderedSend.TryRemove(out reliable))
            {
                reliable.NetworkEvent.DecrementRefs();
            }

            // clear packet events out 
            packet.ReliableEvents.Clear();
        }

        public override void Lost(Packet packet)
        {
            for (int i = 0; i < packet.ReliableEvents.Count; ++i)
            {
                reliableOrderedSend.SetSend(packet.ReliableEvents[i]);
            }

            packet.ReliableEvents.Clear();
        }

        public override void Pack(Packet packet)
        {
            int startPos = packet.Position;

            // prune events and calculate priority for remaining ones
            for (int i = 0; i < unreliableSend.Count; ++i)
            {
                EventUnreliable r = unreliableSend[i];

                if (r.NetworkEvent.IsEntityEvent)
                {
                    var existsOnRemote = connection.entityChannel.ExistsOnRemote(r.NetworkEvent.TargetEntity);
                    if (existsOnRemote == false)
                    {
                        unreliableSend[i].NetworkEvent.DecrementRefs();
                        unreliableSend.RemoveAt(i);

                        i -= 1;

                        continue;
                    }
                }

                r.Priority =
                  r.NetworkEvent.IsEntityEvent
                    ? r.NetworkEvent.TargetEntity.PriorityCalculator.CalculateEventPriority(connection, r.NetworkEvent)
                    : 10;

                unreliableSend[i] = r;
            }

            // sort on priority (descending)
            unreliableSend.Sort(EventUnreliable.PriorityComparer.Instance);

            int ptrStart = packet.Position;

            // pack reliable events into packet
            EventReliable reliable;

            while (reliableOrderedSend.TryNext(out reliable))
            {
                int ptr = packet.Position;

                bool packOk = PackEvent(reliable.NetworkEvent, packet, reliable.Sequence);
                bool notOverflowing = packet.Overflowing == false;

                if (packOk == false)
                {
                    NetLog.Error("Reliable failed to pack, this means all other reliable events will stall");
                }

                if (packOk && notOverflowing)
                {
                    packet.ReliableEvents.Add(reliable);
                }
                else
                {
                    // reset ptr
                    packet.Position = ptr;

                    // flag for sending
                    reliableOrderedSend.SetSend(reliable);
                    break;
                }
            }

            // pack unreliable events into packet
            for (int i = 0; i < unreliableSend.Count; ++i)
            {
                int ptr = packet.Position;

                bool packOk = PackEvent(unreliableSend[i].NetworkEvent, packet, 0);
                bool notOverflowing = packet.Overflowing == false;

                if (packOk && notOverflowing)
                {
                    //unreliableSend[i].NetworkEvent.DecrementRefs();
                    unreliableSend.RemoveAt(i);
                }
                else
                {
                    // reset ptr
                    packet.Position = ptr;
                }
            }

            packet.WriteStopMarker();

            // prune entities which have been skipped twice
            for (int i = 0; i < unreliableSend.Count; ++i)
            {
                EventUnreliable r = unreliableSend[i];

                if (r.Skipped)
                {
                    unreliableSend.RemoveAt(i);
                }
                else
                {
                    // this will be pruned next time if it's not sent
                    r.Skipped = true;

                    unreliableSend[i] = r;
                }
            }

            packet.Stats.EventBits = packet.Position - startPos;
        }

        bool PackEvent(Event ev, Packet stream, uint sequence)
        {
            NetLog.Debug("sending event {0}", ev);

            stream.WriteContinueMarker();

            // type id of this event
            stream.WriteTypeId(ev.Meta.TypeId);

            // targets of this event
            stream.WriteInt(ev.Targets, 5);

            if (stream.WriteBool(ev.Reliability == ReliabilityModes.ReliableOrdered))
            {
                // write sequence number for reliable events
                stream.WriteUInt(sequence, Event.RELIABLE_SEQUENCE_BITS);
            }
            else
            {
                if (ev.IsEntityEvent)
                {
                    // write network id for entity events
                    stream.WriteEntity(ev.TargetEntity);
                }
            }

            stream.WriteByteArrayLengthPrefixed(ev.BinaryData, Core.Config.packetSize / 2);
            return ev.Pack(connection, stream);
        }

        public override void Read(Packet packet)
        {
            int startPtr = packet.Position;

            while (packet.ReadStopMarker())
            {
                uint sequence = 0;
                Event ev = ReadEvent(packet, ref sequence);

                NetLog.Debug("recv event {0}", ev);
                if (ev.Reliability == ReliabilityModes.Unreliable)
                {
                    EventDispatcher.Received(ev);
                }
                else
                {
                    switch (reliableOrderedRecv.TryEnqueue(EventReliable.Wrap(ev, sequence)))
                    {
                        case RecvBufferAddResult.Old:
                        case RecvBufferAddResult.OutOfBounds:
                        case RecvBufferAddResult.AlreadyExists:
                            NetLog.Debug("FAILED");
                            //ev.DecrementRefs();
                            break;
                    }
                }
            }

            EventReliable reliable;

            while (reliableOrderedRecv.TryRemove(out reliable))
            {
                EventDispatcher.Received(reliable.NetworkEvent);
            }

            packet.Stats.EventBits = packet.Position - startPtr;
        }

        Event ReadEvent(Packet stream, ref uint sequence)
        {
            Event ev;

            ev = Factory.NewEvent(stream.ReadTypeId());
            ev.Targets = stream.ReadInt(5);
            ev.SourceConnection = connection;

            if (stream.ReadBool())
            {
                sequence = stream.ReadUInt(Event.RELIABLE_SEQUENCE_BITS);

                // assign relability mode
                ev.Reliability = ReliabilityModes.ReliableOrdered;
            }
            else
            {
                if (ev.IsEntityEvent)
                {
                    ev.TargetEntity = stream.ReadEntity();
                }

                // assign relability mode
                ev.Reliability = ReliabilityModes.Unreliable;
            }

            ev.BinaryData = stream.ReadByteArraySimple();
            ev.Read(connection, stream);
            return ev;
        }
    }
}
