using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascension.Networking
{
    public abstract class NetworkProperty_Mecanim : NetworkProperty
    {
        public MecanimMode MecanimMode;
        public MecanimDirection MecanimDirection;
        public float MecanimDamping;
        public int MecanimLayer;

        public override bool WantsOnSimulateAfter
        {
            get { return true; }
        }

        public void Settings_Mecanim(MecanimMode mode, MecanimDirection direction, float damping, int layer)
        {
            MecanimMode = mode;
            MecanimDirection = direction;
            MecanimDamping = damping;
            MecanimLayer = layer;
        }

        public override void OnSimulateAfter(NetworkObj obj)
        {
            NetworkState state = (NetworkState)obj.Root;

            if (MecanimMode != MecanimMode.Disabled)
            {
                if (state.Animators.Count > 0)
                {
                    if (MecanimMode == MecanimMode.LayerWeight)
                    {
                        if (ShouldPullDataFromMecanim(state))
                        {
                            PullMecanimLayer(state);
                        }
                        else
                        {
                            PushMecanimLayer(state);
                        }
                    }
                    else
                    {
                        if (ShouldPullDataFromMecanim(state))
                        {
                            PullMecanimValue(state);
                        }
                        else
                        {
                            PushMecanimValue(state);
                        }
                    }
                }
                else
                {
#if DEBUG
                    if (state.MecanimWarningTimeout < UnityEngine.Time.time)
                    {
                        NetLog.Warn("You have not assigned a mecanim animator to the state on {0}, but you have properties set to use mecanim", state.Entity.UnityObject.gameObject.name);
                        state.MecanimWarningTimeout = UnityEngine.Time.time + 1;
                    }
#endif
                }
            }
        }

        public bool ShouldPullDataFromMecanim(NetworkState state)
        {
#if DEBUG
            if (
#else
        return
#endif
        MecanimDirection == MecanimDirection.UsingAnimatorMethods && (state.Entity.IsOwner || state.Entity.HasPredictedControl)

#if DEBUG
        )
            {
                if (state.Animators.Count > 1)
                {
                    NetLog.Warn("Property '{0}' set to 'UsingAnimatorMethods' but several animators have been specified, only the first one added will be used.");
                }

                return true;
            }

            return false;
#else
        ;
#endif
        }

        public virtual void PullMecanimValue(NetworkState state) { }
        public virtual void PushMecanimValue(NetworkState state) { }

        void PullMecanimLayer(NetworkState state)
        {
            if (state.Animator == null)
            {
                return;
            }

            //UnityEngine.Debug.Log(state.RootState.Entity.UnityObject.name + ":" + state.Animator.runtimeAnimatorController.name + ":"  + MecanimLayer);

            float newValue = state.Animator.GetLayerWeight(MecanimLayer);
            float oldValue = state.Storage.Values[state[this]].Float0;

            state.Storage.Values[state[this]].Float0 = newValue;

            if (NetworkValue.Diff(newValue, oldValue))
            {
                state.Storage.PropertyChanged(state.OffsetProperties + this.OffsetProperties);
            }
        }

        void PushMecanimLayer(NetworkState state)
        {
            for (int i = 0; i < state.Animators.Count; ++i)
            {
                state.Animators[i].SetLayerWeight(MecanimLayer, state.Storage.Values[state[this]].Float0);
            }
        }
    }
}
