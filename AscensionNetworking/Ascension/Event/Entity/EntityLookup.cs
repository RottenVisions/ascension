using System.Collections.Generic;

namespace Ascension.Networking
{
    public class EntityLookup : IEnumerable<AscensionEntity>
    {
        readonly Dictionary<NetworkId, EntityProxy> dict;


        public int Count
        {
            get { return dict.Count; }
        }

        public EntityLookup(Dictionary<NetworkId, EntityProxy> d)
        {
            dict = d;
        }

        public bool TryGet(NetworkId id, out AscensionEntity entity)
        {
            EntityProxy proxy;

            if (dict.TryGetValue(id, out proxy) && proxy.Entity != null && proxy.Entity.UnityObject != null)
            {
                entity = proxy.Entity.UnityObject;
                return true;
            }

            entity = null;
            return false;
        }

        public IEnumerator<AscensionEntity> GetEnumerator()
        {
            foreach (var proxy in dict.Values)
            {
                if (proxy != null && proxy.Entity != null && proxy.Entity.IsAttached && proxy.Entity.UnityObject != null)
                {
                    yield return proxy.Entity.UnityObject;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}