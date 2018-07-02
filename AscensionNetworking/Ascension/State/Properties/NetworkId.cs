
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class NetworkProperty_NetworkId : NetworkProperty
    {
        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].NetworkId;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            var v = (NetworkId)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].NetworkId, v))
            {
                obj.Storage.Values[obj[this]].NetworkId = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].NetworkId;
        }

        public override int BitCount(NetworkObj obj)
        {
            return 64;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            packet.WriteNetworkId(storage.Values[obj[this]].NetworkId);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            storage.Values[obj[this]].NetworkId = packet.ReadNetworkId();
        }
    }
}
