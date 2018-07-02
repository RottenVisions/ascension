using System;
using System.Collections;
using System.Collections.Generic;

namespace Ascension.Networking
{

    public class ListExtended<T> : IEnumerable<T> where T : class, IListNode
    {
        private T first;
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
                return first;
            }
        }

        public T FirstOrDefault
        {
            get
            {
                if (count > 0)
                {
                    return First;
                }

                return default(T);
            }
        }

        public T Last
        {
            get
            {
                VerifyNotEmpty();
                return (T) first.Prev;
            }
        }

        public T LastOrDefault
        {
            get
            {
                if (count > 0)
                {
                    return Last;
                }

                return default(T);
            }
        }

        public Iterator<T> GetIterator()
        {
            return new Iterator<T>(first, count);
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                {
                    throw new IndexOutOfRangeException(index.ToString());
                }

                T val = First;

                while (index-- > 0)
                {
                    val = Next(val);
                }

                return val;
            }
        }

        public bool Contains(T node)
        {
            return (ReferenceEquals(node, null) == false) && (ReferenceEquals(this, node.List) == true);
        }

        public bool IsFirst(T node)
        {
            VerifyInList(node);

            if (count == 0)
                return false;

            return ReferenceEquals(node, first);
        }

        public void AddLast(T node)
        {
            VerifyCanInsert(node);

            if (count == 0)
            {
                InsertEmpty(node);
            }
            else
            {
                InsertBefore(node, first);
            }
        }

        public void AddFirst(T node)
        {
            VerifyCanInsert(node);

            if (count == 0)
            {
                InsertEmpty(node);
            }
            else
            {
                InsertBefore(node, first);
                first = node;
            }
        }

        public T Remove(T node)
        {
            VerifyInList(node);
            VerifyNotEmpty();
            RemoveNode(node);
            return node;
        }

        public T RemoveFirst()
        {
            return Remove(first);
        }

        public T RemoveLast()
        {
            return Remove((T) first.Prev);
        }

        public void Clear()
        {
            first = null;
            count = 0;
        }

        public T Prev(T node)
        {
            VerifyInList(node);
            return (T) node.Prev;
        }

        public T Next(T node)
        {
            VerifyInList(node);
            return (T) node.Next;
        }

        public void Replace(T node, T newNode)
        {
            VerifyInList(node);
            VerifyCanInsert(newNode);

            // setup new node
            newNode.List = this;
            newNode.Next = node.Next;
            newNode.Prev = node.Prev;

            T Next = (T) newNode.Next;
            T Prev = (T) newNode.Prev;

            Next.Prev = newNode;
            Prev.Next = newNode;

            // if this node is the "first" node, then replace
            if (ReferenceEquals(first, node))
            {
                first = newNode;
            }

            // clean up old node
            node.List = null;
            node.Prev = null;
            node.Next = null;
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

        private void InsertBefore(T node, T before)
        {
            node.Next = before;
            node.Prev = before.Prev;

            T Prev = (T) before.Prev;
            Prev.Next = (T) node;

            before.Prev = node;

            node.List = this;
            ++count;
        }

        private void InsertEmpty(T node)
        {
            first = node;
            first.Next = node;
            first.Prev = node;

            node.List = this;
            ++count;
        }

        private void RemoveNode(T node)
        {
            if (count == 1)
            {
                first = null;
            }
            else
            {
                T Next = (T) node.Next;
                T Prev = (T) node.Prev;

                Next.Prev = node.Prev;
                Prev.Next = node.Next;

                if (ReferenceEquals(first, node))
                {
                    first = (T) node.Next;
                }
            }

            node.List = null;
            --count;
        }

        private void VerifyNotEmpty()
        {
            if (count == 0)
                throw new InvalidOperationException("List is empty");
        }

        public IEnumerator<T> GetEnumerator()
        {
            T n = first;
            int c = Count;

            while (c > 0)
            {
                yield return n;
                n = (T) n.Next;
                c = c - 1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator bool(ListExtended<T> List)
        {
            return List != null;
        }
    }
}
