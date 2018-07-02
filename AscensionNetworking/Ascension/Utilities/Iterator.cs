namespace Ascension.Networking
{
    public struct Iterator<T> where T : class, IListNode
    {
        T node;
        int count;
        int number;

        public T val;

        public Iterator(T node, int count)
        {
            this.node = node;
            this.count = count;
            number = 0;
            val = default(T);
        }

        public bool Next()
        {
            return Next(out val);
        }

        public bool Next(out T item)
        {
            if (number < count)
            {
                item = node;

                node = (T)node.Next;
                number += 1;

                return true;
            }

            item = default(T);
            return false;
        }
    }
}
