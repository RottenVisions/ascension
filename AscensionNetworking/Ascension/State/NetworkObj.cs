using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ascension.Networking
{
    public abstract class NetworkObj_Root : NetworkObj
    {
        public NetworkObj_Root(NetworkObj_Meta meta)
          : base(meta)
        {
            InitRoot();
        }
    }

    public abstract class NetworkObj : IDisposable
    {
        public String Path;
        public NetworkObj Root;
        public List<NetworkObj> RootObjects;
        public readonly NetworkObj_Meta Meta;

        public int OffsetObjects;
        public int OffsetStorage;
        public int OffsetProperties;

        public NetworkState RootState
        {
            get { return (NetworkState)Root; }
        }

        public void Add()
        {
            NetAssert.True(OffsetObjects == Objects.Count);
            Objects.Add(this);
        }

        public List<NetworkObj> Objects
        {
            get { return Root.RootObjects; }
        }

        public virtual NetworkStorage Storage
        {
            get { return Root.Storage; }
        }

        public NetworkObj(NetworkObj_Meta meta)
        {
            Meta = meta;
        }

        public void InitRoot()
        {
            RootObjects = new List<NetworkObj>(Meta.CountObjects);

            Path = null;
            Meta.InitObject(this, this, new NetworkObj_Meta.Offsets());

            NetAssert.True(RootObjects.Count == Meta.CountObjects, "RootObjects.Count == Meta.CountObjects");
        }

        public void Init(string path, NetworkObj parent, NetworkObj_Meta.Offsets offsets)
        {
            Path = path;
            Meta.InitObject(this, parent, offsets);
        }


        public NetworkStorage AllocateStorage()
        {
            return Meta.AllocateStorage();
        }

        public NetworkStorage DuplicateStorage(NetworkStorage s)
        {
            NetworkStorage c;

            c = AllocateStorage();
            c.Root = s.Root;
            c.Frame = s.Frame;

            Array.Copy(s.Values, 0, c.Values, 0, s.Values.Length);

            return c;
        }

        public void FreeStorage(NetworkStorage storage)
        {
            Meta.FreeStorage(storage);
        }

        public int this[NetworkProperty property]
        {
            get
            {
#if DEBUG
                NetAssert.NotNull(property);

                NetAssert.True(OffsetObjects >= 0);
                NetAssert.True(OffsetObjects < Root.Meta.CountObjects);

                NetAssert.Same(Root.Objects[OffsetObjects], this);
                NetAssert.Same(Root.Objects[OffsetObjects].Meta, property.PropertyMeta);
                NetAssert.Same(Root.Meta.Properties[Root.Objects[OffsetObjects].OffsetProperties + property.OffsetProperties].Property, property);
#endif

                return this.OffsetStorage + property.OffsetStorage;
            }
        }

        void IDisposable.Dispose()
        {
        }
    }
}
