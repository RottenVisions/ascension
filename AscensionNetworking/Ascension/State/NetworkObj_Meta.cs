using System.Linq;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public abstract class NetworkObj_Meta
    {
        static readonly int[] ZeroIndices = new int[0];
        static readonly string[] ZeroPaths = new string[0];

        public struct Offsets
        {
            public int OffsetStorage;
            public int OffsetObjects;
            public int OffsetProperties;

            public Offsets(int properties, int storage, int objects)
            {
                OffsetStorage = storage;
                OffsetObjects = objects;
                OffsetProperties = properties;
            }
        }

        public TypeId TypeId;
        public BitSet[] Filters;
        public Priority[] PropertiesTempPriority;
        public NetworkPropertyInfo[] Properties;

        public HashSet<string> CallbackPaths = new HashSet<string>();
        public Stack<NetworkStorage> StoragePool = new Stack<NetworkStorage>();
        public List<NetworkPropertyInfo> OnRender = new List<NetworkPropertyInfo>();
        public List<NetworkPropertyInfo> OnSimulateAfter = new List<NetworkPropertyInfo>();
        public List<NetworkPropertyInfo> OnSimulateBefore = new List<NetworkPropertyInfo>();
        public List<NetworkPropertyInfo> OnControlGainedLost = new List<NetworkPropertyInfo>();
        public List<NetworkPropertyInfo> OnFrameCloned = new List<NetworkPropertyInfo>();

        public int CountObjects;
        public int CountStorage;
        public int CountProperties;

        public NetworkObj_Meta()
        {
            Filters = new BitSet[32];
            Filters[31] = new BitSet();
            Filters[30] = new BitSet();
        }


        public NetworkStorage AllocateStorage()
        {
            if (StoragePool.Count > 0)
            {
                return StoragePool.Pop();
            }

            return new NetworkStorage(CountStorage);
        }

        public void FreeStorage(NetworkStorage storage)
        {
            storage.Frame = 0;
            storage.Root = null;
            storage.ClearAll();

            System.Array.Clear(storage.Values, 0, storage.Values.Length);

            StoragePool.Push(storage);
        }

        void AddPropertyToArray(int offsetProperties, int offsetObjects, NetworkProperty property)
        {
            NetAssert.Null(Properties[offsetProperties].Property);

            if (offsetProperties > 0)
            {
                NetAssert.NotNull(Properties[offsetProperties - 1].Property);
            }

            Properties[offsetProperties].Property = property;
            Properties[offsetProperties].OffsetObjects = offsetObjects;

            for (int i = 0; i < 32; ++i)
            {
                int f = 1 << i;

                // this can't be set
                if (Filters[i] != null)
                {
                    NetAssert.False(Filters[i].IsSet(offsetProperties));
                }

                // if property is included in this filter, flag it
                if ((property.PropertyFilters & f) == f)
                {

                    if (Filters[i] == null)
                    {
                        Filters[i] = new BitSet();
                    }

                    Filters[i].Set(offsetProperties);

                    // now it must be set
                    NetAssert.True(Filters[i].IsSet(offsetProperties));
                }
            }
        }

        public void AddProperty(int offsetProperties, int offsetObjects, NetworkProperty property, int arrayIndex)
        {
            AddPropertyToArray(offsetProperties, offsetObjects, property);

            Properties[offsetProperties].Paths =
              property.AllowCallbacks
              ? new string[] { property.PropertyName }
              : ZeroPaths;

            Properties[offsetProperties].Indices =
              (arrayIndex >= 0) && property.AllowCallbacks
              ? new int[] { arrayIndex }
              : ZeroIndices;

            CallbackPaths.Add(property.PropertyName);
        }

        void AddCopiedProperty(int offsetProperties, int offsetObjects, NetworkPropertyInfo property, string prefix, int arrayIndex)
        {
            AddPropertyToArray(offsetProperties, offsetObjects, property.Property);

            Properties[offsetProperties].Indices =
              (arrayIndex >= 0) && property.Property.AllowCallbacks
              ? property.Indices.AddFirst(arrayIndex)
              : ZeroIndices;

            Properties[offsetProperties].Paths =
              property.Property.AllowCallbacks
              ? property.Paths.Select(x => prefix + "." + x).ToArray().AddFirst(prefix)
              : ZeroPaths;

            if (property.Property.AllowCallbacks)
            {
                for (int i = 0; i < Properties[offsetProperties].Paths.Length; ++i)
                {
                    CallbackPaths.Add(Properties[offsetProperties].Paths[i]);
                }
            }
        }

        public void CopyProperties(int offsetProperties, int offsetObjects, NetworkObj_Meta meta, string prefix, int arrayIndex)
        {
            for (int i = 0; i < meta.Properties.Length; ++i)
            {
                AddCopiedProperty(offsetProperties + i, offsetObjects + meta.Properties[i].OffsetObjects, meta.Properties[i], prefix, arrayIndex);
            }
        }

        public abstract void InitObject(NetworkObj obj, Offsets offsets);

        public virtual void InitMeta()
        {
            for (int i = 0; i < Properties.Length; ++i)
            {
                if (Properties[i].Property.WantsOnRender)
                {
                    OnRender.Add(Properties[i]);
                }

                if (Properties[i].Property.WantsOnSimulateBefore)
                {
                    OnSimulateBefore.Add(Properties[i]);
                }

                if (Properties[i].Property.WantsOnSimulateAfter)
                {
                    OnSimulateAfter.Add(Properties[i]);
                }

                if (Properties[i].Property.WantsOnControlGainedLost)
                {
                    OnControlGainedLost.Add(Properties[i]);
                }

                if (Properties[i].Property.WantsOnFrameCloned)
                {
                    OnFrameCloned.Add(Properties[i]);
                }
            }

            PropertiesTempPriority = new Priority[Properties.Length];
        }

        public void InitObject(NetworkObj obj, NetworkObj root, Offsets offsets)
        {
            obj.Root = root;

            obj.OffsetStorage = offsets.OffsetStorage;
            obj.OffsetObjects = offsets.OffsetObjects;
            obj.OffsetProperties = offsets.OffsetProperties;

            obj.Add();

            InitObject(obj, offsets);
        }
    }
}
