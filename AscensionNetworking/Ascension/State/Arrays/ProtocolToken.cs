
namespace Ascension.Networking
{
    public class NetworkArray_ProtocolToken : NetworkArray_Values<IMessageRider>
    {
        internal NetworkArray_ProtocolToken(int length, int stride)
            : base(length, stride)
        {
            NetAssert.True(stride == 1);
        }

        protected override IMessageRider GetValue(int index)
        {
            return Storage.Values[index].ProtocolToken;
        }

        protected override bool SetValue(int index, IMessageRider value)
        {
            if (ReferenceEquals(Storage.Values[index].ProtocolToken, value) == false)
            {
                Storage.Values[index].ProtocolToken = value;
                return true;
            }

            return false;
        }
    }
}