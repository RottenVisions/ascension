using System;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class NetworkProperty_Float : NetworkProperty_Mecanim
    {
        PropertyFloatSettings Settings;
        PropertyFloatCompressionSettings Compression;

        public override bool WantsOnSimulateBefore
        {
            get { return Interpolation.Enabled; }
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
                var value = NetMath.InterpolateFloat(obj.RootState.Frames, idx + 1, obj.RootState.Entity.Frame, Settings.IsAngle);

                while (it.Next())
                {
                    it.val.Values[idx].Float0 = value;
                }
            }
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            if (MecanimDirection == MecanimDirection.UsingAnimatorMethods)
            {
                NetLog.Error("Can't call SetDynamic on a float in 'UsingAnimatorMethods' mode");
                return;
            }

            var v = (float)value;

            if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Float0, v))
            {
                obj.Storage.Values[obj[this]].Float0 = v;
                obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
            }
        }

        public override object GetDynamic(NetworkObj obj)
        {
            return obj.Storage.Values[obj[this]].Float0;
        }

        public void Settings_Float(PropertyFloatCompressionSettings compression)
        {
            Compression = compression;
        }

        public void Settings_Float(PropertyFloatSettings settings)
        {
            Settings = settings;
        }

        public override int BitCount(NetworkObj obj)
        {
            return Compression.BitsRequired;
        }

        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return storage.Values[obj[this]].Float0;
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            Compression.Pack(packet, storage.Values[obj[this]].Float0);
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            if (Interpolation.Enabled)
            {
                storage.Values[obj[this] + 1].Float1 = Compression.Read(packet);
            }
            else
            {
                storage.Values[obj[this]].Float0 = Compression.Read(packet);
            }
        }

        public override void PullMecanimValue(NetworkState state)
        {
            if (state.Animator == null)
            {
                return;
            }

            float newValue = state.Animator.GetFloat(PropertyName);
            float oldValue = state.Storage.Values[state[this]].Float0;

            state.Storage.Values[state[this]].Float0 = newValue;

            if (NetworkValue.Diff(newValue, oldValue))
            {
                state.Storage.PropertyChanged(state.OffsetProperties + this.OffsetProperties);
            }
        }

        public override void PushMecanimValue(NetworkState state)
        {
            for (int i = 0; i < state.Animators.Count; ++i)
            {
                state.Animators[i].SetFloat(PropertyName, state.Storage.Values[state[this]].Float0, MecanimDamping, AscensionNetwork.FrameDeltaTime);
            }
        }
    }
}
