using Ascension.Networking.Sockets;
using UnityEngine;

namespace Ascension.Networking
{
    public class NetworkProperty_Color32 : NetworkProperty
    {
        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Color32;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            var v = (Color32)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Color32, v))
            {
                obj.Storage.Values[obj[this]].Color32 = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Color32;
        }

        public override int BitCount(NetworkObj obj)
        {
            return 128;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            packet.WriteColor32RGBA(storage.Values[obj[this]].Color32);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            storage.Values[obj[this]].Color32 = packet.ReadColor32RGBA();
        }
    }
}
