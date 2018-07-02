using System.Collections.Generic;

namespace Ascension.Networking
{
    public struct Priority
    {
        public class Comparer : IComparer<Priority>
        {
            public static readonly Comparer Instance = new Comparer();

            Comparer()
            {

            }

            int IComparer<Priority>.Compare(Priority x, Priority y)
            {
                return y.PropertyPriority.CompareTo(x.PropertyPriority);
            }
        }

        public int PropertyIndex;
        public int PropertyPriority;

//#if DEBUG
        public int PropertyUpdated;
//#endif
    }
}
