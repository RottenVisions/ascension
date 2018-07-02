using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class NetworkProperty_Bool : NetworkProperty_Mecanim
    {
        public override int BitCount(NetworkObj obj)
        {
            return 1;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            if (MecanimDirection == MecanimDirection.UsingAnimatorMethods)
            {
                NetLog.Error("Can't call SetDynamic on a bool in 'UsingAnimatorMethods' mode");
                return;
            }

            var v = (bool)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Bool, v))
            {
                obj.Storage.Values[obj[this]].Bool = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Bool;
        }

        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Bool;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            packet.WriteBool(storage.Values[obj[this]].Bool);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            storage.Values[obj[this]].Bool = packet.ReadBool();
        }

        public override void PullMecanimValue(NetworkState state)
        {
            if (state.Animator == null)
            {
                return;
            }

            bool newValue = state.Animator.GetBool(PropertyName);
            bool oldValue = state.Storage.Values[state[this]].Bool;

            state.Storage.Values[state[this]].Bool = newValue;

            if (NetworkValue.Diff(newValue, oldValue))
            {
                state.Storage.PropertyChanged(state.OffsetProperties + this.OffsetProperties);
            }
        }

        public override void PushMecanimValue(NetworkState state)
        {
            for (int i = 0; i < state.Animators.Count; ++i)
            {
                state.Animators[i].SetBool(PropertyName, state.Storage.Values[state[this]].Bool);
            }
        }
    }
}
