using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public class EntityProxyEnvelope : NetObject, IDisposable
    {
        public int PacketNumber;
        public ProxyFlags Flags;
        public EntityProxy Proxy = null;
        public List<Priority> Written = new List<Priority>();

        public IMessageRider ControlTokenLost;
        public IMessageRider ControlTokenGained;

        public void Dispose()
        {
            Proxy = null;
            Flags = ProxyFlags.ZERO;

            Written.Clear();

            EntityProxyEnvelopePool.Release(this);
        }
    }

}
