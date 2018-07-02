using System;
using Ascension.Networking.Sockets;
using UE = UnityEngine;

namespace Ascension.Networking
{
    public class NetworkProperty_Vector : NetworkProperty
    {
        PropertyVectorCompressionSettings Compression;

        public override bool WantsOnSimulateBefore
        {
            get { return Interpolation.Enabled; }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Vector3;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            var v = (UE.Vector3)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Vector3, v))
            {
                obj.Storage.Values[obj[this]].Vector3 = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public void Settings_Vector(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict)
        {
            Compression = PropertyVectorCompressionSettings.Create(x, y, z, strict);
        }

        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Vector3;
        }

        public override int BitCount(NetworkObj obj)
        {
            return Compression.BitsRequired;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            Compression.Pack(packet, storage.Values[obj[this]].Vector3);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            UE.Vector3 v = Compression.Read(packet);

            if (Interpolation.Enabled && (obj.Root is NetworkState))
            {
                storage.Values[obj[this] + 1].Vector3 = v;
            }
            else
            {
                storage.Values[obj[this]].Vector3 = v;
            }
        }

        public override void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t)
        {
            if (Interpolation.Enabled)
            {
                var v0 = from.Values[obj[this]].Vector3;
                var v1 = to.Values[obj[this]].Vector3;
                var m = (v1 - v0).sqrMagnitude;

                if (m < (Interpolation.SnapMagnitude * Interpolation.SnapMagnitude))
                {
                    storage.Values[obj[this]].Vector3 = UE.Vector3.Lerp(v0, v1, t);
                }
                else
                {
                    storage.Values[obj[this]].Vector3 = v1;
                }
            }
            else
            {
                storage.Values[obj[this]].Vector3 = to.Values[obj[this]].Vector3;
            }
        }

        public override void OnSimulateBefore(NetworkObj obj)
        {
            if (Interpolation.Enabled)
            {
                var root = (NetworkState)obj.Root;

                if (root.Entity.IsOwner)
                {
                    return;
                }

                if (root.Entity.HasControl && !ToController)
                {
                    return;
                }

                var it = root.Frames.GetIterator();
                var idx = obj[this];
                var value = NetMath.InterpolateVector(obj.RootState.Frames, idx + 1, obj.RootState.Entity.Frame, Interpolation.SnapMagnitude);

                while (it.Next())
                {
                    it.val.Values[idx].Vector3 = value;
                }
            }
        }
    }
}
