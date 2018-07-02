using System;
using UnityEngine;

namespace Ascension.Networking
{

    public class NetworkArray_Transform : NetworkArray_Values<NetworkTransform>
    {
        public new NetworkTransform this[int index]
        {
            get { return base[index]; }
        }

        public NetworkArray_Transform(int length, int stride)
          : base(length, stride)
        {
            NetAssert.True(stride == 3);
        }

        protected override NetworkTransform GetValue(int index)
        {
            return Storage.Values[index].Transform;
        }

        protected override bool SetValue(int index, NetworkTransform value)
        {
            throw new NotSupportedException();
        }
    }
}