
namespace Ascension.Networking
{

    public class NetworkArray_Entity : NetworkArray_Values<AscensionEntity>
    {
        public NetworkArray_Entity(int length, int stride)
          : base(length, stride)
        {
            NetAssert.True(stride == 1);
        }

        protected override AscensionEntity GetValue(int index)
        {
            return AscensionNetwork.FindEntity(Storage.Values[index].NetworkId);
        }

        protected override bool SetValue(int index, AscensionEntity value)
        {
            NetworkId newValue = value == null ? new NetworkId() : value.AEntity.NetworkId;

            if (Storage.Values[index].NetworkId != newValue)
            {
                Storage.Values[index].NetworkId = newValue;
                return true;
            }

            return false;
        }
    }
}