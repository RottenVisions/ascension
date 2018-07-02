using System;
using System.Collections.Generic;

namespace Ascension.Networking
{

    public class NetworkArray_Objects<T> : NetworkObj, IEnumerable<T> where T : NetworkObj
    {
        int _length;
        int _stride;

        public int Length
        {
            get { return _length; }
        }

        public override NetworkStorage Storage
        {
            get { return Root.Storage; }
        }

        public NetworkArray_Objects(int length, int stride)
      : base(NetworkArray_Meta.Instance)
        {
            _length = length;
            _stride = stride;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                {
                    throw new IndexOutOfRangeException();
                }

                return (T)Objects[this.OffsetObjects + 1 + (index * _stride)];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _length; ++i)
            {
                yield return this[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}