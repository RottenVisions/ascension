#define DEBUG
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Ascender;
using Ascender.Utils;
using Ascension.Networking.Physics;
using Ascension.Networking.Sockets;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using ConnectionState = Ascension.Networking.Sockets.ConnectionState;

namespace Ascension.Networking
{
    public class Core : MonoBehaviour
    {
        static Stopwatch timer = new Stopwatch();
        static SceneLoadState localSceneLoading;

        static bool canReceiveEntities = true;
        static IPrefabPool prefabPool = new DefaultPrefabPool();
        static IEventFilter eventFilter = new DefaultEventFilter();

        private static SocketInterface sockInt;
        private static SocketEvent ev;

        private static int frame = 0;
        private static NetworkModes mode = NetworkModes.None;

        private static RuntimeSettings config = null;
        private static AscensionConfig ascenConf;

        public static ListExtended<Entity> entitiesThawed = new ListExtended<Entity>();
        public static ListExtended<Entity> entitiesFrozen = new ListExtended<Entity>();

        public static ListExtended<Connection> connections = new ListExtended<Connection>();
        static EventDispatcher globalEventDispatcher = new EventDispatcher();
        static Dictionary<UniqueId, AscensionEntity> sceneObjects = new Dictionary<UniqueId, AscensionEntity>(UniqueId.EqualityComparer.Instance);

        static GameObject globalControlObject = null;
        static GameObject globalBehaviorObject = null;
        static List<NetTuple<GlobalBehaviorAttribute, Type>> globalBehaviours = new List<NetTuple<GlobalBehaviorAttribute, Type>>();

        private static PacketPool packetPool;
        //Fragmented data
        private static bool receivingFragmentedData;
        private static List<byte[]> fragmentedData;

        static Action<string> mapLoadStartAction;
        static string mapLoadStartName;
        static string autoloadScene = "";

        public static Action<string> MapLoadStartAction
        {
            set { mapLoadStartAction = value; }
        }

        public static string MapLoadStartName
        {
            set { mapLoadStartName = value; }
        }

        public static NetworkModes Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public static RuntimeSettings Config
        {
            get { return config; }
        }

        public static AscensionConfig AscensionConf
        {
            get { return ascenConf; }
        }

        public static Dictionary<UniqueId, AscensionEntity> SceneObjects
        {
            get { return sceneObjects; }
        }

        static internal IEnumerable<Entity> entities
        {
            get
            {
                var it = entitiesThawed.GetIterator();

                while (it.Next())
                {
                    yield return it.val;
                }

                it = entitiesFrozen.GetIterator();

                while (it.Next())
                {
                    yield return it.val;
                }
            }
        }

        public static IEnumerable<AscensionEntity> Entities
        {
            get { return entities.Select(x => x.UnityObject); }
        }

        public static IEnumerable<Connection> Connections
        {
            get { return connections; }
        }
        public static IPrefabPool PrefabPool
        {
            get { return prefabPool; }
            set { prefabPool = value; }
        }

        public static IEventFilter EventFilter
        {
            get { return eventFilter; }
            set { eventFilter = value; }
        }

        public static IEnumerable<Connection> Clients
        {
            get { return connections.Where(c => c.SockConn.IsHost); }
        }

        public static Connection Server
        {
            get { return connections.FirstOrDefault(c => c.SockConn.IsClient); }
        }

        public static EventDispatcher GlobalEventDispatcher
        {
            get { return globalEventDispatcher; }
        }

        public static GameObject GlobalObject
        {
            get { return globalBehaviorObject; }
        }

        public static Stopwatch Timer
        {
            get { return timer; }
        }

        public static SceneLoadState LocalSceneLoading
        {
            get { return localSceneLoading; }
        }

        public static SocketInterface SockInt
        {
            get { return sockInt; }
        }

        public static int FramesPerSecond
        {
            get { return config.framesPerSecond; }
        }

        public static int NetworkRate
        {
            get
            {
                int rate = 0;

                if (FramesPerSecond == ((FramesPerSecond / LocalSendRate) * LocalSendRate))
                {
                    rate = (FramesPerSecond / LocalSendRate);
                }
                else
                {
                    rate = (int)Math.Round((float)FramesPerSecond / LocalSendRate, 2);
                }
                return rate;
            }

        }

        public static bool ThrottleSendRate
        {
            get { return config.allowSendRateThrottling; }
        }

        public static int ServerFrame
        {
            get { return mode == NetworkModes.None ? 0 : (IsServer ? frame : Server.RemoteFrame); }
        }

        public static float ServerTime
        {
            get { return ((float)ServerFrame) / ((float)FramesPerSecond); }
        }

        public static float CurrentTime
        {
            get { return Time.time; }
        }

        public static float FrameBeginTime
        {
            get { return Time.fixedTime; }
        }

        public static float FrameDeltaTime
        {
            get { return Time.fixedDeltaTime; }
        }

        public static float FrameAlpha
        {
            get { return Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime); }
        }

        public static int Frame
        {
            get { return frame; }
        }

        public static bool CanReceiveEntities
        {
            get { return canReceiveEntities; }
            set { canReceiveEntities = value; }
        }

        public static bool IsClient
        {
            get { return HasSocket && mode == NetworkModes.Client; }
        }

        public static bool IsServer
        {
            get { return HasSocket && mode == NetworkModes.Host; }
        }

        private static bool HasSocket
        {
            get { return SockInt != null; }
        }

        private static bool HasSocketPeer
        {
            get { return SockInt.socketPeer != null; }
        }

        public static int LocalSendRate
        {
            get
            {
                switch (mode)
                {
                    case NetworkModes.Host: return config.serverSendRate;
                    case NetworkModes.Client: return config.clientSendRate;
                    default: return -1;
                }
            }
        }

        public static int RemoteSendRate
        {
            get
            {
                switch (mode)
                {
                    case NetworkModes.Host: return config.clientSendRate;
                    case NetworkModes.Client: return config.serverSendRate;
                    default: return -1;
                }
            }
        }

        public static int LocalInterpolationDelay
        {
            get
            {
                switch (mode)
                {
                    case NetworkModes.Host: return config.serverDejitterDelay;
                    case NetworkModes.Client: return config.clientDejitterDelay;
                    default: return -1;
                }
            }
        }

        public static int LocalInterpolationDelayMin
        {
            get
            {
                switch (mode)
                {
                    case NetworkModes.Host: return config.serverDejitterDelayMin;
                    case NetworkModes.Client: return config.clientDejitterDelayMin;
                    default: return -1;
                }
            }
        }

        public static int LocalInterpolationDelayMax
        {
            get
            {
                switch (mode)
                {
                    case NetworkModes.Host: return config.serverDejitterDelayMax;
                    case NetworkModes.Client: return config.clientDejitterDelayMax;
                    default: return -1;
                }
            }
        }

        #region Debug
        public static bool IsDebugMode
        {
#if DEBUG
            get { return true; }
#else
            get { return false; }
#endif
        }

        public static Func<GameObject, Vector3, Quaternion, GameObject> instantiate =
  (go, p, r) => (GameObject)UnityEngine.GameObject.Instantiate(go, p, r);

        public static Action<GameObject> destroy =
          (go) => GameObject.Destroy(go);

        public static int LoadedScene
        {
            get { return localSceneLoading.Scene.Index; }
        }

        public static string LoadedSceneName
        {
            get { return AscensionNetworkInternal.GetSceneName(localSceneLoading.Scene.Index); }
        }

        public static GameObject globalObject
        {
            get { return globalBehaviorObject; }
        }

#if DEBUG
        static Func<float> CreatePerlinNoise()
        {
            var x = UnityEngine.Random.value;
            var s = Stopwatch.StartNew();
            return () => Mathf.PerlinNoise(x, (float)s.Elapsed.TotalSeconds);
        }

        static Func<float> CreateRandomNoise()
        {
            var r = new System.Random();
            return () => (float)r.NextDouble();
        }
#endif
        static void SocketLogWriter(uint level, string message)
        {
#if DEBUG
            switch (level)
            {
                case SocketLog.DEBUG:
                case SocketLog.TRACE:
                    NetLog.Debug(message);
                    break;

                case SocketLog.INFO:
                    NetLog.Info(message);
                    break;

                case SocketLog.WARN:
                    NetLog.Warn(message);
                    break;

                case SocketLog.ERROR:
                    NetLog.Error(message);
                    break;
            }
#endif
        }

        static void UnityLogCallback(string condition, string stackTrace, LogType type)
        {
            stackTrace = (stackTrace ?? "").Trim();

            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    NetLog.Error(condition);

                    if (stackTrace.Length > 0)
                    {
                        NetLog.Error(stackTrace);
                    }
                    break;

                case LogType.Log:
                    NetLog.Info(condition);

                    if (stackTrace.Length > 0)
                    {
                        NetLog.Info(stackTrace);
                    }
                    break;

                case LogType.Warning:
                    NetLog.Warn(condition);

                    if (stackTrace.Length > 0)
                    {
                        NetLog.Warn(stackTrace);
                    }
                    break;
            }
        }
        #endregion

        #region Connect
        public static void Connect(string address, int port, IMessageRider token)
        {
            if (Server != null)
            {
                NetLog.Error("You must disconnect from the current server first");
                return;
            }

            IPEndPoint endPoint = Utils.CreateIPEndPoint(address, port);

            // Attempt connection, handle failure
            if (sockInt.Connect(token, address, port))
            {
                //Successful connect attempt
                GlobalEventListenerBase.ConnectAttemptInvoke(endPoint);

            }
            else
            {
                //Failed to connect
                HandleConnectFailed(endPoint);
            }
        }

        public static void Connect(IPEndPoint endPoint, IMessageRider token)
        {
            if (Server != null)
            {
                NetLog.Error("You must disconnect from the current server first");
                return;
            }

            // Attempt connection, handle failure
            if (sockInt.Connect(endPoint, token))
            {
                //Successful connect attempt
                GlobalEventListenerBase.ConnectAttemptInvoke(endPoint);
            }
            else
            {
                //Failed to connect
                HandleConnectFailed(endPoint);
            }
        }

        public static void Connect(SocketEndPoint socketEndPoint, IMessageRider token)
        {
            if (Server != null)
            {
                NetLog.Error("You must disconnect from the current server first");
                return;
            }

            //Prepare an endpoint
            IPEndPoint endPoint = Utils.CreateIPEndPoint(socketEndPoint.address, socketEndPoint.port);

            // Attempt connection, handle failure
            if (sockInt.Connect(endPoint, token))
            {
                //Successful connect attempt
                GlobalEventListenerBase.ConnectAttemptInvoke(endPoint);
            }
            else
            {
                //Failed to connect
                HandleConnectFailed(endPoint);
            }
        }


        #endregion

        #region Instantiate / Destroy
        public static void Destroy(AscensionEntity entity, IMessageRider detachToken)
        {
            if (!entity.IsOwner)
            {
                NetLog.Warn("Only the owner can destroy an entity, ignoring call to Destroy().");
                return;
            }

            if (!entity.IsAttached)
            {
                NetLog.Warn("Entity is not attached, ignoring call to Destroy().");
                return;
            }

            entity.AEntity.DetachToken = detachToken;
            DestroyForce(entity.AEntity);
        }

        public static void DestroyForce(Entity entity)
        {
            // detach
            entity.Detach();

            // destroy
            PrefabPool.Destroy(entity.UnityObject.gameObject);
        }

        public static AscensionEntity Instantiate(PrefabId prefabId, TypeId serializerId, Vector3 position, Quaternion rotation, InstantiateFlags instanceFlags, Connection controller, IMessageRider attachToken)
        {
            // prefab checks
            GameObject prefab = PrefabPool.LoadPrefab(prefabId);
            AscensionEntity entity = prefab.GetComponent<AscensionEntity>();

            if (IsClient && (entity.allowInstantiateOnClient == false))
            {
                throw new AscensionException("This prefab is not allowed to be instantiated on clients");
            }

            if (entity.prefabId != prefabId.Value)
            {
                throw new AscensionException("PrefabId for AscensionEntity component did not return the same value as prefabId passed in as argument to Instantiate");
            }

            Entity eo;
            eo = Entity.CreateFor(prefabId, serializerId, position, rotation);
            eo.Initialize();
            eo.AttachToken = attachToken;
            eo.Attach();

            return eo.UnityObject;
        }
        #endregion

        #region Attach / Detach
        public static GameObject Attach(GameObject gameObject, EntityFlags flags)
        {
            return Attach(gameObject, flags, null);
        }

        public static GameObject Attach(GameObject gameObject, EntityFlags flags, IMessageRider attachToken)
        {
            AscensionEntity ae = gameObject.GetComponent<AscensionEntity>();
            return Attach(gameObject, Factory.GetFactory(ae.SerializerGuid).TypeId, flags, attachToken);
        }

        public static GameObject Attach(GameObject gameObject, TypeId serializerId, EntityFlags flags, IMessageRider attachToken)
        {
            AscensionEntity be = gameObject.GetComponent<AscensionEntity>();

            Entity en;
            en = Entity.CreateFor(gameObject, new PrefabId(be.prefabId), serializerId, flags);
            en.Initialize();
            en.AttachToken = attachToken;
            en.Attach();

            return en.UnityObject.gameObject;
        }

        public static void Detach(AscensionEntity entity, IMessageRider detachToken)
        {
            NetAssert.NotNull(entity.AEntity);
            entity.AEntity.DetachToken = detachToken;
            entity.AEntity.Detach();
        }
        #endregion

        #region Scene Loading

        public static void LoadScene(int index, IMessageRider token)
        {
            if (IsServer == false)
            {
                NetLog.Error("You are not the server, only the server can initiate a scene load");
                return;
            }

            // pass to public call
            LoadSceneInternal(localSceneLoading.BeginLoad(index, token));
        }


        public static void LoadSceneInternal(SceneLoadState loading)
        {
            // update
            localSceneLoading = loading;

            // begin loading
            SceneLoader.Enqueue(localSceneLoading);
        }

        public static void SceneLoadBegin(SceneLoadState state)
        {
            foreach (var itval in entities)
            {
                if (itval.IsOwner && (itval.PersistsOnSceneLoad == false))
                {
                    DestroyForce(itval);
                }
            }

            // clear out scene entities
            sceneObjects = new Dictionary<UniqueId, AscensionEntity>();

            // update behaviours
            UpdateActiveGlobalBehaviours(state.Scene.Index);

            // call out to user code
            GlobalEventListenerBase.SceneLoadLocalBeginInvoke(AscensionNetworkInternal.GetSceneName(state.Scene.Index));

            if (state.Token != null)
            {
                GlobalEventListenerBase.SceneLoadLocalBeginInvoke(AscensionNetworkInternal.GetSceneName(state.Scene.Index), state.Token);
            }
        }

        public static void SceneLoadDone(SceneLoadState state)
        {
            // switch local state
            if (state.Scene == localSceneLoading.Scene)
            {
                localSceneLoading.State = SceneLoadState.STATE_LOADING_DONE;
            }

            // 
            UpdateSceneObjectsLookup();

            // call out to user code
            GlobalEventListenerBase.SceneLoadLocalDoneInvoke(AscensionNetworkInternal.GetSceneName(state.Scene.Index));

            if (state.Token != null)
            {
                GlobalEventListenerBase.SceneLoadLocalDoneInvoke(AscensionNetworkInternal.GetSceneName(state.Scene.Index), state.Token);
            }
        }
        #endregion

        #region Behaviors

        static public void UpdateActiveGlobalBehaviours(int index)
        {
#if DEBUG
            var useConsole = (RuntimeSettings.Instance.logTargets & ConfigLogTargets.Console) == ConfigLogTargets.Console;
            if (useConsole)
            {
                NetConsole console = CreateGlobalBehavior(typeof(NetConsole)) as NetConsole;

                if (console)
                {
                    console.toggleKey = RuntimeSettings.Instance.consoleToggleKey;
                    console.visible = RuntimeSettings.Instance.consoleVisibleByDefault;
                }
            }
            else
            {
                DeleteGlobalBehavior(typeof(NetConsole));
            }
#endif

            CreateGlobalBehavior(typeof(Poll));
            CreateGlobalBehavior(typeof(Send));
            CreateGlobalBehavior(typeof(SceneLoader));

            foreach (var pair in globalBehaviours)
            {
                if ((pair.item0.Mode & mode) == mode)
                {
                    var anyMap = ((pair.item0.Scenes == null) || (pair.item0.Scenes.Length == 0)) && ((pair.item0.Scenes == null) || (pair.item0.Scenes.Length == 0));
                    var nameMatches = (index >= 0) && (pair.item0.Scenes != null) && (Array.FindIndex<string>(pair.item0.Scenes, v => Regex.IsMatch(AscensionNetworkInternal.GetSceneName(index), v)) != -1);

                    if (anyMap || nameMatches)
                    {
                        CreateGlobalBehavior(pair.item1);
                    }
                    else
                    {
                        DeleteGlobalBehavior(pair.item1);
                    }
                }
                else
                {
                    DeleteGlobalBehavior(pair.item1);
                }
            }
        }

        static Component CreateGlobalBehavior(Type t)
        {
            if (globalBehaviorObject)
            {
                var component = globalBehaviorObject.GetComponent(t);
                var shouldCreate = !component;

                if (shouldCreate)
                {
                    NetLog.Debug("Creating Global Behavior: '{0}'", t);
                    return globalBehaviorObject.AddComponent(t);
                }
            }

            return null;
        }

        static void DeleteGlobalBehavior(Type t)
        {
            if (globalBehaviorObject)
            {
                var component = globalBehaviorObject.GetComponent(t);
                var shouldDelete = component;

                if (shouldDelete)
                {
                    NetLog.Debug("Deleting Global Behavior: '{0}'", t);
                    Destroy(component);
                }
            }
        }

        static void CreateAscensionBehaviorObject()
        {
            // create the global 'Ascension' unity object
            if (globalBehaviorObject)
            {
                Destroy(globalBehaviorObject);
            }

            globalBehaviours = AscensionNetworkInternal.GetGlobalBehaviourTypes();
            globalBehaviorObject = new GameObject("AscensionBehaviors");

            DontDestroyOnLoad(globalBehaviorObject);
        }

        #endregion

        #region Utility

        public static void UpdateSceneObjectsLookup()
        {
            // grab all scene entities
            sceneObjects =
             GameObject.FindObjectsOfType(typeof(AscensionEntity))
                .Cast<AscensionEntity>()
                .Where(x => x.SceneGuid != UniqueId.None)
                .ToDictionary(x => x.SceneGuid);

            // how many?
            NetLog.Debug("Found {0} Scene Objects", sceneObjects.Count);

            // update settings
            foreach (var se in sceneObjects.Values)
            {
                // attach on server
                if (IsServer && (se.IsAttached == false) && se.sceneObjectAutoAttach)
                {
                    AscensionEntity entity;

                    entity = Attach(se.gameObject, EntityFlags.SCENE_OBJECT).GetComponent<AscensionEntity>();
                    entity.AEntity.SceneId = se.SceneGuid;
                }
            }
        }

        public static GameObject FindSceneObject(UniqueId uniqueId)
        {
            AscensionEntity entity;

            if (sceneObjects.TryGetValue(uniqueId, out entity))
            {
                return entity.gameObject;
            }

            return null;
        }

        public static Packet AllocatePacket()
        {
            return packetPool.Acquire();
        }

        public static BasePacket AllocateBasePacket()
        {
            return sockInt.PacketPool.Acquire();
        }

        public static Entity FindEntity(NetworkId id)
        {
            // remap network id to local id
            if ((id.Connection == uint.MaxValue) && (NetworkIdAllocator.LocalConnectionId != uint.MaxValue))
            {
                id = new NetworkId(NetworkIdAllocator.LocalConnectionId, id.Entity);
            }

            foreach (var itval in entities)
            {
                if (itval.NetworkId == id)
                {
                    return itval;
                }
            }

            return null;
        }

        static void ResetIdAllocator(NetworkModes mode)
        {
            if (mode == NetworkModes.Host)
            {
                NetworkIdAllocator.Reset(1U); //U means send the digit unsigned, so 1 unsigned
            }
            else
            {
                NetworkIdAllocator.Reset(uint.MaxValue);
            }
        }

        static void CreateAscensionConfig(RuntimeSettings runConfig)
        {
            var isHost = mode == NetworkModes.Host;

            // setup ascension configuration
            ascenConf = new AscensionConfig();

            ascenConf.connectionLimit = isHost ? runConfig.maxDefaultConnections : 0;
            ascenConf.allowIncommingConnections = isHost;
            ascenConf.autoAcceptIncommingConnections = isHost && (runConfig.serverConnectionAcceptMode == ConnectionAcceptMode.Auto);

            //Shall we do additional configuration?
            AdditionalConfiguration(runConfig);
        }

        static void SetSimulation(RuntimeSettings runConfig)
        {
#if DEBUG
            if (runConfig.useNetworkSimulation)
            {
                ascenConf.simulatedLoss = Mathf.RoundToInt(runConfig.simulatedLoss * 100);

                //Set our simulation configuration
                sockInt.SetSimulation(runConfig.simulateLatency, runConfig.simulatePacketLoss, runConfig.simulationMaxLatency, ascenConf.simulatedLoss);
            }
#endif
        }

        static void AdditionalConfiguration(RuntimeSettings runConfig)
        {
            runConfig.pingTimeout = (uint)(LocalSendRate * 1.5f * FrameDeltaTime * 1000f);
        }

        static void SetExtraSettings()
        {
            // set counting LLAPI messages to the bandwidth
            sockInt.countLLAPIMessagesBandwidth = config.lowLevelMessagesIncrementBandwidth;

            // register our internal token
            AscensionNetwork.RegisterTokenClass<DisconnectionInfo>();
        }
        #endregion

        #region Start / Shutdown

        public static void BeginStart(ControlCommandStart cmd)
        {
            if (AscensionNetwork.IsRunning)
            {
                cmd.State = ControlState.Failed;

                // make sure we don't wait for this
                cmd.FinishedEvent.Set();

                // 
                throw new AscensionException("Ascension is already running, you must call AscensionLauncher.Shutdown() before starting a new instance of Ascension.");
            }

            // done!
            mode = cmd.Mode;
            config = cmd.Config;
            canReceiveEntities = true;

            // reset id allocator
            ResetIdAllocator(mode);

            // clear everything in console
            NetConsole.Clear();

            // setup debug info display
            if (config.showDebugInfo)
                DebugInfo.SetupAndShow();

            // setup logging
            NetLog.Setup(mode, config.logTargets);

            // tell user we're starting
            NetLog.Debug("Ascension starting with a simulation rate of {0} steps per second ({1}ms)", config.framesPerSecond, 1f / (float)config.framesPerSecond);

            // set udpkits log writer
            SocketLog.SetWriter(SocketLogWriter);

            // set frametime
            Time.fixedDeltaTime = 1f / (float)config.framesPerSecond;

            // create the gameobject that holds all Ascension global behaviours, etc.
            CreateAscensionBehaviorObject();

            // call to generated code so it knows we're starting
            if (AscensionNetworkInternal.EnvironmentSetup != null)
                AscensionNetworkInternal.EnvironmentSetup();

            // setup default local scene load state
            localSceneLoading = SceneLoadState.DefaultLocal();

            // create Socket
            sockInt = new SocketInterface();

            // create Ascension config (this must happen after initialization of the socket interface)
            CreateAscensionConfig(config);

            // create prefab cache
            PrefabDatabase.BuildCache();

            // init all global behaviours
            UpdateActiveGlobalBehaviours(-1);

            // set up packet pool
            packetPool = new PacketPool(sockInt);

            // invoke started
            GlobalEventListenerBase.AscensionStartBeginInvoke();

            // extra settings
            SetExtraSettings();

            //  start Socket
            sockInt.Initialize(cmd.EndPoint, cmd.FinishedEvent);

            //Set simulation after intialization
            SetSimulation(config);
        }

        public static void BeginShutdown(ControlCommandShutdown cmd)
        {
            if (!AscensionNetwork.IsRunning)
            {
                cmd.State = ControlState.Failed;
                cmd.FinishedEvent.Set();

                throw new AscensionException("Ascension is not running so it can't be shutdown");
            }

            // notify user code
            GlobalEventListenerBase.AscensionShutdownBeginInvoke(cmd.Callbacks.Add);

            //If we are a client, send a disconnect event immediately before continuing the shutdown process
            //Do this before resetting the mode to 'None'

            //Do not try to disconnect as the client if we are already disconnected
            if (IsClient && sockInt.IsConnected)
            {
                sockInt.Disconnect();
                //Purge both socket and normal connections

                //This currently does nothing
                if (connections.Count > 0)
                {
                    //We must purge the socket connection first as it depends upon the normal connection
                    PurgeConnection(connections[0]);
                }

            }
            //If we are the server, disconnect everyone connected
            if (IsServer)
            {
                //Create disconnection info
                DisconnectionInfo dInfo = new DisconnectionInfo();
                dInfo.reason = "Graceful Server Shutdown";
                dInfo.type = 1;
                //Disconnect all connections sending the reason
                for (int i = 0; i < connections.Count; i++)
                {
                    sockInt.Disconnect(connections[i].SockConn, dInfo);
                    //Purge both socket and normal connections
                    //We must purge the socket connection first as it depends upon the normal connection
                    //sockInt.PurgeConnection(connections[0].SockConn);
                    AttemptRemovalSocket(connections[i]);
                    PurgeConnection(connections[i]);
                }
            }

            mode = NetworkModes.Shutdown;

            // destroy all entities
            foreach (Entity entity in entities.ToArray())
            {
                try
                {
                    DestroyForce(entity);
                }
                catch { }
            }

            entitiesThawed.Clear();
            entitiesFrozen.Clear();
            connections.Clear();
            globalEventDispatcher.Clear();
            globalBehaviours.Clear();

            if (globalBehaviorObject)
            {
                // disables the immediate shutdown ascension does in the editor and OnApplicationQuit
                globalBehaviorObject.GetComponent<Poll>().AllowImmediateShutdown = false;

                // destroy everything!
                Destroy(globalBehaviorObject);
            }

            // reset environment stuff (we can probably remove this)
            if (AscensionNetworkInternal.EnvironmentReset != null)
                AscensionNetworkInternal.EnvironmentReset();

            // set a specific writer for this
            SocketLog.SetWriter((i, m) => UnityEngine.Debug.Log(m));

            // begin closing socket
            sockInt.Shutdown(cmd.FinishedEvent);

            // unset net event listener
            UnsetNetEventListener(sockInt.socketEventListener);

            // clear Socket
            sockInt = null;

            Factory.UnregisterAll();
            NetLog.RemoveAll();
            NetConsole.Clear();
            DebugInfo.Hide();
        }

        public static void StartDone()
        {
            // global callback
            GlobalEventListenerBase.AscensionStartDoneInvoke();

            // auto load scene
            if (autoloadScene != null)
            {
                AscensionNetwork.LoadScene(autoloadScene);
            }
        }

        public static void StartFailed()
        {
            // global callback
            GlobalEventListenerBase.AscensionStartFailedInvoke();
        }

        public static void Initialize(NetworkModes mode, IPEndPoint endPoint, RuntimeSettings config, string sceneToLoad)
        {
            autoloadScene = sceneToLoad;

            if (!globalControlObject)
            {
                globalControlObject = new GameObject("AscensionControl");
                globalControlObject.AddComponent<ControlBehavior>();

                DontDestroyOnLoad(globalControlObject);
            }

            globalControlObject.SendMessage("QueueStart", new ControlCommandStart { Mode = mode, Config = config, EndPoint = endPoint, MapLoadAction = mapLoadStartAction, MapLoadActionName = mapLoadStartName });
        }

        public static void Shutdown()
        {
            if (globalControlObject)
            {
                globalControlObject.SendMessage("QueueShutdown", new ControlCommandShutdown());
            }
            else
            {
                throw new AscensionException("Could not find Ascension Control object");
            }
        }

        public static void ShutdownImmediate()
        {
            if (!AscensionNetwork.IsRunning)
            {
                return;
            }

            //If we are a client, send a disconnect event immediately before continuing the shutdown process
            //Do this before resetting the mode to 'None'

            //Do not try to disconnect as the client if we are already disconnected
            if (IsClient && sockInt.IsConnected)
            {
                sockInt.Disconnect();
                //Purge both socket and normal connections if ehey exist
                //This currently does nothing
                if (connections.Count > 0)
                {
                    sockInt.PurgeConnection(connections[0].SockConn);
                    PurgeConnection(connections[0]);
                }
            }
            //If we are the server, disconnect everyone connected
            if (IsServer)
            {
                //Create disconnection info
                DisconnectionInfo dInfo = new DisconnectionInfo();
                dInfo.reason = "Sudden Server Shutdown";
                dInfo.type = 0;
                //Disconnect all connections sending the reason
                for (int i = 0; i < connections.Count; i++)
                {
                    sockInt.Disconnect(connections[i].SockConn, dInfo);
                    //Purge both socket and normal connections
                    //This currently does nothing
                    sockInt.PurgeConnection(connections[i].SockConn);
                    PurgeConnection(connections[i]);
                }
            }
            //Reset Modes
            mode = NetworkModes.None;

            // destroy all entities
            foreach (Entity entity in entities.ToArray())
            {
                try
                {
                    DestroyForce(entity);
                }
                catch
                {

                }
            }

            entitiesThawed.Clear();
            entitiesFrozen.Clear();
            connections.Clear();
            globalEventDispatcher.Clear();
            globalBehaviours.Clear();

            if (globalBehaviorObject)
            {
                GameObject.Destroy(globalBehaviorObject);
            }

            //This can probably be removed, it calls to the environment code to run its reset action
            if (AscensionNetworkInternal.EnvironmentReset != null)
                AscensionNetworkInternal.EnvironmentReset();

            // set a specific writer for this
            SocketLog.SetWriter((i, m) => UnityEngine.Debug.Log(m));

            // begin closing Socket
            sockInt.Shutdown();

            // clear Socket
            sockInt = null;

            Factory.UnregisterAll();
            NetLog.RemoveAll();
            NetConsole.Clear();
            DebugInfo.Hide();
        }

        #endregion

        #region Low Level Network API

        public static void NetworkMessageReceieved(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            //Deconstruct
            string messageName = string.Empty;
            byte[] deconstructedBytes;
            int newDataSize = dataSize - 8 - 16;
            using (var p = new Packet(data, dataSize))
            {
                //Must read to move to the actual byte array
                p.ReadByte();
                //Message Name
                messageName = p.ReadString();
                //Actual data
                deconstructedBytes = p.ReadByteArray(newDataSize);
                //Count this data if we specify
                if (sockInt.countLLAPIMessagesBandwidth)
                {
                    if (c != null && c.GetAscensionConnection() != null)
                    {
                        c.GetAscensionConnection().AddBitsPerSecondIn(p.Position);
                    }
                }
            }
            // call out to user code
            GlobalEventListenerBase.OnMessageReceievedInvoke(peer, c, newDataSize, deconstructedBytes, messageName);
            GlobalEventListenerBase.OnMessageReceievedInvoke(peer, c, newDataSize, sockInt.FromByteArray(deconstructedBytes), messageName);
            SocketEventListenerBase.OnMessageReceievedInvoke(peer, c, newDataSize, deconstructedBytes, messageName);
        }

        #endregion

        #region Handle Connections / Disconnections
        static void HandleConnectFailed(IPEndPoint endpoint)
        {
            try
            {
                GlobalEventListenerBase.ConnectFailedInvoke(endpoint);
            }
            finally
            {
                Shutdown();
                //Quick and dirty try to clear a failed connect attempt
                Connection cn = GetConnection(endpoint);
                if (cn != null)
                {
                    //Unready this pending connection
                    AttemptUnreadyPendingSocket(cn);
                    //Unready this connection
                    AttemptUnreadySocket(cn);
                    //Remove All
                    AttemptRemovalAll(cn);
                }
            }

        }

        static void HandleConnectRefused(IPEndPoint endpoint, byte[] token)
        {
            try
            {
                GlobalEventListenerBase.ConnectRefusedInvoke(endpoint);
                GlobalEventListenerBase.ConnectRefusedInvoke(endpoint, token.ToToken());
            }
            finally
            {
                if (IsServer)
                {

                }
                else
                {
                    Shutdown();
                }
                //Quick and dirty try to clear a refused connect attempt
                Connection cn = GetConnection(endpoint);
                if (cn != null)
                {
                    //Unready this pending connection
                    AttemptUnreadyPendingSocket(cn);
                    //Unready this connection
                    AttemptUnreadySocket(cn);
                    //Remove All
                    AttemptRemovalAll(cn);
                }
            }
        }

        static void HandleConnectRefusedFinal(SocketConnection conn, DisconnectInfo discInfo)
        {
            if (HasSocket)
            {
                //Remove socket connection
                sockInt.RemovePendingConnection(conn);

                //Quick and dirty try to clear a failed connect attempt
                Connection cn = GetConnection(conn);
                if (cn != null)
                {
                    //Unready this pending connection
                    AttemptUnreadyPendingSocket(cn);
                    //Unready this connection
                    AttemptUnreadySocket(cn);
                    //Remove All
                    AttemptRemovalAll(cn);
                }
            }
        }

        static void HandleConnectRequest(IPEndPoint endpoint, byte[] token)
        {
            GlobalEventListenerBase.ConnectRequestInvoke(endpoint);
            GlobalEventListenerBase.ConnectRequestInvoke(endpoint, token.ToToken());
        }

        static void HandleConnected(SocketConnection conn)
        {
            if (IsClient)
            {
                NetworkIdAllocator.Assigned(conn.ConnectionInfo.Peer.ConnectId);

                foreach (Entity eo in entities)
                {
                    // if we have instantiated something, this MUST have long.MaxValue as connection id
                    NetAssert.True(eo.NetworkId.Connection == uint.MaxValue);

                    // update with our received connection id
                    eo.NetworkId = new NetworkId(conn.ConnectionInfo.Peer.ConnectId, eo.NetworkId.Entity);
                }
            }

            Connection cn;

            cn = new Connection(conn);
            cn.AcceptToken = conn.AcceptToken.ToToken();
            cn.ConnectToken = conn.ConnectToken.ToToken();

            // put on connection list
            connections.AddLast(cn);

            // generic connected callback
            GlobalEventListenerBase.ConnectedInvoke(cn);

            // spawn entities
            if (Config.scopeMode == ScopeMode.Automatic)
            {
                foreach (Entity eo in entities)
                {
                    cn.entityChannel.CreateOnRemote(eo);
                }
            }
        }

        static void HandleDisconnected(Connection cn, byte[] token)
        {
            cn.DisconnectToken = token.ToToken();

            // generic disconnected callback
            GlobalEventListenerBase.DisconnectedInvoke(cn);

            if (HasSocket)
            {
                // cleanup                                                      
                try
                {
                    cn.DisconnectedInternal();
                }
                catch (Exception exn)
                {
                    NetLog.Error(exn);
                }
                //Unready this pending connection
                AttemptUnreadyPendingSocket(cn);
                //Unready this connection
                AttemptUnreadySocket(cn);

                // remove from socket connections before
                if (sockInt.PendingConnectionContains(cn.ConnectionId))
                    sockInt.PurgePendingConnection(cn.SockConn);

                //AttemptRemovalSocket(cn);

                // remove from connection list
                PurgeConnection(cn);

                // if this is the client, we should shutdown all of ascension when we get disconnected
                if (cn.SockConn.IsClient)
                {
                    Shutdown();
                    //Make sure we are now set to disconnected
                    sockInt.SetDisconnected();
                }
            }
        }

        public static void AcceptConnection(IPEndPoint endpoint, object userToken, IMessageRider acceptToken, IMessageRider connectToken)
        {
            if (!IsServer)
            {
                NetLog.Error("AcceptConnection can only be called on the server");
                return;
            }

            if (config.serverConnectionAcceptMode != ConnectionAcceptMode.Manual)
            {
                NetLog.Warn("AcceptConnection can only be called ConnectionAcceptMode is set to Manual");
                return;
            }
            //Accept
            if (sockInt.Accept(endpoint, acceptToken, connectToken))
            {
                //Invoke callback
                HandleConnected(SockInt.GetConnection(endpoint));

                NetLog.Info("Accepted connection {0}", endpoint);
                return;
            }
            NetLog.Error("Failed to accept connection {0}", endpoint);
        }

        public static void RefuseConnection(IPEndPoint endpoint, IMessageRider token)
        {
            if (!IsServer)
            {
                NetLog.Error("RefuseConnection can only be called on the server");
                return;
            }

            if (config.serverConnectionAcceptMode != ConnectionAcceptMode.Manual)
            {
                NetLog.Warn("RefuseConnection can only be called ConnectionAcceptMode is set to Manual");
                return;
            }
            //Refuse
            sockInt.Refuse(endpoint, token);
            //Announce connect refusal
            HandleConnectRefused(endpoint, token.ToByteArray());

            NetLog.Info("Refused connection {0}", endpoint);
        }
        #endregion

        #region Freeze
        public static void FreezeProxies()
        {
            var it = entitiesThawed.GetIterator();
            var freezeList = new List<Entity>();

            while (it.Next())
            {
                if ((it.val.AutoFreezeProxyFrames > 0) && !it.val.IsOwner && !it.val.HasControl && (it.val.LastFrameReceived + it.val.AutoFreezeProxyFrames < AscensionNetwork.Frame))
                {
                    freezeList.Add(it.val);
                }
            }

            for (int i = 0; i < freezeList.Count; ++i)
            {
                freezeList[i].Freeze(true);
            }
        }
        #endregion

        #region Polling / Updating

        public static void Update()
        {
            var it = entitiesThawed.GetIterator();

            while (it.Next())
            {
                if (it.val.IsFrozen)
                {
                    continue;
                }

                it.val.Render();
            }
        }

        public static void Poll()
        {
            if (HasSocket)
            {
                frame += 1;

                // first thing we do is to poll the network
                PollNetwork();

                // update our current network stats
                PollConnections();

                // handle loading and unloading of scenes
                PollRemoteSceneCallbacks();

                // adjust estimated frame numbers for connections
                AdjustEstimatedRemoteFrames();

                // step remote events and entities which depends on remote estimated frame numbers
                StepNonControlledRemoteEntities();

                // step entities which we in some way are controlling locally
                var iter = entitiesThawed.GetIterator();

                while (iter.Next())
                {
                    if (iter.val.IsFrozen)
                    {
                        continue;
                    }

                    if (iter.val.IsOwner || iter.val.HasPredictedControl)
                    {
                        iter.val.Simulate();
                    }
                }

                //Freeze proxies
                FreezeProxies();

                //Dispatch all events
                EventDispatcher.DispatchAllEvents();
            }
        }

        public static void Send()
        {
            if (HasSocket)
            {
                // auto scope everything
                if (config.scopeMode == ScopeMode.Automatic)
                {
                    var eo = entitiesThawed.GetIterator();

                    while (eo.Next())
                    {
                        var cn = connections.GetIterator();

                        while (cn.Next())
                        {
                            cn.val.entityChannel.CreateOnRemote(eo.val);
                        }
                    }
                }

                AscensionPhysics.SnapshotWorld();

                // switch perf counters
                if ((frame % FramesPerSecond) == 0)
                {
                    var it = connections.GetIterator();

                    while (it.Next())
                    {
                        it.val.SwitchPerfCounters();
                    }
                }

                // send data on all connections
                {
                    var it = connections.GetIterator();

                    while (it.Next())
                    {
                        int modifiedSendRate = LocalSendRate;

                        //Is send rate throttling enabled?
                        //This gets the send rate we should throttle to based upon the amount of data being transmitted
                        //If the send rate is less than the original, just use the local send rate (only throttle up, not down)
                        if (ThrottleSendRate)
                            modifiedSendRate = it.val.ThrottledSendRate > LocalSendRate ? LocalSendRate : it.val.ThrottledSendRate;
                        // if both connection and local can receive entities, use local sendrate
                        if ((frame % modifiedSendRate) == 0)
                        {
                            //NetLog.Info("Frame: {0}, ModSR: {1}, LocalSR: {2}, Mult: {3}", frame, modifiedSendRate, LocalSendRate, it.val.SendRateMultiplier);
                            //Attempt to send a packet
                            var packet = new Packet(0);
                            if (it.val.Send(out packet))
                            {
                                //success
                                it.val.PacketDelivered(packet);
                            }
                            else
                            {
                                //failure
                                it.val.PacketLost(packet);
                            }

                            //differing send rate
                            //if (modifiedSendRate != LocalSendRate)
                            //{
                            //    NetLog.Debug("Send Rate: {0} / {1}", modifiedSendRate, LocalSendRate);
                            //}
                            //Debug.Log(modifiedSendRate);
                        }
                    }
                }
            }
        }

        private static void PollNetwork()
        {
            sockInt.PollNetwork();
        }

        private static void PollConnections()
        {
            sockInt.PollConnections();
        }

        private static void PollRemoteSceneCallbacks()
        {
            if (localSceneLoading.State == SceneLoadState.STATE_LOADING_DONE)
            {
                var it = connections.GetIterator();

                while (it.Next())
                {
                    var sameScene = it.val.remoteSceneLoading.Scene == localSceneLoading.Scene;
                    var loadingDone = it.val.remoteSceneLoading.State == SceneLoadState.STATE_LOADING_DONE;

                    if (sameScene && loadingDone)
                    {
                        try
                        {
                            GlobalEventListenerBase.SceneLoadRemoteDoneInvoke(it.val);

                            if (localSceneLoading.Token != null)
                            {
                                GlobalEventListenerBase.SceneLoadRemoteDoneInvoke(it.val, localSceneLoading.Token);
                            }
                        }
                        finally
                        {
                            it.val.remoteSceneLoading.State = SceneLoadState.STATE_CALLBACK_INVOKED;
                        }
                    }
                }
            }
        }
        #endregion

        #region Ascender
        public static void OnPeerConnected(NetPeer peer)
        {
            sockInt.socketPeer = peer;

            SocketConnectionInfo infoThisFrame = sockInt.GetConnectionInfo(peer);
            if (IsServer)
            {
                SocketConnection c;
                //If we have chosen to auto accept all incoming connections then do so
                if (ascenConf.autoAcceptIncommingConnections)
                {
                    //Successful in adding a connection to the socket interface
                    if (sockInt.AddConnection(infoThisFrame, out c))
                    {
                        //Set the connection type to host
                        c.SetType(ConnectionMode.Host);
                        //Send a ready event to connection
                        sockInt.SetConnectionReady(peer);
                        //Tell this connection we have accepted it
                        c.SendToken(null, 0, NetworkMsg.Accept);
                        c.SendToken(null, 0, NetworkMsg.Connect);
                        //Invoke connected callback
                        HandleConnected(c);
                        NetLog.Info("{0}:{1} established a connection to us. Raw: {2}", c.ConnectionInfo.Address, c.ConnectionInfo.Port, c.ConnectionInfo.RawAddress);
                    }
                    //Failed
                    else
                    {
                        HandleConnectFailed(Utils.CreateIPEndPoint(infoThisFrame.Address, infoThisFrame.Port));
                        NetLog.Error("Failed connect attempt from {0} ", peer.EndPoint);
                    }
                }
                else
                {
                    //Successful in adding a connection to the socket interface
                    if (sockInt.AddPendingConnection(infoThisFrame, out c))
                    {
                        //Set the connection type to host
                        c.SetType(ConnectionMode.Host);
                        //Send a ready event to connection
                        sockInt.SetConnectionReady(peer);
                        NetLog.Info("{0}:{1} is awaiting approval for connectivity establishment. Raw: {2}", c.ConnectionInfo.Address, c.ConnectionInfo.Port, c.ConnectionInfo.RawAddress);
                    }
                    //Failed
                    else
                    {
                        HandleConnectFailed(Utils.CreateIPEndPoint(infoThisFrame.Address, infoThisFrame.Port));
                        NetLog.Error("Failed connect attempt from {0} ", peer.EndPoint);
                    }
                }
            }
            else
            {
                SocketConnection c;
                //Try to establish a connection to the host
                if (sockInt.UpdateConnection(Utils.CreateIPEndPoint(infoThisFrame.Address, infoThisFrame.Port), infoThisFrame, out c))
                {
                    //Set the connection type to client
                    c.SetType(ConnectionMode.Client);
                    //Invoke callback
                    GlobalEventListenerBase.ConnectAttemptInvoke(Utils.CreateIPEndPoint(infoThisFrame.Address, infoThisFrame.Port));
                    NetLog.Info("Successfully connected to {0} ", peer.EndPoint);
                }
                else
                {
                    HandleConnectFailed(Utils.CreateIPEndPoint(infoThisFrame.Address, infoThisFrame.Port));
                    NetLog.Error("Failed to establish a connection to {0} ", peer.EndPoint);
                }
            }
        }

        public static void OnPeerDisconnected(NetPeer peer, DisconnectInfo discInfo)
        {
            if (discInfo.Reason != DisconnectReason.DisconnectPeerCalled && discInfo.Reason != DisconnectReason.RemoteConnectionClose)
            {
                SocketLog.Info("{0}", discInfo.Reason);
            }

            SocketConnection sockConnection = SockInt.GetConnection(peer);
            //HANDLE HOST REFUSAL
            if (sockConnection == null && IsServer)
            {
                sockConnection = SockInt.GetPendingConnection(peer);
                if (sockConnection == null)
                {
                    if (peer != null)
                        SocketLog.Error("Refusal disconnect event received but there is no Pending Connection or Connection for {0}", peer.EndPoint);
                    return;
                }
                else
                {
                    HandleConnectRefusedFinal(sockConnection, discInfo);
                }
            }

            Connection conn = null;
            Connection connection = null;

            //LOCAL
            //If the interfaces connection id matches the current, this is a local disconnection from the server
            if (IsClient)
            {
                //If client, peer will be null on disconnect, so find it based upon the original stored address and port
                sockConnection = sockInt.GetConnection(sockInt.RemoteEndPoint);
                connection = GetConnection(sockInt.RemoteEndPoint);
                //If ascension connection is null
                if (sockConnection.GetAscensionConnection() == null)
                {
                    SocketLog.Info("Was unable to get a valid connection to: {0} ", sockInt.RemoteEndPoint);
                    return;
                }
                //Connection FAILSAFE
                if (connection == null)
                {
                    connection = sockConnection.GetAscensionConnection();
                }
                //requested connection cannot be established, for reason see error code
                if (connection != null)
                {
                    HandleDisconnected(connection, connection.DisconnectToken.ToByteArray());
                }
                SocketLog.Info("Disconnected from {0} ", sockInt.RemoteEndPoint);
            }
            else
            {
                //Get Connections first
                if (sockConnection != null)
                    conn = sockConnection.GetAscensionConnection();
                if (conn != null)
                    connection = GetConnection(conn);

                //one of existing connection has been disconnected, for reason see error code
                if (connection != null)
                {
                    HandleDisconnected(connection, connection.DisconnectToken.ToByteArray());
                }

                if (peer != null)
                    SocketLog.Info("{0} has disconnected from us. ID: {1}", peer.EndPoint, peer.ConnectId);
            }

            //Clean up matching peers
            if (peer == sockInt.socketPeer)
                sockInt.socketPeer = null;
        }

        public static void OnNetworkError(NetEndPoint endPoint, int error)
        {
            NetLog.Error("{0}:{1} - {2}", endPoint.Host, endPoint.Port, error);
        }

        public static void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            NetworkMsg type = NetworkMsg.Unknown;
            SocketConnection c = null;
            IMessageRider rider = null;

            int dataSize = reader.Data.Length;
            byte[] data = reader.Data;

            //Try to get a valid signature
            using (var p = new Packet(data, dataSize))
            {
                type = (NetworkMsg)p.ReadByte();
            }
            //Get the established connection or pending connection
            if (sockInt.PendingConnectionContains(peer))
                c = sockInt.GetPendingConnection(peer);
            else
                c = sockInt.GetConnection(peer);

            //TODO: REMOVE this is for sometimes connecting to fast no connection is available?
            //If connection still is null after this, it means we received a race condition in which
            //The data was sent before we completed our OnConnected event
            if (c == null)
            {
                //DUMP
                foreach (var connection in sockInt.Connections)
                {
                    Debug.Log(connection.ConnectionInfo);
                    Debug.Log(connection.ConnectionInfo.Peer);
                    Debug.Log(connection);
                }
                foreach (var connection in sockInt.PendingConnections)
                {
                    Debug.Log(connection);
                }
                //Failsafe
                OnReceiveUpdateConnection(peer, out c);
            }

            switch (type)
            {
                case NetworkMsg.Accept:
                    if (IsClient)
                        AcceptToken(peer, c, dataSize, data);
                    else
                        HandleAcceptToken(peer, c, dataSize, data);
                    break;
                case NetworkMsg.Connect:
                    if (IsClient)
                        ConnectToken(peer, c, dataSize, data);
                    else
                        HandleConnectToken(peer, c, dataSize, data);
                    break;
                case NetworkMsg.Ready:
                    HandleConnectionReady(peer, c, dataSize, data);
                    break;
                case NetworkMsg.Data:
                    if (IsClient) //LOCAL
                        HandleLocalData(peer, c, dataSize, data);
                    else //HOST
                        HandleHostData(peer, c, dataSize, data);
                    break;
                case NetworkMsg.Disconnect:
                    break;
                case NetworkMsg.Refuse:
                    if (IsClient)
                        RefuseToken(peer, c, dataSize, data);
                    else
                        HandleRefuseToken(peer, c, dataSize, data);
                    break;
                case NetworkMsg.Message:
                    NetworkMessageReceieved(peer, c, dataSize, data);
                    break;
                case NetworkMsg.Null:
                    break;
                case NetworkMsg.Unknown:
                    NetLog.Warn("Unknown data received");
                    break;
                default:
                    //LLAPI sends with fragments could be causing this to trigger, disabled for now
                    //TODO: solve this issue later on
                    //NetLog.Error("Default data received - " + type);
                    break;
            }
        }

        public static void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            Debug.Log("[SERVER] We have lost a peer " + remoteEndPoint.Host);
        }

        public static void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {

        }
        #endregion       

        #region Network Event Handlers

        public static void SetNetEventListener(EventBasedNetListener listener)
        {
            if (listener == null)
                listener = new EventBasedNetListener();

            listener.PeerConnectedEvent += OnPeerConnected;
            listener.PeerDisconnectedEvent += OnPeerDisconnected;
            listener.NetworkReceiveEvent += OnNetworkReceive;
            listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;
            listener.NetworkLatencyUpdateEvent += OnNetworkLatencyUpdate;
            listener.NetworkErrorEvent += OnNetworkError;
        }

        public static void UnsetNetEventListener(EventBasedNetListener listener)
        {
            if (listener == null) return;

            listener.PeerConnectedEvent -= OnPeerConnected;
            listener.PeerDisconnectedEvent -= OnPeerDisconnected;
            listener.NetworkReceiveEvent -= OnNetworkReceive;
            listener.NetworkReceiveUnconnectedEvent -= OnNetworkReceiveUnconnected;
            listener.NetworkLatencyUpdateEvent -= OnNetworkLatencyUpdate;
            listener.NetworkErrorEvent -= OnNetworkError;
        }

        static void HandleHostData(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            if (c == null) return;

            using (var packet = new Packet(data, dataSize))
            {
                //Old method - does not work on disconnect...
                //c.GetAscensionConnection().PacketReceived(packet);

                //Get the ascension connection - through the list as the above will only update the connection by reference of the user token
                var conn = c.GetAscensionConnection();
                //Set it
                var connection = GetConnection(conn);
                if (connection != null)
                {
                    connection.PacketReceived(packet);
                }
            }
        }

        static void HandleLocalData(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            if (c == null) return;

            using (var packet = new Packet(data, dataSize))
            {
                //Old method - does not work on disconnect...
                //c.GetAscensionConnection().PacketReceived(packet);

                //Get the ascension connection - through the list as the above will only update the connection by reference of the user token
                var conn = c.GetAscensionConnection();
                var connection = GetConnection(conn);
                if (connection != null)
                {
                    connection.PacketReceived(packet);
                }
            }
        }

        static void HandleConnectionReady(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            //Poll to receive a ready event
            if (sockInt.ReceiveConnectionReady(c.ConnectionInfo.Peer, data, dataSize))
            {
                if (c != null)
                {
                    //Send the connect token
                    sockInt.SendToken(peer, c.ConnectToken);
                }
            }
        }

        static void HandleAcceptToken(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            IMessageRider rider;
            NetworkMsg type;

            //If we received an accept token
            if (c.State == ConnectionState.Connecting)
            {
                //Set the new state of the connection to connected, rather than pending
                c.SetState(ConnectionState.Connected);
                //Handle the new connection
                HandleConnected(c);
                NetLog.Info("Received a accept token from {0}", c.ConnectionInfo.Peer.EndPoint);
            }
        }

        static void HandleConnectToken(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            IMessageRider rider;
            NetworkMsg type;

            IPEndPoint endPoint = Utils.CreateIPEndPoint(c.ConnectionInfo.Address, c.ConnectionInfo.Port);

            if (dataSize == 0)
            {
                HandleConnectFailed(endPoint);
                NetLog.Error("Connect token failed setting for {0}", c.ConnectionInfo.Peer.EndPoint);
            }
            else
            {
                //Set Token
                c.SetToken(dataSize, data);
                //Handle Connect
                if (!ascenConf.autoAcceptIncommingConnections)
                    HandleConnectRequest(endPoint, c.ConnectToken);
                NetLog.Info("Received a connect token from {0}", c.ConnectionInfo.Peer.EndPoint);
            }
        }

        static void HandleRefuseToken(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            //The server NEVER handles a refuse token
        }

        static void AcceptToken(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            //Deserialize and set the token
            c.SetToken(dataSize, data);

            sockInt.SetConnected();

            NetLog.Info("Received & set a <color=green>accept</color> token from Server of size {0}", dataSize);
        }

        static void ConnectToken(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            //Deserialize and set the token
            c.SetToken(dataSize, data);

            //If we are still in the connecting state, set to connected
            if (c.State == ConnectionState.Connecting)
            {
                //Set the new state of the connection to connected, rather than pending
                c.SetState(ConnectionState.Connected);
                //Handle the new connection
                HandleConnected(c);
            }

            NetLog.Info("Received & set a <color=green>connect</color> token from Server of size {0}", dataSize);
        }

        static void RefuseToken(NetPeer peer, SocketConnection c, int dataSize, byte[] data)
        {
            //Deserialize and set the token
            c.SetToken(dataSize, data);

            NetLog.Info("Received & set a <color=red>refuse</color> token from Server of size {0}", dataSize);
        }
        #endregion

        #region Adjusting / Stepping
        static void AdjustEstimatedRemoteFrames()
        {
            if (HasSocket)
            {
                Iterator<Connection> it = connections.GetIterator();

                while (it.Next())
                {
                    it.val.AdjustRemoteFrame();
                }
            }
        }

        static void StepNonControlledRemoteEntities()
        {
            if (HasSocket)
            {
                bool retry;

                do
                {
                    retry = false;
                    Iterator<Connection> it = connections.GetIterator();

                    while (it.Next())
                    {
                        if (it.val.StepRemoteEntities())
                        {
                            retry = true;
                        }
                    }
                } while (retry);
            }
        }
        #endregion

        #region Utils

        static Connection GetConnection(SocketConnection sConn)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].Equals(sConn.UserToken as Connection) || connections[i].SockConn == sConn)
                {
                    return connections[i];
                }
            }
            if (SockInt.DebugMsgs)
                Debug.Log(string.Format("No connection with the socket connection {0} found. Returning null...", sConn));
            return null;
        }

        static Connection GetConnection(Connection conn)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].ConnectionId == conn.ConnectionId)
                {
                    return connections[i];
                }
            }
            if (SockInt.DebugMsgs)
                Debug.Log(string.Format("No connection {0} found. Returning null...", conn));
            return null;
        }

        static Connection GetConnection(IPEndPoint endPoint)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (Equals(connections[i].RemoteEndPoint, endPoint))
                {
                    return connections[i];
                }
            }
            if (SockInt.DebugMsgs)
                Debug.Log(string.Format("No connection with the endpoint {0} found. Returning null...", endPoint));
            return null;
        }

        static Connection GetConnectionIndex(int index)
        {
            if (SockInt.DebugMsgs && index >= connections.Count)
            {
                Debug.Log(string.Format("No connection with the index {0} exists, out of range. Returning null...", index));
                return null;
            }
            return connections[index];
        }

        static bool RemoveConnection(Connection conn)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].ConnectionId == conn.ConnectionId)
                {
                    connections.Remove(connections[i]);
                    NetLog.Info(string.Format("Sucessfully Removed Connection {0}", conn));
                    return true;
                }
            }
            //NetLog.Info(string.Format("No connection {0} found. Failing to remove...", conn));
            return false;
        }

        static bool ClearConnection(Connection conn)
        {
            if (conn != null)
            {
                conn = null;
                return true;
            }
            return false;
        }

        static bool PurgeConnection(Connection conn)
        {
            return RemoveConnection(conn) && ClearConnection(conn);
        }

        static bool AttemptRemovalAll(Connection cn)
        {
            return AttemptRemovalSocket(cn) && AttemptRemovalSocketPending(cn) && AttemptRemovalConnection(cn);
        }

        static bool AttemptRemovalConnection(Connection cn)
        {
            if (connections.Contains(cn))
            {
                PurgeConnection(cn);
                return true;
            }
            return false;
        }

        static bool AttemptRemovalSocketPending(Connection cn)
        {
            // try to remove from pending if we were just accepted
            if (sockInt.PendingConnectionContains((int)cn.ConnectionId))
            {
                sockInt.PurgePendingConnection(cn.SockConn);
                return true;
            }
            return false;
        }

        static bool AttemptRemovalSocket(Connection cn)
        {
            // try to remove from pending if we were just accepted
            if (sockInt.ConnectionContains((int)cn.ConnectionId))
            {
                sockInt.PurgeConnection(cn.SockConn);
                return true;
            }
            return false;
        }

        static bool AttemptUnreadyPendingSocket(Connection cn)
        {
            var sConn = sockInt.GetPendingConnection((int)cn.ConnectionId);
            if (sConn != null)
            {
                if (sockInt.PendingConnectionContains((int)cn.ConnectionId))
                    sConn.SetConnectionUnready();
                return true;
            }
            return false;
        }

        static bool AttemptUnreadySocket(Connection cn)
        {
            var sConn = sockInt.GetConnection((int)cn.ConnectionId);
            if (sConn != null)
            {
                if (sockInt.ConnectionContains((int)cn.ConnectionId))
                    sConn.SetConnectionUnready();
                return true;
            }
            return false;
        }

        static void OnReceiveUpdateConnection(NetPeer peer, out SocketConnection c)
        {
            SocketConnectionInfo infoThisFrame = sockInt.GetConnectionInfo(peer);

            sockInt.UpdateConnection(Utils.CreateIPEndPoint(infoThisFrame.Address, infoThisFrame.Port), infoThisFrame,
                out c);

        }

        #endregion
    }
}
