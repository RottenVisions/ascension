using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Ascension.Networking
{
    partial class NetworkState : IState
    {
        Dictionary<string, List<PropertyCallback>> Callbacks = new Dictionary<string, List<PropertyCallback>>();
        Dictionary<string, List<PropertyCallbackSimple>> CallbacksSimple = new Dictionary<string, List<PropertyCallbackSimple>>();

        bool VerifyCallbackPath(string path)
        {
            if (Meta.CallbackPaths.Contains(path))
            {
                return true;
            }

            NetLog.Error("No callback path '{0}' available on {1}", path, this);
            return false;
        }

        UE.Animator IState.Animator
        {
            get { return Animators[0]; }
        }

        IEnumerable<UE.Animator> IState.AllAnimators
        {
            get { return Animators; }
        }

        void IState.AddAnimator(UE.Animator animator)
        {
            Animators.Add(animator);
        }

        void IState.SetAnimator(UnityEngine.Animator animator)
        {
            Animators.Clear();
            Animators.Add(animator);
        }

        void IState.AddCallback(string path, PropertyCallback callback)
        {
#if DEBUG
            if (!VerifyCallbackPath(path)) return;
#endif
            List<PropertyCallback> callbacksList;

            if (Callbacks.TryGetValue(path, out callbacksList) == false)
            {
                Callbacks[path] = callbacksList = new List<PropertyCallback>(32);
            }

            callbacksList.Add(callback);
        }

        void IState.AddCallback(string path, PropertyCallbackSimple callback)
        {
#if DEBUG
            if (!VerifyCallbackPath(path)) return;
#endif
            List<PropertyCallbackSimple> callbacksList;

            if (CallbacksSimple.TryGetValue(path, out callbacksList) == false)
            {
                CallbacksSimple[path] = callbacksList = new List<PropertyCallbackSimple>(32);
            }

            callbacksList.Add(callback);
        }

        void IState.RemoveCallback(string path, PropertyCallback callback)
        {
#if DEBUG
            if (!VerifyCallbackPath(path)) return;
#endif
            List<PropertyCallback> callbacksList;

            if (Callbacks.TryGetValue(path, out callbacksList) == true)
            {
                callbacksList.Remove(callback);
            }
        }

        void IState.RemoveCallback(string path, PropertyCallbackSimple callback)
        {
#if DEBUG
            if (!VerifyCallbackPath(path)) return;
#endif
            List<PropertyCallbackSimple> callbacksList;

            if (CallbacksSimple.TryGetValue(path, out callbacksList) == true)
            {
                callbacksList.Remove(callback);
            }
        }

        void IState.SetDynamic(string property, object value)
        {
            var hash = property.GetHashCode();

            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                if ((Meta.Properties[i].OffsetObjects == 0) && (Meta.Properties[i].Property.PropertyNameHash == hash) && (Meta.Properties[i].Property.PropertyName == property))
                {
                    Meta.Properties[i].Property.SetDynamic(this, value);
                    return;
                }
            }
        }

        object IState.GetDynamic(string property)
        {
            var hash = property.GetHashCode();

            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                if ((Meta.Properties[i].OffsetObjects == 0) && (Meta.Properties[i].Property.PropertyNameHash == hash) && (Meta.Properties[i].Property.PropertyName == property))
                {
                    return Meta.Properties[i].Property.GetDynamic(this);
                }
            }

            throw new ArgumentException(string.Format("Unknown property {0}", property));
        }


        void IState.SetTransforms(NetworkTransform transform, UE.Transform simulate)
        {
            (this as IState).SetTransforms(transform, simulate, null);
        }

        void IState.SetTransforms(NetworkTransform transform, UE.Transform simulate, UE.Transform render)
        {
            transform.SetTransformsInternal(simulate, render);

            if (Entity.AttachIsRunning && simulate)
            {
                NetAssert.Same(transform, this.Storage.Values[transform.PropertyIndex].Transform);

                this.Storage.Values[transform.PropertyIndex].Vector3 = simulate.position;
                this.Storage.Values[transform.PropertyIndex + 1].Quaternion = simulate.rotation;
            }
        }

        void IState.ForceTransform(NetworkTransform transform, UE.Vector3 position)
        {
            (this as IState).ForceTransform(transform, position, UE.Quaternion.identity);
        }

        void IState.ForceTransform(NetworkTransform transform, UE.Vector3 position, UE.Quaternion rotation)
        {
            if (Entity.IsOwner)
            {
                return;
            }

            var it = this.Frames.GetIterator();

            while (it.Next())
            {
                it.val.Values[transform.PropertyIndex].Vector3 = position;
                it.val.Values[transform.PropertyIndex + 1].Quaternion = rotation;
            }
        }

        Boolean IState.TrySetDynamic(String property, Object value)
        {
            var hash = property.GetHashCode();

            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                if ((Meta.Properties[i].OffsetObjects == 0) && (Meta.Properties[i].Property.PropertyNameHash == hash) && (Meta.Properties[i].Property.PropertyName == property))
                {
                    Meta.Properties[i].Property.SetDynamic(this, value);
                    return true;
                }
            }

            return false;
        }

        Boolean IState.TryGetDynamic(String property, out Object value)
        {
            var hash = property.GetHashCode();

            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                if ((Meta.Properties[i].OffsetObjects == 0) && (Meta.Properties[i].Property.PropertyNameHash == hash) && (Meta.Properties[i].Property.PropertyName == property))
                {
                    value = Meta.Properties[i].Property.GetDynamic(this);
                    return true;
                }
            }

            value = null;
            return false;
        }

        void IDisposable.Dispose()
        {
        }

        void InvokeCallbacks()
        {
            while (!Frames.First.IsZero)
            {
                // merge into default mask
                PropertyDefaultMask.Combine(Frames.First);

                // merge into proxy masks
                if (Entity.Proxies.Count > 0)
                {
                    var proxies = Entity.Proxies.GetIterator();

                    while (proxies.Next())
                    {
                        proxies.val.Combine(Frames.First);
                    }
                }

                // invoke callbacks
                for (var n = 0; n < BitSet.BITSET_LONGS; ++n)
                {
                    var v = Frames.First[n];

                    if (v == 0UL)
                    {
                        continue;
                    }

                    // clear all bits
                    Frames.First[n] = 0UL;

                    for (var b = 0; b < 64; ++b)
                    {
                        if ((v & (1UL << b)) == 0UL)
                        {
                            continue;
                        }

                        // clear bit
                        v &= ~(1UL << b);

                        // invoke callbacks
                        InvokeCallbacksForProperty((n * 64) + b);
                    }
                }
            }
        }

        void InvokeCallbacksForProperty(int propertyIndex)
        {
            try
            {
                NetworkPropertyInfo pi = Meta.Properties[propertyIndex];

                List<PropertyCallback> callbacks;
                List<PropertyCallbackSimple> callbacksSimple;

                for (int i = 0; i < pi.Paths.Length; ++i)
                {
                    if (Callbacks.TryGetValue(pi.Paths[i], out callbacks))
                    {
                        for (int c = 0; c < callbacks.Count; ++c)
                        {
                            callbacks[c](this, pi.Paths[pi.Paths.Length - 1], new ArrayIndices(pi.Indices));
                        }
                    }

                    if (CallbacksSimple.TryGetValue(pi.Paths[i], out callbacksSimple))
                    {
                        for (int c = 0; c < callbacksSimple.Count; ++c)
                        {
                            callbacksSimple[c]();
                        }
                    }
                }
            }
            catch (Exception exn)
            {
                NetLog.Exception(exn);
            }
        }

    }
}
