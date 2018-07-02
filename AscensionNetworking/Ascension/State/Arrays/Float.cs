using System;
using UnityEngine;

namespace Ascension.Networking
{

    public class NetworkArray_Float : NetworkArray_Values<Single>
    {
        public NetworkArray_Float(int length, int stride)
          : base(length, stride)
        {
            NetAssert.True((stride == 1) || (stride == 2));
        }

        protected override Single GetValue(int index)
        {
            return Storage.Values[index].Float0;
        }

        protected override bool SetValue(int index, Single value)
        {
            if (Storage.Values[index].Float0 != value)
            {
                Storage.Values[index].Float0 = value;
                return true;
            }

            return false;
        }
    }
}