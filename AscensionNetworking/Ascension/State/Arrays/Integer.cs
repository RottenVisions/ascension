using System;
using UnityEngine;

namespace Ascension.Networking
{

    public class NetworkArray_Integer : NetworkArray_Values<Int32>
    {
        public NetworkArray_Integer(int length, int stride)
          : base(length, stride)
        {
            NetAssert.True(stride == 1);
        }

        protected override Int32 GetValue(int index)
        {
            return Storage.Values[index].Int0;
        }

        protected override bool SetValue(int index, Int32 value)
        {
            if (Storage.Values[index].Int0 != value)
            {
                Storage.Values[index].Int0 = value;
                return true;
            }

            return false;
        }
    }
}