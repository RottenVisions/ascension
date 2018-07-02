using System;
using System.Collections;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public class ListExtendedSingular<T> : IEnumerable<T> where T : class, IListNode
    {

        private T head;
        private T tail;
        private int count;

        public int Count
        {
            get { return count; }
        }

        public T First
        {
            get
            {
                VerifyNotEmpty();
                return head;
            }
        }

        public T Last
        {
            get
            {
                VerifyNotEmpty();
                return tail;
            }
        }

        public Iterator<T> GetIterator()
        {
            return new Iterator<T>(head, count);
        }

        public void AddFirst(T item)
        {
            VerifyCanInsert(item);

            if (count == 0)
            {
                head = tail = item;
            }
            else
            {
                item.Next = head;
                head = item;
            }

            item.List = this;
            ++count;
        }

        public void AddLast(T item)
        {
            VerifyCanInsert(item);

            if (count == 0)
            {
                head = tail = item;
            }
            else
            {
                tail.Next = item;
                tail = item;
            }

            item.List = this;
            ++count;
        }

        public T PeekFirst()
        {
            VerifyNotEmpty();
            return head;
        }

        public T RemoveFirst()
        {
            VerifyNotEmpty();

            T result = head;

            if (count == 1)
            {
                head = tail = null;
            }
            else
            {
                head = (T) head.Next;
            }

            --count;
            result.List = null;
            return result;
        }

        public void Clear()
        {
            head = null;
            tail = null;
            count = 0;
        }

        public T Next(T item)
        {
            VerifyInList(item);
            return (T) item.Next;
        }

        public IEnumerator<T> GetEnumerator()
        {
            T current = head;

            while (current != null)
            {
                yield return current;
                current = (T) current.Next;
            }
        }

        private void VerifyNotEmpty()
        {
            if (count == 0)
                throw new InvalidOperationException("List is empty");
        }

        private void VerifyCanInsert(T node)
        {
            if (ReferenceEquals(node.List, null) == false)
            {
                throw new InvalidOperationException("Node is already in a List");
            }
        }

        private void VerifyInList(T node)
        {
            if (ReferenceEquals(node.List, this) == false)
            {
                throw new InvalidOperationException("Node is not in this List");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator bool(ListExtendedSingular<T> List)
        {
            return List != null;
        }
    }
}

