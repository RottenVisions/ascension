using System;
using Ascension.Networking.Sockets;
using UE = UnityEngine;

namespace Ascension.Networking
{
    public abstract class NetworkProperty
    {
        public Int32 OffsetStorage;
        public Int32 OffsetProperties;

        public int PropertyFilters;
        public int PropertyNameHash;
        public String PropertyName;
        public Int32 PropertyPriority;
        public NetworkObj_Meta PropertyMeta;

        public bool ToProxies;
        public bool ToController;

        public PropertyInterpolationSettings Interpolation;

        public virtual bool AllowCallbacks { get { return true; } }

        public virtual bool WantsOnRender { get { return false; } }
        public virtual bool WantsOnSimulateAfter { get { return false; } }
        public virtual bool WantsOnSimulateBefore { get { return false; } }
        public virtual bool WantsOnControlGainedLost { get { return false; } }
        public virtual bool WantsOnFrameCloned { get { return false; } }

        public void Settings_Property(string name, int priority, int filters)
        {
            PropertyName = name;
            PropertyNameHash = name.GetHashCode();
            PropertyFilters = filters;
            PropertyPriority = UE.Mathf.Clamp(priority, 1, 100);

            ToProxies = (filters & (1 << 30)) == (1 << 30);
            ToController = (filters & (1 << 31)) == (1 << 31);
        }

        public void Settings_Offsets(Int32 properties, Int32 storage)
        {
            OffsetStorage = storage;
            OffsetProperties = properties;
        }
        public void Settings_Interpolation(float snapMagnitude, bool enabled)
        {
            Interpolation.Enabled = enabled;
            Interpolation.SnapMagnitude = snapMagnitude;
        }

        public abstract bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet);
        public abstract void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet);

        public virtual object DebugValue(NetworkObj obj, NetworkStorage storage) { return "NONE"; }
        public virtual int BitCount(NetworkObj obj) { return -1; }
        public virtual void SetDynamic(NetworkObj obj, object value) { throw new NotSupportedException(); }
        public virtual object GetDynamic(NetworkObj obj) { throw new NotSupportedException(); }

        public virtual void OnInit(NetworkObj obj) { }
        public virtual void OnRender(NetworkObj obj) { }
        public virtual void OnSimulateBefore(NetworkObj obj) { }
        public virtual void OnSimulateAfter(NetworkObj obj) { }
        public virtual void OnParentChanged(NetworkObj obj, Entity newParent, Entity oldParent) { }
        public virtual void OnFrameCloned(NetworkObj obj, NetworkStorage storage) { }

        public virtual void OnControlGained(NetworkObj obj) { }
        public virtual void OnControlLost(NetworkObj obj) { }

        public virtual void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t) { }
    }

}
