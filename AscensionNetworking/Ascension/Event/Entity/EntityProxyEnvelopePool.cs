using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ascension.Networking
{
    static class EntityProxyEnvelopePool
    {
        static readonly Stack<EntityProxyEnvelope> Pool = new Stack<EntityProxyEnvelope>();

        public static EntityProxyEnvelope Acquire()
        {
            EntityProxyEnvelope obj;

            if (Pool.Count > 0)
            {
                obj = Pool.Pop();
            }
            else
            {
                obj = new EntityProxyEnvelope();
            }

            return obj;
        }

        public static void Release(EntityProxyEnvelope obj)
        {
            Pool.Push(obj);
        }
    }

}
