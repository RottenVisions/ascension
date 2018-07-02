using System.Collections.Generic;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class PacketPool
    {
        readonly SocketInterface socket;
        readonly Stack<Packet> pool = new Stack<Packet>();

        public PacketPool(SocketInterface s)
        {
            socket = s;
        }

        public void Release(Packet stream)
        {
            NetAssert.False(stream.isPooled);

            lock (pool)
            {
                stream.Size = 0;
                stream.Position = 0;
                stream.isPooled = true;

                pool.Push(stream);
            }
        }

        public Packet Acquire()
        {
            Packet stream = null;

            lock (pool)
            {
                if (pool.Count > 0)
                {
                    stream = pool.Pop();
                }
            }

            if (stream == null)
            {
                stream = new Packet(new byte[RuntimeSettings.Instance.packetSize - 100]);
                stream.pool = this;
            }

            NetAssert.True(stream.isPooled);

            stream.isPooled = false;
            stream.Position = 0;
            stream.Size = (RuntimeSettings.Instance.packetSize - 100) << 3;

            return stream;
        }

        public void Free()
        {
            lock (pool)
            {
                while (pool.Count > 0)
                {
                    pool.Pop();
                }
            }
        }

        public static void Dispose(Packet packet)
        {
            //Do something here if we are pooling our own packet implementation
            packet.Dispose();
            packet = null;
        }
    }
}
