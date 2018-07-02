using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascension.Compiler
{
    public static class Utils
    {
        public static string ToStringSigned(this float f)
        {
            if (f < 0)
            {
                return f.ToString();
            }

            return "+" + f;
        }

        public static string ToStringSigned(this int i)
        {
            if (i < 0)
            {
                return i.ToString();
            }

            return "+" + i;
        }

        public static string Join<T>(this IEnumerable<T> items, string seperator)
        {
            return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
        }

        public static T[] Add<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[1] {item};
            }

            T[] clone = new T[array.Length + 1];
            array.CopyTo(clone, 0);
            clone[array.Length] = item;
            return clone;
        }
    }
}