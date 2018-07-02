using System;
using UnityEngine;

namespace Ascension.Networking
{

    public class NetworkArray_PrefabId : NetworkArray_Values<PrefabId>
    {
        public NetworkArray_PrefabId(int length, int stride)
          : base(length, stride)
        {
            NetAssert.True(stride == 1);
        }

        protected override PrefabId GetValue(int index)
        {
            return Storage.Values[index].PrefabId;
        }

        protected override bool SetValue(int index, PrefabId value)
        {
            if (Storage.Values[index].PrefabId != value)
            {
                Storage.Values[index].PrefabId = value;
                return true;
            }

            return false;
        }
    }
}