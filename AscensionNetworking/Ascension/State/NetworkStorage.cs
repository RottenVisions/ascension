using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascension.Networking
{
    public class NetworkStorage : BitSet, IListNode
    {
        public int Frame;
        public NetworkObj Root;
        public NetworkValue[] Values;

        public NetworkStorage(int size)
        {
            Values = new NetworkValue[size];
        }

        public void PropertyChanged(int property)
        {
            Set(property);
        }

        object IListNode.Prev
        {
            get;
            set;
        }

        object IListNode.Next
        {
            get;
            set;
        }

        object IListNode.List
        {
            get;
            set;
        }
    }
}
