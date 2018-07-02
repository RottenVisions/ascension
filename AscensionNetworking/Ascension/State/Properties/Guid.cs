using System;

namespace Ascension.Networking
{
    public class NetworkProperty_Guid : NetworkProperty
    {
        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Guid;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            var v = (Guid)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Guid, v))
            {
                obj.Storage.Values[obj[this]].Guid = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Guid;
        }

        public override int BitCount(NetworkObj obj)
        {
            return 128;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            packet.WriteGuid(storage.Values[obj[this]].Guid);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            storage.Values[obj[this]].Guid = packet.ReadGuid();
        }
    }
}
