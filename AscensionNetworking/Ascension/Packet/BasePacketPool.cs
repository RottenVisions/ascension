using System.Collections.Generic;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class BasePacketPool
    {
        readonly SocketInterface socket;
        readonly Stack<BasePacket> pool = new Stack<BasePacket>();

        public BasePacketPool(SocketInterface s)
        {
            socket = s;
        }

        public void Release(BasePacket stream)
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

        public BasePacket Acquire()
        {
            BasePacket stream = null;

            lock (pool)
            {
                if (pool.Count > 0)
                {
                    stream = pool.Pop();
                }
            }

            if (stream == null)
            {
                stream = new BasePacket(new byte[1500]);
                stream.pool = this;
            }

            NetAssert.True(stream.isPooled);

            stream.isPooled = false;
            stream.Position = 0;
            stream.Size = (1500) << 3;

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
    }
}