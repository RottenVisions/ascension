using System.Collections.Generic;

namespace Ascension.Networking
{

    public struct Filter
    {

        public class EqualityComparer : IEqualityComparer<Filter>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            EqualityComparer()
            {

            }

            bool IEqualityComparer<Filter>.Equals(Filter a, Filter b)
            {
                return a.Bits == b.Bits;
            }

            int IEqualityComparer<Filter>.GetHashCode(Filter f)
            {
                return f.Bits;
            }
        }

        public readonly int Bits;
        public static string[] Names = new string[32];

        public Filter(int bits)
        {
            Bits = bits;
        }

        public override int GetHashCode()
        {
            return Bits;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            sb.Append("Filter");

            for (int i = 0; i < 32; ++i)
            {
                int b = 1 << i;

                if ((Bits & b) == b)
                {
                    if (Names[i] == null)
                    {
                        sb.Append(" ?" + i);
                    }
                    else
                    {
                        sb.Append(" " + Names[i]);
                    }
                }
            }

            sb.Append("]");
            return sb.ToString();
        }

        public static implicit operator bool (Filter a)
        {
            return a.Bits != 0;
        }

        public static Filter operator &(Filter a, Filter b)
        {
            return new Filter(a.Bits & b.Bits);
        }

        public static Filter operator |(Filter a, Filter b)
        {
            return new Filter(a.Bits | b.Bits);
        }
    }
}
