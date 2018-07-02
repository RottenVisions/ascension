using System;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public abstract class NetworkArray_Values<T> : NetworkObj, IEnumerable<T>
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

        public NetworkArray_Values(int length, int stride)
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

                return GetValue(this.OffsetStorage + index);
            }
            set
            {
                if (index < 0 || index >= _length)
                {
                    throw new IndexOutOfRangeException();
                }

                if (SetValue(this.OffsetStorage + (index * _stride), value))
                {
                    Storage.PropertyChanged(this.OffsetProperties + index);
                }
            }
        }

        protected abstract T GetValue(int index);
        protected abstract bool SetValue(int index, T value);

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
