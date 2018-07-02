
using System;
using UnityEngine;

namespace Ascension.Networking
{

    public class NetworkArray_Vector : NetworkArray_Values<Vector3>
    {
        public NetworkArray_Vector(int length, int stride)
          : base(length, stride)
        {
            NetAssert.True((stride == 1) || (stride == 2));
        }

        protected override Vector3 GetValue(int index)
        {
            return Storage.Values[index].Vector3;
        }

        protected override bool SetValue(int index, Vector3 value)
        {
            if (Storage.Values[index].Vector3 != value)
            {
                Storage.Values[index].Vector3 = value;
                return true;
            }

            return false;
        }
    }
}