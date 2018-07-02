using System.Collections.Generic;
using UE = UnityEngine;

namespace Ascension.Networking
{
    public abstract partial class NetworkState : NetworkObj_Root
    {
#if DEBUG
        public float MecanimWarningTimeout = 0;
#endif

        public Entity Entity;
        public List<UE.Animator> Animators = new List<UE.Animator>();
        public new NetworkState_Meta Meta;

        public BitSet PropertyDefaultMask = new BitSet();
        public Priority[] PropertyPriorityTemp;

        public ListExtended<NetworkStorage> Frames = new ListExtended<NetworkStorage>();

        public UE.Animator Animator
        {
            get { return Animators.Count > 0 ? Animators[0] : null; }
        }

        public sealed override NetworkStorage Storage
        {
            get { return Frames.First; }
        }

        public NetworkState(NetworkState_Meta meta)
          : base(meta)
        {
            Meta = meta;
        }

        public void SetAnimator(UE.Animator animator)
        {
            Animators.Clear();

            if (animator)
            {
                Animators.Add(animator);
            }
        }

        public void AddAnimator(UE.Animator animator)
        {
            if (animator)
            {
                Animators.Add(animator);
            }
        }
    }
}
