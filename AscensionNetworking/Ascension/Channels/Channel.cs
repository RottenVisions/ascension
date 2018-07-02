using System;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public abstract class Channel
    {
        Connection cn;

        public Connection connection
        {
            get { return cn; }
            set
            {
                if (cn == null)
                {
                    cn = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public abstract void Pack(Packet packet);
        public abstract void Read(Packet packet);

        public virtual void Lost(Packet packet) { }
        public virtual void Delivered(Packet packet) { }

        public virtual void Disconnected() { }
    }
}
