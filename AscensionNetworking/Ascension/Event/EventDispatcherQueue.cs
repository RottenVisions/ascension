using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ascension.Networking
{

    public partial class EventDispatcher
    {
        static Queue<Event> dispatchQueue = new Queue<Event>();

        public static void Enqueue(Event ev)
        {
            dispatchQueue.Enqueue(ev);
        }

        public static void Received(Event ev)
        {
            if (Core.EventFilter.EventReceived(ev))
            {
                dispatchQueue.Enqueue(ev);
            }
        }

        public static void DispatchAllEvents()
        {
            while (dispatchQueue.Count > 0)
            {
                Dispatch(dispatchQueue.Dequeue());
            }
        }

        static void Dispatch(Event ev)
        {
            switch (ev.Targets)
            {
                case Event.ENTITY_EVERYONE:
                    Entity_Everyone(ev);
                    break;

                case Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER:
                    Entity_Everyone_Except_Controller(ev);
                    break;

                case Event.ENTITY_EVERYONE_EXCEPT_OWNER:
                    Entity_Everyone_Except_Owner(ev);
                    break;

                case Event.ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER:
                    Entity_Everyone_Except_Owner_And_Controller(ev);
                    break;

                case Event.ENTITY_ONLY_CONTROLLER:
                    Entity_Only_Controller(ev);
                    break;

                case Event.ENTITY_ONLY_CONTROLLER_AND_OWNER:
                    Entity_Only_Controller_And_Owner(ev);
                    break;

                case Event.ENTITY_ONLY_OWNER:
                    Entity_Only_Owner(ev);
                    break;

                case Event.ENTITY_ONLY_SELF:
                    Entity_Only_Self(ev);
                    break;

                case Event.GLOBAL_EVERYONE:
                    Global_Everyone(ev);
                    break;

                case Event.GLOBAL_OTHERS:
                    Global_Others(ev);
                    break;

                case Event.GLOBAL_ALL_CLIENTS:
                    Global_All_Clients(ev);
                    break;

                case Event.GLOBAL_ONLY_SERVER:
                    Global_Server(ev);
                    break;

                case Event.GLOBAL_SPECIFIC_CONNECTION:
                    Global_Specific_Connection(ev);
                    break;

                case Event.GLOBAL_ONLY_SELF:
                    Global_Only_Self(ev);
                    break;
            }
        }

        private static void Entity_Only_Controller_And_Owner(Event ev)
        {
            if (ev.TargetEntity)
            {
                if (ev.TargetEntity.HasControl)
                {
                    RaiseLocal(ev);

                    if (ev.TargetEntity.IsOwner)
                    {

                        // if we're also owner, free this
                        ev.FreeStorage();
                    }
                    else {

                        // check so this was not received from source, and if it wasn't then send it to it
                        if (ev.SourceConnection != ev.TargetEntity.Source)
                        {
                            ev.TargetEntity.Source.eventChannel.Queue(ev);
                        }

                    }
                }
                else {
                    if (ev.TargetEntity.IsOwner)
                    {
                        // raise this locally for owner
                        RaiseLocal(ev);

                        if (ev.TargetEntity.Controller != null)
                        {

                            // check so we didn't receive this from owner
                            if (ev.SourceConnection != ev.TargetEntity.Controller)
                            {
                                ev.TargetEntity.Controller.eventChannel.Queue(ev);
                            }

                        }
                        else
                        {
                            NetLog.Warn("NetworkEvent sent to controller but no controller exists, event will NOT be raised");
                        }
                    }
                    else
                    {
                        ev.TargetEntity.Source.eventChannel.Queue(ev);
                    }
                }
            }
            else
            {
                NetLog.Warn("NetworkEvent with NULL target, event will NOT be forwarded or raised");
            }
        }

        static void Global_Only_Self(Event ev)
        {
            RaiseLocal(ev);

            // we can free this (never used after this)
            ev.FreeStorage();
        }

        static void Entity_Only_Self(Event ev)
        {
            if (ev.TargetEntity)
            {
                RaiseLocal(ev);

                // we can free this (never used after this)
                ev.FreeStorage();
            }
        }

        static void Entity_Only_Owner(Event ev)
        {
            if (ev.TargetEntity)
            {
                if (ev.TargetEntity.IsOwner)
                {
                    RaiseLocal(ev);

                    // we can free this (never used after this)
                    ev.FreeStorage();
                }
                else
                {
                    // forward to owner
                    ev.TargetEntity.Source.eventChannel.Queue(ev);
                }
            }
        }

        static void Entity_Only_Controller(Event ev)
        {
            if (ev.TargetEntity)
            {
                if (ev.TargetEntity.HasControl)
                {
                    RaiseLocal(ev);

                    // we can free this (never used after this)
                    ev.FreeStorage();
                }
                else
                {
                    if (ev.TargetEntity.IsOwner)
                    {
                        if (ev.TargetEntity.Controller != null)
                        {
                            ev.TargetEntity.Controller.eventChannel.Queue(ev);
                        }
                        else
                        {
                            NetLog.Warn("NetworkEvent sent to controller but no controller exists, event will NOT be raised");
                        }
                    }
                    else
                    {
                        ev.TargetEntity.Source.eventChannel.Queue(ev);
                    }
                }
            }
            else
            {
                NetLog.Warn("NetworkEvent with NULL target, event will NOT be forwarded or raised");
            }
        }

        static void Entity_Everyone_Except_Owner_And_Controller(Event ev)
        {
            if (ev.TargetEntity != null)
            {
                var it = Core.connections.GetIterator();

                while (it.Next())
                {
                    if (ReferenceEquals(it.val, ev.SourceConnection))
                    {
                        continue;
                    }

                    it.val.eventChannel.Queue(ev);
                }

                if (ev.TargetEntity.IsOwner == false && ev.TargetEntity.HasControl == false)
                {
                    RaiseLocal(ev);
                }
            }
        }

        static void Entity_Everyone_Except_Owner(Event ev)
        {
            if (ev.TargetEntity != null)
            {
                var it = Core.connections.GetIterator();

                while (it.Next())
                {
                    if (ReferenceEquals(it.val, ev.SourceConnection))
                    {
                        continue;
                    }

                    it.val.eventChannel.Queue(ev);
                }

                if (ev.TargetEntity.IsOwner == false)
                {
                    RaiseLocal(ev);
                }
            }
        }

        static void Entity_Everyone_Except_Controller(Event ev)
        {
            if (ev.TargetEntity != null)
            {
                var it = Core.connections.GetIterator();

                while (it.Next())
                {
                    if (ev.TargetEntity.IsController(it.val))
                    {
                        continue;
                    }

                    if (ReferenceEquals(it.val, ev.SourceConnection))
                    {
                        continue;
                    }

                    it.val.eventChannel.Queue(ev);
                }

                if (ev.TargetEntity.HasControl == false)
                {
                    RaiseLocal(ev);
                }
            }
        }

        static void Entity_Everyone(Event ev)
        {
            var it = Core.connections.GetIterator();

            if (ev.TargetEntity != null)
            {
                while (it.Next())
                {
                    if (ReferenceEquals(it.val, ev.SourceConnection))
                    {
                        continue;
                    }

                    it.val.eventChannel.Queue(ev);
                }

                RaiseLocal(ev);
            }
        }

        static void Global_Specific_Connection(Event ev)
        {
            if (ev.FromSelf)
            {
                ev.TargetConnection.eventChannel.Queue(ev);
            }
            else
            {
                RaiseLocal(ev);
            }
        }

        static void Global_Server(Event ev)
        {
            if (Core.IsServer)
            {
                RaiseLocal(ev);
            }
            else
            {
                Core.Server.eventChannel.Queue(ev);
            }
        }

        static void Global_All_Clients(Event ev)
        {
            var it = Core.connections.GetIterator();

            while (it.Next())
            {
                if (ReferenceEquals(it.val, ev.SourceConnection))
                {
                    continue;
                }

                it.val.eventChannel.Queue(ev);
            }

            if (Core.IsClient)
            {
                RaiseLocal(ev);
            }
        }

        static void Global_Others(Event ev)
        {
            var it = Core.connections.GetIterator();

            while (it.Next())
            {
                if (ReferenceEquals(it.val, ev.SourceConnection))
                {
                    continue;
                }

                it.val.eventChannel.Queue(ev);
            }

            if (ev.FromSelf == false)
            {
                RaiseLocal(ev);
            }
        }

        static void Global_Everyone(Event ev)
        {
            var it = Core.connections.GetIterator();

            while (it.Next())
            {
                if (ReferenceEquals(it.val, ev.SourceConnection))
                {
                    continue;
                }

                it.val.eventChannel.Queue(ev);
            }

            RaiseLocal(ev);
        }

        static void RaiseLocal(Event ev)
        {
            try
            {
                NetLog.Debug("Raising {0}", ev);

                if (ev.IsEntityEvent)
                {
                    ev.TargetEntity.EventDispatcher.Raise(ev);
                }
                else
                {
                    Core.GlobalEventDispatcher.Raise(ev);
                }

                if (Core.IsClient && ev.FromSelf == false)
                {
                    ev.FreeStorage();
                }
            }
            finally
            {
                ev.DecrementRefs();
            }
        }
    }
}
