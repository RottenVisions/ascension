using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class EntityProxy : BitSet
    {
        public class PriorityComparer : IComparer<EntityProxy>
        {
            public static readonly PriorityComparer Instance = new PriorityComparer();

            PriorityComparer()
            {

            }

            int IComparer<EntityProxy>.Compare(EntityProxy x, EntityProxy y)
            {
                return y.Priority.CompareTo(x.Priority);
            }
        }

        //public NetworkState State;
        public NetworkId NetworkId;
        public ProxyFlags Flags;
        public Priority[] PropertyPriority;

        public Entity Entity;
        public Connection Connection;
        public Queue<EntityProxyEnvelope> Envelopes;

        public IMessageRider ControlTokenLost;
        public IMessageRider ControlTokenGained;

        public int Skipped;
        public float Priority;

        // ###################

        public EntityProxy()
        {
            Envelopes = new Queue<EntityProxyEnvelope>();
        }

        public EntityProxyEnvelope CreateEnvelope()
        {
            EntityProxyEnvelope env = EntityProxyEnvelopePool.Acquire();
            env.Proxy = this;
            env.Flags = this.Flags;
            env.ControlTokenLost = this.ControlTokenLost;
            env.ControlTokenGained = this.ControlTokenGained;
            return env;
        }

        public override string ToString()
        {
            return string.Format("[Proxy {0} {1}]", NetworkId, ((object)Entity) ?? ((object)"NULL"));
        }
    }

}
