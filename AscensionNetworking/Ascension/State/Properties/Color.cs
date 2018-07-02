using Ascension.Networking.Sockets;
using UnityEngine;

namespace Ascension.Networking
{
    public class NetworkProperty_Color : NetworkProperty
    {
        public override int BitCount(NetworkObj obj)
        {
            return 32;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            var v = (Color)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Color, v))
            {
                obj.Storage.Values[obj[this]].Color = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Color;
        }

        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Color;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            packet.WriteColorRGBA(storage.Values[obj[this]].Color);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            storage.Values[obj[this]].Color = packet.ReadColorRGBA();
        }
    }
}
