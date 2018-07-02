using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public partial class Entity : IListNode, IPriorityCalculator, IEntityReplicationFilter
    {
        object IListNode.Prev { get; set; }
        object IListNode.Next { get; set; }
        object IListNode.List { get; set; }

        bool canQueueCommands = false;
        bool canQueueCallbacks = false;

        public UniqueId SceneId;
        public NetworkId NetworkId;
        public PrefabId PrefabId;
        public EntityFlags Flags;
        public bool AttachIsRunning;

        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;

        public Entity Parent;
        public AscensionEntity UnityObject;
        public Connection Source;
        public Connection Controller;

        public IMessageRider DetachToken;
        public IMessageRider AttachToken;
        public IMessageRider ControlLostToken;
        public IMessageRider ControlGainedToken;

        public IEntitySerializer Serializer;
        public IEntityBehaviour[] Behaviours;

        public IPriorityCalculator PriorityCalculator;
        public IEntityReplicationFilter ReplicationFilter;

        public bool IsOwner;
        public bool IsFrozen;
        public bool AutoRemoveChildEntities;
        public bool AllowFirstReplicationWhenFrozen;

        public int UpdateRate;
        public int LastFrameReceived;

        public int AutoFreezeProxyFrames;
        public bool CanFreeze = true;

        public ushort CommandSequence = 0;
        public Command CommandLastExecuted = null;

        public EventDispatcher EventDispatcher = new EventDispatcher();
        public ListExtended<Command> CommandQueue = new ListExtended<Command>();
        public List<CommandCallbackItem> CommandCallbacks = new List<CommandCallbackItem>();
        public ListExtended<EntityProxy> Proxies = new ListExtended<EntityProxy>();

        public int Frame
        {
            get
            {
                if (IsOwner)
                {
                    return Core.Frame;
                }

                if (HasPredictedControl)
                {
                    return Core.Frame;
                }

                return Source.RemoteFrame;
            }
        }

        public int SendRate
        {
            get
            {
                if (IsOwner)
                {
                    return UpdateRate * Core.LocalSendRate;
                }
                else
                {
                    return UpdateRate * Core.RemoteSendRate;
                }
            }
        }

        public bool IsSceneObject
        {
            get { return Flags & EntityFlags.SCENE_OBJECT; }
        }

        public bool HasParent
        {
            get { return Parent != null && Parent.IsAttached; }
        }

        public bool IsAttached
        {
            get { return Flags & EntityFlags.ATTACHED; }
        }

        public bool IsDummy
        {
            get { return !IsOwner && !HasPredictedControl; }
        }

        public bool HasControl
        {
            get { return Flags & EntityFlags.HAS_CONTROL; }
        }

        public bool HasPredictedControl
        {
            get { return HasControl && (Flags & EntityFlags.CONTROLLER_LOCAL_PREDICTION); }
        }

        public bool PersistsOnSceneLoad
        {
            get { return Flags & EntityFlags.PERSIST_ON_LOAD; }
        }

        public bool CanQueueCommands
        {
            get { return canQueueCommands; }
        }
        public override string ToString()
        {
            return string.Format("[Entity {0} {1}]", NetworkId, Serializer);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return NetworkId.GetHashCode();
        }

        public void SetParent(Entity entity)
        {
            if (IsOwner || HasPredictedControl)
            {
                SetParentInternal(entity);
            }
            else
            {
                NetLog.Error("You are not allowed to assign the parent of this entity, only the owner or a controller with local prediction can");
            }
        }

        public void SetParentInternal(Entity entity)
        {
            if (entity != Parent)
            {
                if ((entity != null) && (entity.IsAttached == false))
                {
                    NetLog.Error("You can't assign a detached entity as the parent of another entity");
                    return;
                }

                try
                {
                    // notify serializer
                    Serializer.OnParentChanging(entity, Parent);
                }
                finally
                {
                    // set parent
                    Parent = entity;
                }
            }
        }

        public void SetScopeAll(bool inScope)
        {
            var it = Core.connections.GetIterator();

            while (it.Next())
            {
                SetScope(it.val, inScope);
            }
        }

        public void SetScope(Connection connection, bool inScope)
        {
            connection.entityChannel.SetScope(this, inScope);
        }

        public void Freeze(bool freeze)
        {
            if (IsFrozen != freeze)
            {
                if (IsFrozen)
                {
                    IsFrozen = false;
                    Core.entitiesFrozen.Remove(this);
                    Core.entitiesThawed.AddLast(this);
                    GlobalEventListenerBase.EntityThawedInvoke(this.UnityObject);
                }
                else
                {
                    if (CanFreeze)
                    {
                        IsFrozen = true;
                        Core.entitiesThawed.Remove(this);
                        Core.entitiesFrozen.AddLast(this);
                        GlobalEventListenerBase.EntityFrozenInvoke(this.UnityObject);
                    }
                }
            }
        }

        public EntityProxy CreateProxy()
        {
            EntityProxy p;

            p = new EntityProxy();
            p.Entity = this;
            p.Combine(Serializer.GetDefaultMask());

            // add to list
            Proxies.AddLast(p);

            // let serializer init
            Serializer.InitProxy(p);

            return p;
        }

        public void Attach()
        {
            NetAssert.NotNull(UnityObject);
            NetAssert.False(IsAttached);
            NetAssert.True((NetworkId.Packed == 0UL) || (Source != null));

            try
            {
                AttachIsRunning = true;

                // mark as don't destroy on load
                GameObject.DontDestroyOnLoad(UnityObject.gameObject);

                // assign network id
                if (Source == null)
                {
                    NetworkId = NetworkIdAllocator.Allocate();
                }

                // add to entities list
                Core.entitiesThawed.AddLast(this);

                // mark as attached
                Flags |= EntityFlags.ATTACHED;

                // call out to behaviours
                foreach (IEntityBehaviour eb in Behaviours)
                {
                    try
                    {
                        if (eb.Invoke && ReferenceEquals(eb.entity, this.UnityObject))
                        {
                            eb.Attached();
                        }
                    }
                    catch (Exception exn)
                    {
                        NetLog.Error("User code threw exception inside Attached callback");
                        NetLog.Exception(exn);
                    }
                }

                // call out to user
                try
                {
                    GlobalEventListenerBase.EntityAttachedInvoke(this.UnityObject);
                }
                catch (Exception exn)
                {
                    NetLog.Error("User code threw exception inside Attached callback");
                    NetLog.Exception(exn);
                }

                // log
                NetLog.Debug("Attached {0} (Token: {1})", this, AttachToken);
            }
            finally
            {
                AttachIsRunning = false;
            }
        }

        public void Detach()
        {
            NetAssert.NotNull(UnityObject);
            NetAssert.True(IsAttached);
            NetAssert.True(NetworkId.Packed != 0UL);

            if (AutoRemoveChildEntities)
            {
                foreach (AscensionEntity child in UnityObject.GetComponentsInChildren(typeof(AscensionEntity), true))
                {
                    if (child.IsAttached && (ReferenceEquals(child.entity, this) == false))
                    {
                        child.transform.parent = null;
                    }
                }
            }

            if (Controller)
            {
                RevokeControl(null);
            }

            // destroy on all connections
            var it = Core.connections.GetIterator();

            while (it.Next())
            {
                it.val.entityChannel.DestroyOnRemote(this);
            }

            // call out to behaviours
            foreach (IEntityBehaviour eb in Behaviours)
            {
                try
                {
                    if (eb.Invoke && ReferenceEquals(eb.entity, this.UnityObject))
                    {
                        eb.Detached();
                        eb.entity = null;
                    }
                }
                catch (Exception exn)
                {
                    NetLog.Error("User code threw exception inside Detach callback");
                    NetLog.Exception(exn);
                }
            }

            // call out to user
            try
            {
                GlobalEventListenerBase.EntityDetachedInvoke(this.UnityObject);
            }
            catch (Exception exn)
            {
                NetLog.Error("User code threw exception inside Detach callback");
                NetLog.Exception(exn);
            }

            // clear out attached flag
            Flags &= ~EntityFlags.ATTACHED;

            // remove from entities list
            if (Core.entitiesFrozen.Contains(this))
            {
                Core.entitiesFrozen.Remove(this);
            }

            if (Core.entitiesThawed.Contains(this))
            {
                Core.entitiesThawed.Remove(this);
            }

            // clear from unity object
            UnityObject.entity = null;

            // log
            NetLog.Debug("Detached {0}", this);
        }

        public void AddEventListener(MonoBehaviour behaviour)
        {
            EventDispatcher.Add(behaviour);
        }

        public void RemoveEventListener(MonoBehaviour behaviour)
        {
            EventDispatcher.Remove(behaviour);
        }

        public bool IsController(Connection connection)
        {
            return connection != null && Controller != null && ReferenceEquals(Controller, connection);
        }

        public void Render()
        {
            Serializer.OnRender();
        }

        public void Initialize()
        {
            IsOwner = ReferenceEquals(Source, null);

            // make this a bit faster
            Behaviours = UnityObject.GetComponentsInChildren(typeof(IEntityBehaviour)).Select(x => x as IEntityBehaviour).Where(x => x != null).ToArray();

            // assign usertokens
            UnityObject.entity = this;

            // try to find a priority calculator
            var calculators = UnityObject.GetComponentsInChildren(typeof(IPriorityCalculator), true);

            foreach (IPriorityCalculator calculator in calculators)
            {
                var parent = ((MonoBehaviour)(object)calculator).GetComponentInParent<AscensionEntity>();

                if (parent && ReferenceEquals(parent.entity, this))
                {
                    PriorityCalculator = calculator;
                    break;
                }
            }

            // use the default priority calculator if none is available
            if (PriorityCalculator == null)
            {
                PriorityCalculator = this;
            }

            // find replication filter
            var filters = UnityObject.GetComponentsInChildren(typeof(IEntityReplicationFilter), true);

            foreach (IEntityReplicationFilter filter in filters)
            {
                var parent = ((MonoBehaviour)(object)filter).GetComponentInParent<AscensionEntity>();

                if (parent && ReferenceEquals(parent.entity, this))
                {
                    ReplicationFilter = filter;
                    break;
                }
            }

            // use the default replication filter if none is available
            if (ReplicationFilter == null)
            {
                ReplicationFilter = this;
            }

            // call into serializer
            Serializer.OnInitialized();

            // call to behaviours (this happens BEFORE attached)
            foreach (IEntityBehaviour eb in Behaviours)
            {
                eb.Initialized();
            }
        }

        public void SetIdle(Connection connection, bool idle)
        {
            if (idle && IsController(connection))
            {
                NetLog.Error("You can not idle {0} on {1}, as it is the controller for this entity", this, connection);
                return;
            }

            connection.entityChannel.SetIdle(this, idle);
        }

        void RemoveOldCommandCallbacks(int number)
        {
            for (int i = 0; i < CommandCallbacks.Count; ++i)
            {
                if (CommandCallbacks[i].End < number)
                {
                    // remove this index
                    CommandCallbacks.RemoveAt(i);

                    // 
                    --i;
                }
            }
        }

        public void Simulate()
        {
            Serializer.OnSimulateBefore();

            Iterator<Command> it;

            if (IsOwner)
            {
                foreach (IEntityBehaviour eb in Behaviours)
                {
                    try
                    {
                        if (eb != null && ((MonoBehaviour)(object)eb) && eb.Invoke && ReferenceEquals(eb.entity, this.UnityObject))
                        {
                            eb.SimulateOwner();
                        }
                    }
                    catch (Exception exn)
                    {
                        Debug.LogException(exn);
                    }
                }
            }

            else
            {
                //FIXED: Entities getting frozen on clients after 10 seconds.
                //if (AscensionNetwork.IsClient)
                //{
                //    var diff = AscensionNetwork.ServerFrame - (Serializer as NetworkState).Frames.Last.Frame;
                //    if (diff > 600)
                //    {
                //        Freeze(true);
                //    }
                //}
            }

            if (HasControl)
            {
                NetAssert.Null(Controller);

                // execute all old commands (in order)
                it = CommandQueue.GetIterator();

                while (it.Next())
                {
                    NetAssert.True(it.val.Flags & CommandFlags.HAS_EXECUTED);

                    var resetState = ReferenceEquals(it.val, CommandQueue.First);
                    if (resetState)
                    {
                        it.val.SmoothCorrection();
                    }

                    // exec old command
                    ExecuteCommand(it.val, resetState);
                }

                try
                {
                    canQueueCommands = true;

                    foreach (IEntityBehaviour eb in Behaviours)
                    {
                        if (eb.Invoke && ReferenceEquals(eb.entity, this.UnityObject))
                        {
                            eb.SimulateController();
                        }
                    }
                }
                finally
                {
                    canQueueCommands = false;
                }

                // execute all new commands (in order)
                it = CommandQueue.GetIterator();

                while (it.Next())
                {
                    if (it.val.Flags & CommandFlags.HAS_EXECUTED)
                    {
                        continue;
                    }

                    ExecuteCommand(it.val, false);
                }

                // if this is a local entity we are controlling
                // we should dispose all commands (there is no need to store them)
                if (IsOwner)
                {
                    while (CommandQueue.Count > 0)
                    {
                        CommandQueue.RemoveFirst();
                    }

                    //RemoveOldCommandCallbacks(CommandSequence);
                }
                else
                {
                    //if (CommandQueue.count > 0) {
                    //  RemoveOldCommandCallbacks(CommandQueue.First.Sequence);
                    //}
                }
            }
            else
            {
                if (Controller != null)
                {
                    //if (CommandQueue.count > 0) {
                    //  RemoveOldCommandCallbacks(CommandQueue.First.Sequence);
                    //}

                    if (ExecuteCommandsFromRemote() == 0)
                    {
                        Command cmd = CommandQueue.LastOrDefault;

                        for (int i = 0; i < Behaviours.Length; ++i)
                        {
                            if (ReferenceEquals(Behaviours[i].entity, this.UnityObject))
                            {
                                Behaviours[i].MissingCommand(cmd);
                            }
                        }
                    }
                }
            }

            Serializer.OnSimulateAfter();
        }

        int ExecuteCommandsFromRemote()
        {
            int commandsExecuted = 0;

            NetAssert.True(IsOwner);

            do
            {
                var it = CommandQueue.GetIterator();

                while (it.Next())
                {
                    if (it.val.Flags & CommandFlags.HAS_EXECUTED)
                    {
                        continue;
                    }

                    try
                    {
                        ExecuteCommand(it.val, false);
                        commandsExecuted += 1;
                        break;
                    }
                    finally
                    {
                        it.val.Flags |= CommandFlags.SEND_STATE;
                    }
                }
            } while (UnexecutedCommandCount() > Core.Config.commandDejitterDelay);

            return commandsExecuted;
        }

        //void ExecuteCommandCallback(CommandCallbackItem cb, Command cmd) {
        //  try {
        //    cb.Callback(cb.Command, cmd);
        //  }
        //  catch (Exception exn) {
        //    NetLog.Exception(exn);
        //  }
        //}

        void ExecuteCommand(Command cmd, bool resetState)
        {
            try
            {
                // execute all command callbacks
                //for (int i = 0; i < CommandCallbacks.Count; ++i) {
                //  var cb = CommandCallbacks[i];

                //  switch (cb.Mode) {
                //    case CommandCallbackModes.InvokeOnce:
                //      if (cmd.Sequence == cb.End) {
                //        ExecuteCommandCallback(cb, cmd);
                //      }
                //      break;

                //    case CommandCallbackModes.InvokeRepeating:
                //      if (cmd.Sequence >= cb.Start && cmd.Sequence <= cb.End) {
                //        ExecuteCommandCallback(cb, cmd);
                //      }
                //      break;
                //  }
                //}

                canQueueCallbacks = cmd.IsFirstExecution;

                foreach (IEntityBehaviour eb in Behaviours)
                {
                    eb.ExecuteCommand(cmd, resetState);
                }
            }
            finally
            {
                // flag this so it can't queue more callbacks
                canQueueCallbacks = false;

                // flag this as executed
                cmd.Flags |= CommandFlags.HAS_EXECUTED;
            }
        }

        int UnexecutedCommandCount()
        {
            int count = 0;
            var it = CommandQueue.GetIterator();

            while (it.Next())
            {
                if (it.val.IsFirstExecution)
                {
                    count += 1;
                }
            }

            return count;
        }

        public void InvokeOnce(Command command, CommandCallback callback, int delay)
        {
            NetAssert.True(delay > 0);

            if (!canQueueCallbacks)
            {
                NetLog.Error("Can only queue callbacks when commands with 'IsFirstExecution' set to true are executing");
                return;
            }

            //CommandCallbacks.Add(new CommandCallbackItem { Command = command, Callback = callback, Start = -1, End = command.Number + delay, Mode = CommandCallbackModes.InvokeOnce });
        }

        public void InvokeRepeating(Command command, CommandCallback callback, int period)
        {
            NetAssert.True(period > 0);

            if (!canQueueCallbacks)
            {
                NetLog.Error("Can only queue callbacks when commands with 'IsFirstExecution' set to true are executing");
                return;
            }

            //CommandCallbacks.Add(new CommandCallbackItem { Command = command, Callback = callback, Start = command.Number + 1, End = command.Number + period, Mode = CommandCallbackModes.InvokeRepeating });
        }

        public static Entity CreateFor(PrefabId prefabId, TypeId serializerId, Vector3 position, Quaternion rotation)
        {
            return CreateFor(Core.PrefabPool.Instantiate(prefabId, position, rotation), prefabId, serializerId);
        }

        public static Entity CreateFor(GameObject instance, PrefabId prefabId, TypeId serializerId)
        {
            return CreateFor(instance, prefabId, serializerId, EntityFlags.ZERO);
        }

        public static Entity CreateFor(GameObject instance, PrefabId prefabId, TypeId serializerId, EntityFlags flags)
        {
            Entity eo;

            eo = new Entity();
            eo.UnityObject = instance.GetComponent<AscensionEntity>();
            eo.UpdateRate = eo.UnityObject.updateRate;
            eo.AutoFreezeProxyFrames = eo.UnityObject.autoFreezeProxyFrames;
            eo.AllowFirstReplicationWhenFrozen = eo.UnityObject.allowFirstReplicationWhenFrozen;
            eo.AutoRemoveChildEntities = eo.UnityObject.autoRemoveChildEntities;
            eo.PrefabId = prefabId;
            eo.Flags = flags;

            if (prefabId.Value == 0)
            {
                eo.Flags |= EntityFlags.SCENE_OBJECT;
                eo.SceneId = eo.UnityObject.SceneGuid;
            }

            if (eo.UnityObject.persistThroughSceneLoads) { eo.Flags |= EntityFlags.PERSIST_ON_LOAD; }
            if (eo.UnityObject.clientPredicted) { eo.Flags |= EntityFlags.CONTROLLER_LOCAL_PREDICTION; }

            // create serializer
            eo.Serializer = Factory.NewSerializer(serializerId);
            eo.Serializer.OnCreated(eo);


            // done
            return eo;
        }

        public static implicit operator bool(Entity entity)
        {
            return entity != null;
        }

        public static bool operator ==(Entity a, Entity b)
        {
            return ReferenceEquals(a, b);
        }

        public static bool operator !=(Entity a, Entity b)
        {
            return ReferenceEquals(a, b) == false;
        }

        bool IPriorityCalculator.Always
        {
            get { return false; }
        }

        float IPriorityCalculator.CalculateStatePriority(Connection connection, int skipped)
        {
            return Mathf.Max(1, skipped);
        }

        float IPriorityCalculator.CalculateEventPriority(Connection connection, Event evnt)
        {
            if (HasControl)
            {
                return 3;
            }

            if (IsController(connection))
            {
                return 2;
            }

            return 1;
        }

        bool IEntityReplicationFilter.AllowReplicationTo(Connection connection)
        {
            return true;
        }
    }
}
