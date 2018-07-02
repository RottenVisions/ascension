using System;
using UnityEngine;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public partial class EventDispatcher
    {
        struct EventListener
        {
            public IEventListener Listener;
            public GameObject GameObject;
            public MonoBehaviour Behaviour;
        }

        struct CallbackWrapper
        {
            public System.Object Original;
            public Action<Event> Wrapper;
        }

        List<EventListener> targets = new List<EventListener>();
        Dictionary<Type, List<CallbackWrapper>> callbacks = new Dictionary<Type, List<CallbackWrapper>>();

        public void Raise(Event ev)
        {
            IEventFactory factory = Factory.GetEventFactory(ev.Meta.TypeId);

            List<CallbackWrapper> newCallbacks;

            if (callbacks.TryGetValue(ev.GetType(), out newCallbacks))
            {
                for (int i = 0; i < callbacks.Count; ++i)
                {
                    newCallbacks[i].Wrapper(ev);
                }
            }

            for (int i = 0; i < targets.Count; ++i)
            {
                EventListener mb = targets[i];

                if (mb.Behaviour)
                {
                    // dont call on disabled behaviours
                    if (mb.Behaviour.enabled == false)
                    {
                        if ((mb.Listener == null) || (mb.Listener.InvokeIfDisabled == false))
                        {
                            continue;
                        }
                    }

                    // dont call on behaviours attached to inactive game objects
                    if (mb.GameObject.activeInHierarchy == false)
                    {
                        if ((mb.Listener == null) || (mb.Listener.InvokeIfGameObjectIsInactive == false))
                        {
                            continue;
                        }
                    }

                    // invoke event
                    try
                    {
                        factory.Dispatch(ev, mb.Behaviour);
                    }
                    catch (Exception exn)
                    {
                        NetLog.Error("User code threw exception when invoking {0}", ev);
                        NetLog.Exception(exn);
                    }

                }
                else
                {
                    // remove callback if this behaviour is destroyed
                    targets.RemoveAt(i);

                    // 
                    --i;

                    continue;
                }
            }
        }

        public void Add(MonoBehaviour behaviour)
        {

            for (int i = 0; i < targets.Count; ++i)
            {
                if (ReferenceEquals(targets[i].Behaviour, behaviour))
                {
                    NetLog.Warn("Behaviour is already registered in this dispatcher, ignoring call to Add.");
                    return;
                }
            }

            targets.Add(new EventListener { Behaviour = behaviour, GameObject = behaviour.gameObject, Listener = behaviour as IEventListener });
        }

        public void Add<T>(Action<T> callback) where T : Event
        {
            List<CallbackWrapper> newCallbacks;

            if (callbacks.TryGetValue(typeof(T), out newCallbacks) == false)
            {
                callbacks.Add(typeof(T), newCallbacks = new List<CallbackWrapper>());
            }

            CallbackWrapper wrapper;
            wrapper.Original = callback;
            wrapper.Wrapper = ev => callback((T)ev);

            newCallbacks.Add(wrapper);
        }

        public void Remove<T>(Action<T> callback) where T : Event
        {
            List<CallbackWrapper> newCallbacks;

            if (callbacks.TryGetValue(typeof(T), out newCallbacks) == false)
            {
                for (int i = 0; i < callbacks.Count; ++i)
                {
                    var org = (Action<T>)newCallbacks[i].Original;
                    if (org == callback)
                    {
                        newCallbacks.RemoveAt(i);
                        return;
                    }
                }
            }

            NetLog.Warn("Could not find delegate registered as callback");
        }

        public void Remove(MonoBehaviour behaviour)
        {

            for (int i = 0; i < targets.Count; ++i)
            {
                if (ReferenceEquals(targets[i].Behaviour, behaviour))
                {
                    targets.RemoveAt(i);
                    return;
                }
            }

            NetLog.Warn("Behaviour not available in this dispatcher, ignoring call to Remove.");
        }

        public void Clear()
        {
            callbacks = new Dictionary<Type, List<CallbackWrapper>>();

            for (int i = 0; i < targets.Count; ++i)
            {
                var mb = targets[i].Behaviour as BaseGlobalEventListener;
                if (mb != null)
                {
                    if (mb.PersistBetweenStartupAndShutdown())
                    {
                        continue;
                    }
                }

                // remove at this indexx
                targets.RemoveAt(i);

                // reset index
                --i;
            }
        }
    }

}
