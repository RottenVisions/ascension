using System;
using UnityEngine;

namespace Ascension.Networking
{

    public class NetworkArray_String : NetworkArray_Values<String>
    {
        public NetworkArray_String(int length, int stride)
          : base(length, stride)
        {
            NetAssert.True(stride == 1);
        }

        protected override String GetValue(int index)
        {
            return Storage.Values[index].String;
        }

        protected override bool SetValue(int index, String value)
        {
            if (Storage.Values[index].String != value)
            {
                Storage.Values[index].String = value;
                return true;
            }

            return false;
        }
    }
}