using System;
using Ascension.Networking.Sockets;
using UnityEngine;

namespace Ascension.Networking
{
    public class NetworkProperty_Quaternion : NetworkProperty
    {
        PropertyQuaternionCompression Compression;

        public override bool WantsOnSimulateBefore
        {
            get
            {
                return Interpolation.Enabled;
            }
        }

        public void Settings_Quaternion(PropertyFloatCompressionSettings compression, bool strict)
        {
            Compression = PropertyQuaternionCompression.Create(compression, strict);
        }

        public void Settings_QuaternionEuler(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict)
        {
            Compression = PropertyQuaternionCompression.Create(PropertyVectorCompressionSettings.Create(x, y, z, strict));
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            var v = (Quaternion)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Quaternion, v))
            {
                obj.Storage.Values[obj[this]].Quaternion = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Quaternion;
        }

        public override void OnInit(NetworkObj obj)
        {
            obj.Storage.Values[obj[this]].Quaternion = Quaternion.identity;
        }

        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Quaternion;
        }

        public override int BitCount(NetworkObj obj)
        {
            return Compression.BitsRequired;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            Compression.Pack(packet, storage.Values[obj[this]].Quaternion);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            Quaternion q = Compression.Read(packet);

            if (Interpolation.Enabled)
            {
                storage.Values[obj[this] + 1].Quaternion = q;
            }
            else
            {
                storage.Values[obj[this]].Quaternion = q;
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
                var value = NetMath.InterpolateQuaternion(obj.RootState.Frames, idx + 1, obj.RootState.Entity.Frame);

                while (it.Next())
                {
                    it.val.Values[idx].Quaternion = value;
                }
            }
        }
    }
}
