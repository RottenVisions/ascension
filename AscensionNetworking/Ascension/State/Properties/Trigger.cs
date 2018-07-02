using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class NetworkProperty_Trigger : NetworkProperty_Mecanim
    {
        public override object DebugValue(NetworkObj obj, NetworkStorage storage)
        {
            return "TRIGGER";
        }

        public override bool AllowCallbacks
        {
            get { return false; }
        }

        public override bool WantsOnFrameCloned
        {
            get { return true; }
        }

        public override int BitCount(NetworkObj obj)
        {
            return obj.RootState.Entity.SendRate;
        }

        public override void SetDynamic(NetworkObj obj, object value)
        {
            if (MecanimDirection == MecanimDirection.UsingAnimatorMethods)
            {
                NetLog.Error("Can't call SetDynamic on a trigger in 'UsingAnimatorMethods' mode");
                return;
            }

            obj.Storage.Values[obj[this]].TriggerLocal.Update(Core.Frame, true);
        }

        public override bool Write(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            // adjust trigger
            storage.Values[obj[this]].TriggerSend.Update(Core.Frame, false);

            // write history
            packet.WriteInt(storage.Values[obj[this]].TriggerSend.History, obj.RootState.Entity.SendRate);

            // this always succeeds!
            return true;
        }

        public override void Read(Connection connection, NetworkObj obj, NetworkStorage storage, Packet packet)
        {
            storage.Values[obj[this]].TriggerLocal.Frame = storage.Frame;
            storage.Values[obj[this]].TriggerLocal.History = packet.ReadInt(obj.RootState.Entity.SendRate);
        }

        public override void OnSimulateAfter(NetworkObj obj)
        {
            if (MecanimMode == MecanimMode.Disabled)
            {
                MecanimPush(obj, false);
            }
            else
            {
                if (ShouldPullDataFromMecanim(obj.RootState))
                {
                    MecanimPull(obj, obj.Storage);
                }
                else
                {
                    MecanimPush(obj, true);
                }
            }
        }

        public override void OnFrameCloned(NetworkObj obj, NetworkStorage storage)
        {
            storage.Values[obj[this]].TriggerLocal.Frame = 0;
            storage.Values[obj[this]].TriggerLocal.History = 0;
        }

        void MecanimPull(NetworkObj obj, NetworkStorage storage)
        {
            if (obj.RootState.Animator == null)
            {
                return;
            }

            if (obj.RootState.Animator.GetBool(PropertyName) && (obj.RootState.Animator.IsInTransition(MecanimLayer) == false))
            {
                // update send trigger
                storage.Values[obj[this]].TriggerSend.Update(Core.Frame, true);

                // notify Ascension this property changed
                storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);

                // invoke callback
                var cb = obj.Storage.Values[obj[this]].Action;
                if (cb != null)
                {
                    cb();
                }
            }
        }

        void MecanimPush(NetworkObj obj, bool push)
        {
            var root = obj.RootState;
            var frames = root.Frames.GetIterator();

            while (frames.Next())
            {
                var s = frames.val;
                var i = obj[this];

                var t_frame = s.Values[i].TriggerLocal.Frame;
                var t_history = s.Values[i].TriggerLocal.History;
                var t_callback = s.Values[i].Action;

                for (int k = (obj.RootState.Entity.SendRate - 1); (k >= 0) && (t_history != 0); --k)
                {
                    if (t_frame - k > obj.RootState.Entity.Frame)
                    {
                        continue;
                    }

                    int b = 1 << k;

                    if ((t_history & b) == b)
                    {
                        t_history &= ~b;

                        // clear history for this bit
                        s.Values[i].TriggerLocal.History = t_history;

                        // update send trigger
                        root.Storage.Values[i].TriggerSend.Update(Core.Frame, true);

                        // meep
                        root.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);

                        if (push)
                        {
                            for (int a = 0; a < obj.RootState.Animators.Count; ++a)
                            {
                                obj.RootState.Animators[a].SetTrigger(PropertyName);
                            }
                        }

                        if (t_callback != null)
                        {
                            t_callback();
                        }
                    }
                }
            }
        }
    }
}
