using System;
using System.Collections.Generic;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class Packet : BasePacket, IDisposable
    {
        public volatile bool Pooled = true;
        public PacketPool pool { get; set; }

        public byte Type;
        public int Frame;
        public int Number;

        public PacketStats Stats;

        public List<EventReliable> ReliableEvents = new List<EventReliable>();
        public Queue<EntityProxyEnvelope> EntityUpdates = new Queue<EntityProxyEnvelope>();

        #region Constructors
        public Packet()
        {
            position = 0;
            data = new byte[1500]; //Give an normal number for a packet size
            length = data.Length;
            Type = (byte)NetworkMsg.Unknown;
        }

        public Packet(NetworkMsg msgType)
        {
            position = 0;
            data = new byte[1500]; //Give an normal number for a packet size
            length = data.Length;
            Type = (byte)msgType;
        }

        public Packet(int size)
        {
            position = 0;
            data = new byte[size];
            length = data.Length;
            Type = (byte)NetworkMsg.Unknown;
        }

        public Packet(int size, NetworkMsg msgType)
        {
            position = 0;
            data = new byte[size];
            length = data.Length;
            Type = (byte)msgType;
        }

        public Packet(byte[] arr)
      : this(arr, arr.Length) {
        }

        public Packet(byte[] arr, NetworkMsg msgType)
            : this(arr, arr.Length, msgType)
        {
        }

        public Packet(byte[] arr, int size)
        {
            position = 0;
            data = arr;
            length = size << 3;
            Type = (byte)NetworkMsg.Unknown;
        }
        public Packet(byte[] arr, int size, NetworkMsg msgType)
        {
            position = 0;
            data = arr;
            length = size << 3;
            Type = (byte)msgType;
        }
        #endregion

        public void Set(Packet packet)
        {
            ByteBuffer = packet.ByteBuffer;
            EntityUpdates = packet.EntityUpdates;
            Type = packet.Type;
            Frame = packet.Frame;
            Number = packet.Number;
            Pooled = packet.Pooled;
            Position = packet.Position;
            Size = packet.Size;
            ReliableEvents = packet.ReliableEvents;
            Stats = packet.Stats;
            UserToken = packet.UserToken;
        }

        public void Reset()
        {
            Position = 0;
        }

        void IDisposable.Dispose()
        {
            PacketPool.Dispose(this);
        }
    }

}
