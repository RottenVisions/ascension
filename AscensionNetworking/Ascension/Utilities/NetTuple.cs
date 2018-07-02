using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{
    public struct NetTuple<T0, T1>
    {
        public readonly T0 item0;
        public readonly T1 item1;

        public NetTuple(T0 a, T1 b)
        {
            item0 = a;
            item1 = b;
        }
    }
}
