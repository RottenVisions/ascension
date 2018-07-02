using System;

namespace Ascension.Networking
{
    public class RingBuffer<T> : System.Collections.Generic.IEnumerable<T>
    {
        int head;
        int tail;
        int count;
        bool autofree;

        readonly T[] array;

        public bool Full
        {
            get { return count == array.Length; }
        }

        public bool Empty
        {
            get { return count == 0; }
        }

        public bool Autofree
        {
            get { return autofree; }
            set { autofree = value; }
        }

        public int Count
        {
            get { return count; }
        }

        public T Last
        {
            get
            {
                VerifyNotEmpty();
                return this[Count - 1];
            }
            set
            {
                VerifyNotEmpty();
                this[Count - 1] = value;
            }
        }

        public T First
        {
            get
            {
                VerifyNotEmpty();
                return this[0];
            }
            set
            {
                VerifyNotEmpty();
                this[0] = value;
            }
        }

        public T this[int index]
        {
            get
            {
                VerifyNotEmpty();
                return array[(tail + index) % array.Length];
            }
            set
            {
                if (index >= count)
                {
                    throw new IndexOutOfRangeException("Can't change value of non-existand index");
                }

                array[(tail + index) % array.Length] = value;
            }
        }

        public RingBuffer(int size)
        {
            array = new T[size];
        }

        public void Enqueue(T item)
        {
            if (count == array.Length)
            {
                if (autofree)
                {
                    Dequeue();
                }
                else
                {
                    throw new InvalidOperationException("Buffer is full");
                }
            }

            array[head] = item;
            head = (head + 1) % array.Length;
            count += 1;
        }

        public T Dequeue()
        {
            VerifyNotEmpty();
            T item = array[tail];
            array[tail] = default(T);
            tail = (tail + 1) % array.Length;
            count -= 1;
            return item;
        }

        public T Peek()
        {
            VerifyNotEmpty();
            return array[tail];
        }

        public void Clear()
        {
            Array.Clear(array, 0, array.Length);
            count = tail = head = 0;
        }

        public void CopyTo(RingBuffer<T> other)
        {
            if (this.array.Length != other.array.Length)
            {
                throw new InvalidOperationException("Buffers must be of the same capacity");
            }

            other.head = this.head;
            other.tail = this.tail;
            other.count = this.count;

            Array.Copy(this.array, 0, other.array, 0, this.array.Length);
        }

        void VerifyNotEmpty()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; ++i)
            {
                yield return this[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

}
