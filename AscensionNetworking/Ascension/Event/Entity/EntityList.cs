using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascension.Networking
{
    public class EntityList : IEnumerable<AscensionEntity>
    {
        private readonly List<Entity> _list;

        public int Count
        {
            get { return _list.Count; }
        }

        public EntityList(List<Entity> l)
        {
            _list = l;
        }

        public IEnumerator<AscensionEntity> GetEnumerator()
        {
            foreach (var entity in _list)
            {
                if (entity != null && entity.IsAttached && entity.UnityObject != null)
                {
                    yield return entity.UnityObject;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}