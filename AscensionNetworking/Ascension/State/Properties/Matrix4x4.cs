using Ascension.Networking.Sockets;
using UnityEngine;

namespace Ascension.Networking
{
    public class NetworkProperty_Matrix4x4 : NetworkProperty
    {
        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Matrix4x4;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            var v = (Matrix4x4)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Matrix4x4, v))
            {
                obj.Storage.Values[obj[this]].Matrix4x4 = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Matrix4x4;
        }

        public override int BitCount(NetworkObj obj)
        {
            return 512;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            packet.WriteMatrix4x4(storage.Values[obj[this]].Matrix4x4);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            storage.Values[obj[this]].Matrix4x4 = packet.ReadMatrix4x4();
        }
    }
}
