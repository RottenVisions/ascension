using System;
using UnityEngine;
using System.Collections;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    [ExecuteInEditMode]
    [AscensionExecutionOrder(-2500)]
    public class AscensionEntity : MonoBehaviour, IListNode
    {
        object IListNode.Prev { get; set; }
        object IListNode.Next { get; set; }
        object IListNode.List { get; set; }

        public Entity entity;

        [SerializeField]
        public string sceneGuid;

        [SerializeField]
        public string serializerGuid;

        [SerializeField]
        public int prefabId = -1;

        [SerializeField]
        public int updateRate = 1;

        [SerializeField]
        public int autoFreezeProxyFrames = 0;

        [SerializeField]
        public bool clientPredicted = true;

        [SerializeField]
        public bool allowInstantiateOnClient = true;

        [SerializeField]
        public bool persistThroughSceneLoads = false;

        [SerializeField]
        public bool sceneObjectDestroyOnDetach = false;

        [SerializeField]
        public bool sceneObjectAutoAttach = true;

        [SerializeField]
        public bool alwaysProxy = false;

        [SerializeField]
        public bool detachOnDisable = true;

        [SerializeField]
        public bool allowFirstReplicationWhenFrozen = false;

        [SerializeField]
        public bool autoRemoveChildEntities = false;

        public Entity AEntity
        {
            get
            {
                if (entity == null)
                {
                    throw new AscensionException("You can't access any Ascension specific methods or properties on an entity which is detached");
                }

                return entity;
            }
        }

        public UniqueId SceneGuid
        {
            get { return UniqueId.Parse(sceneGuid); }
            set { sceneGuid = value.guid.ToString(); }
        }

        public UniqueId SerializerGuid
        {
            get { return UniqueId.Parse(serializerGuid); }
            set { serializerGuid = value.guid.ToString(); }
        }

        /// <summary>
        /// The prefabId used to instantiate this entity
        /// </summary>
        public PrefabId PrefabId
        {
            get { return new PrefabId(prefabId); }
        }

        /// <summary>
        /// If this entity was created on another computer, contains the connection we received this entity from, otherwise null
        /// </summary>
        public Connection Source
        {
            get { return AEntity.Source; }
        }

        public IMessageRider AttachToken
        {
            get { return AEntity.AttachToken; }
        }

        public IMessageRider DetachToken
        {
            get { return AEntity.DetachToken; }
        }

        public IMessageRider ControlGainedToken
        {
            get { return AEntity.ControlGainedToken; }
        }

        public IMessageRider ControlLostToken
        {
            get { return AEntity.ControlLostToken; }
        }

        /// <summary>
        /// The unique id of this entity
        /// </summary>
        public NetworkId NetworkId
        {
            get { return AEntity.NetworkId; }
        }

        /// <summary>
        /// Whether the entity can be paused / frozen
        /// </summary>
        public bool CanFreeze
        {
            get { return AEntity.CanFreeze; }
            set { AEntity.CanFreeze = value; }
        }

        /// <summary>
        /// If this entity is controlled by a remote connection it contains that connection, otherwise null
        /// </summary>
        public Connection Controller
        {
            get { return AEntity.Controller; }
        }

        public bool IsControllerOrOwner
        {
            get { return HasControl || IsOwner; }
        }

        /// <summary>
        /// If this entity is attached to Ascension or not
        /// </summary>
        public bool IsAttached
        {
            get { return (entity != null) && entity.IsAttached; }
        }

        /// <summary>
        /// If this entity is currently paused
        /// </summary>
        public bool IsFrozen
        {
            get { return IsAttached && AEntity.IsFrozen; }
        }

        public bool IsControlled 
        {
            get { return HasControl || Controller != null; }
        }

        /// <summary>
        /// This is a scene object placed in the scene in the Unity editor
        /// </summary>
        public bool IsSceneObject
        {
            get { return AEntity.IsSceneObject; }
        }

        /// <summary>
        /// Did the local computer create this entity or not?
        /// </summary>
        public bool IsOwner
        {
            get { return AEntity.IsOwner; }
        }

        /// <summary>
        /// Do we have control of this entity?
        /// </summary>
        public bool HasControl
        {
            get { return AEntity.HasControl; }
        }

        /// <summary>
        /// Do we have control of this entity and are we using client side prediction
        /// </summary>
        public bool HasControlWithPrediction
        {
            get { return AEntity.HasPredictedControl; }
        }

        /// <summary>
        /// Should this entity persist between scene loads
        /// </summary>
        public bool PersistsOnSceneLoad
        {
            get { return AEntity.PersistsOnSceneLoad; }
        }

        /// <summary>
        /// Creates an object which lets you modify the public settings of an entity before it is attached to Ascension.
        /// </summary>
        /// <returns>The object used to modify entity settings</returns>
        public AscensionEntitySettingsModifier ModifySettings()
        {
            VerifyNotAttached();
            return new AscensionEntitySettingsModifier(this);
        }
        /// <summary>
        /// Sets the scope of all currently active connections for this entity. Only usable if Scope Mode has been set to Manual.
        /// </summary>
        /// <param name="inScope">If this entity should be in scope or not</param>
        public void SetScopeAll(bool inScope)
        {
            AEntity.SetScopeAll(inScope);
        }

        /// <summary>
        /// Sets the scope for the connection passed in for this entity. Only usable if Scope Mode has been set to Manual.
        /// </summary>
        /// <param name="connection">The connection being scoped</param>
        /// <param name="inScope">If this entity should be in scope or not</param>
        public void SetScope(Connection connection, bool inScope)
        {
            AEntity.SetScope(connection, inScope);
        }

        /// <summary>
        /// Sets the parent of this entity
        /// </summary>
        public void SetParent(AscensionEntity parent)
        {
            if (parent && parent.IsAttached)
            {
                AEntity.SetParent(parent.AEntity);
            }
            else
            {
                AEntity.SetParent(null);
            }
        }

        /// <summary>
        /// Takes local control of this entity
        /// </summary>
        public void TakeControl()
        {
            AEntity.TakeControl(null);
        }

        /// <summary>
        /// Takes local control of this entity
        /// </summary>
        /// <param name="token">A data token of max size 512 bytes</param>
        public void ReleaseControl()
        {
            AEntity.ReleaseControl(null);
        }

        /// <summary>
        /// Releases local control of this entity
        /// </summary>
        public void ReleaseControl(IMessageRider token)
        {
            AEntity.ReleaseControl(token);
        }

        /// <summary>
        /// Assigns control of this entity to a connection
        /// </summary>
        public void AssignControl(Connection connection)
        {
            AEntity.AssignControl(connection, null);
        }

        /// <summary>
        /// Assigns control of this entity to a connection
        /// </summary>
        /// <param name="connection">The connection to assign control to</param>
        /// <param name="token">A data token of max size 512 bytes</param>
        public void AssignControl(Connection connection, IMessageRider token)
        {
            AEntity.AssignControl(connection, token);
        }

        /// <summary>
        /// Revokes control of this entity from a connection
        /// </summary>
        public void RevokeControl()
        {
            AEntity.RevokeControl(null);
        }

        /// <summary>
        /// Revokes control of this entity from a connection
        /// </summary>
        /// <param name="token">A data token of max size 512 bytes</param>
        public void RevokeControl(IMessageRider token)
        {
            AEntity.RevokeControl(token);
        }

        /// <summary>
        /// Checks if this entity is being controlled by the connection
        /// </summary>
        /// <param name="connection">The connection to check</param>
        public bool IsController(Connection connection)
        {
            return ReferenceEquals(AEntity.Controller, connection);
        }

        /// <summary>
        /// Queue an input data on this entity for execution. This is called on a client which is 
        /// controlling a proxied entity. The data will be sent to the server for authoritative execution
        /// </summary>
        /// <param name="data">The input data to queue</param>
        public bool QueueInput(INetworkCommandData data)
        {
            return AEntity.QueueInput(((NetworkCommand_Data)data).RootCommand);
        }

        /// <summary>
        /// Set this entity as idle on the supplied connection, this means that the connection 
        /// will not receive update state for this entity as long as it's idle.
        /// </summary>
        /// <param name="connection">The connection to idle the entity on</param>
        /// <param name="idle">If this should be idle or not</param>
        public void Idle(Connection connection, bool idle)
        {
            AEntity.SetIdle(connection, idle);
        }

        /// <summary>
        /// Freeze or unfreeze an entity
        /// </summary>
        public void Freeze(bool pause)
        {
            AEntity.Freeze(pause);
        }

        /// <summary>
        /// Add an event listener to this entity.
        /// </summary>
        /// <param name="behaviour">The behaviour to invoke event callbacks on</param>
        public void AddEventListener(MonoBehaviour behaviour)
        {
            AEntity.AddEventListener(behaviour);
        }

        /// <summary>
        /// Add a event callback to this entity.
        /// </summary>
        public void AddEventCallback<T>(Action<T> callback) where T : Event
        {
            AEntity.EventDispatcher.Add<T>(callback);
        }

        /// <summary>
        /// Remove an event listern from this entity
        /// </summary>
        /// <param name="behaviour">The behaviour to remove</param>
        public void RemoveEventListener(MonoBehaviour behaviour)
        {
            AEntity.RemoveEventListener(behaviour);
        }

        /// <summary>
        /// Remove a event callback to this entity.
        /// </summary>
        public void RemoveEventCallback<T>(Action<T> callback) where T : Event
        {
            AEntity.EventDispatcher.Remove<T>(callback);
        }

        /// <summary>
        /// Get the state if this entity
        /// </summary>
        public TState GetState<TState>()
        {
            if (AEntity.Serializer is TState)
            {
                return (TState)(object)AEntity.Serializer;
            }

            NetLog.Error("You are trying to access the state of {0} as '{1}'", AEntity, typeof(TState));
            return default(TState);
        }

        /// <summary>
        /// A null safe way to look for a specific type of state on an entity
        /// </summary>
        /// <typeparam name="TState">The state type to search for</typeparam>
        /// <param name="state">Entity to search</param>
        public bool TryFindState<TState>(out TState state)
        {
            if (AEntity.Serializer is TState)
            {
                state = (TState)(object)AEntity.Serializer;
                return true;
            }

            state = default(TState);
            return false;
        }

        /// <summary>
        /// Checks which type of state this entity has
        /// </summary>
        public bool StateIs<TState>()
        {
            return AEntity.Serializer is TState;
        }

        /// <summary>
        /// Checks which type of state this entity has
        /// </summary>
        public bool StateIs(Type t)
        {
            return t.IsAssignableFrom(AEntity.Serializer.GetType());
        }

        /// <summary>
        /// String representation of the entity
        /// </summary>
        public override string ToString()
        {
            if (IsAttached)
            {
                return AEntity.ToString();
            }
            else
            {
                return string.Format("[DetachedEntity {2} SceneId={0} SerializerId={1} {3}]", SceneGuid, SerializerGuid, PrefabId, gameObject.name);
            }
        }

        public void VerifyNotAttached()
        {
            if (IsAttached)
            {
                throw new InvalidOperationException("You can't modify a AscensionEntity behaviour which is attached to Ascension");
            }
        }

        /// <summary>
        /// Destroy this entity after a given delay
        /// </summary>
        public void DestroyDelayed(float time)
        {
            StartCoroutine(DestroyDelayedInternal(time));
        }

        public void InvokeOnce(Command cmd, int delay, CommandCallback callback)
        {
            AEntity.InvokeOnce(cmd, callback, delay);
        }

        public void InvokeMany(Command cmd, int duration, CommandCallback callback)
        {
            AEntity.InvokeRepeating(cmd, callback, duration);
        }

        IEnumerator DestroyDelayedInternal(float time)
        {
            yield return new WaitForSeconds(time);

            if (IsAttached)
            {
                AscensionNetwork.Destroy(gameObject);
            }
        }

        void Awake()
        {
            // only in the editor
            if ((Application.isEditor == true) && (Application.isPlaying == false))
            {
                // check if we don't have a valid scene guid
                if (SceneGuid == UniqueId.None)
                {
                    // set a new one
                    SceneGuid = UniqueId.New();

                    // tell editor to save us
                    AscensionCoreInternal.ChangedEditorEntities.Add(this);
                }
            }
        }

        void OnDisable()
        {
            if (detachOnDisable)
            {
                if (Application.isPlaying)
                {
                    OnDestroy();
                }
            }
        }

        void OnDestroy()
        {
            if (entity && entity.IsAttached && Application.isPlaying)
            {
                if (entity.IsOwner)
                {
                    NetLog.Warn("{0} is being destroyed/disabled without being detached, forcing detach", AEntity);
                }
                else
                {
                    NetLog.Error("{0} is being destroyed/disabled without being detached by the owner, this will cause this peer to disconnect the next time it receives an update for this entity", AEntity);
                }

                // force detach
                entity.Detach();
                entity = null;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "AscensionEntity Gizmo", true);
        }

        //void Update() {
        //  if (isAttached && Application.isPlaying) {
        //    AEntity.Render();
        //  }
        //}

        public static implicit operator GameObject(AscensionEntity entity)
        {
            return entity == null ? null : entity.gameObject;
        }

    }


    /// <summary>
    /// Modifier for Ascension entity settings before it's attached
    /// </summary>
    public class AscensionEntitySettingsModifier : IDisposable
    {
        AscensionEntity entity;

        public AscensionEntitySettingsModifier(AscensionEntity entity)
        {
            this.entity = entity;
        }

        /// <summary>
        /// The prefab identifier
        /// </summary>
        public PrefabId PrefabId
        {
            get { return entity.PrefabId; }
            set { entity.VerifyNotAttached(); entity.prefabId = value.Value; }
        }

        /// <summary>
        /// A unique identifier present on scene entities
        /// </summary>
        public UniqueId SceneId
        {
            get { return entity.SceneGuid; }
            set { entity.VerifyNotAttached(); entity.SceneGuid = value; }
        }

        /// <summary>
        /// A unique identifier of this entity state serializer
        /// </summary>
        public UniqueId SerializerId
        {
            get { return entity.SerializerGuid; }
            set { entity.VerifyNotAttached(); entity.SerializerGuid = value; }
        }

        /// <summary>
        /// The network update rate for this entity
        /// </summary>
        public int UpdateRate
        {
            get { return entity.updateRate; }
            set { entity.VerifyNotAttached(); entity.updateRate = value; }
        }

        /// <summary>
        /// The network update rate for this entity
        /// </summary>
        public int AutoFreezeProxyFrames
        {
            get { return entity.autoFreezeProxyFrames; }
            set { entity.VerifyNotAttached(); entity.autoFreezeProxyFrames = value; }
        }


        /// <summary>
        /// Enable or disable client prediction on the entity
        /// </summary>
        public bool ClientPredicted
        {
            get { return entity.clientPredicted; }
            set { entity.VerifyNotAttached(); entity.clientPredicted = value; }
        }

        /// <summary>
        /// Enable or disable instantiation of the entity by clients
        /// </summary>
        public bool AllowInstantiateOnClient
        {
            get { return entity.allowInstantiateOnClient; }
            set { entity.VerifyNotAttached(); entity.allowInstantiateOnClient = value; }
        }

        /// <summary>
        /// Whether the entity is persistence between scenes
        /// </summary>
        public bool PersistThroughSceneLoads
        {
            get { return entity.persistThroughSceneLoads; }
            set { entity.VerifyNotAttached(); entity.persistThroughSceneLoads = value; }
        }

        /// <summary>
        /// True if the entity should be destroyed when detached
        /// </summary>
        public bool SceneObjectDestroyOnDetach
        {
            get { return entity.sceneObjectDestroyOnDetach; }
            set { entity.VerifyNotAttached(); entity.sceneObjectDestroyOnDetach = value; }
        }

        /// <summary>
        /// True if Ascension should automatically attach the entity during instantiation
        /// </summary>
        public bool SceneObjectAutoAttach
        {
            get { return entity.sceneObjectAutoAttach; }
            set { entity.VerifyNotAttached(); entity.sceneObjectAutoAttach = value; }
        }

        /// <summary>
        /// True if this entity is always owned by the server
        /// </summary>
        public bool AlwaysProxy
        {
            get { return entity.alwaysProxy; }
            set { entity.VerifyNotAttached(); entity.alwaysProxy = value; }
        }

        void IDisposable.Dispose()
        {

        }
    }
}

