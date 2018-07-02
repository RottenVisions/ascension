using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Ascension.Networking.Physics;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public static class AscensionNetwork
    {
        public static ResourceRequest PreLoadPrefabDatabase()
        {
            return Resources.LoadAsync<PrefabDatabase>("User/" + "PrefabDatabase");
        }

        //public static Action<bool> OnPoll;
        //public static Action<bool> OnSend;

        /// <summary>
        /// A list of all AscensionEntities in the server simulation
        /// </summary>
        public static IEnumerable<AscensionEntity> SceneObjects
        {
            get { return Core.SceneObjects.Values; }
        }
        /// <summary>
        /// The current local simulation frame number
        /// </summary>
        public static int Frame
        {
            get { return Core.Frame; }
        }

        /// <summary>
        /// The max number of client connections to the server
        /// </summary>
        public static int MaxConnections
        {
            get
            {
                if (IsRunning)
                {
                    if (IsSinglePlayer)
                    {
                        return 0;
                    }

                    return IsClient ? 1 : Core.Config.maxDefaultConnections;
                }

                return -1;
            }
        }

        public static bool IsSinglePlayer
        {
            get { return Core.connections.Count < 1; }
        }

        /// <summary>
        /// Returns true if this host is a server
        /// </summary>
        public static bool IsServer
        {
            get { return Core.IsServer; }
        }

        /// <summary>
        /// Returns true if this host is a client
        /// </summary>
        public static bool IsClient
        {
            get { return Core.IsClient; }
        }

        /// <summary>
        /// Returns true if Ascension was compiled in debug mode
        /// </summary>
        public static bool IsDebugMode
        {
            get { return Core.IsDebugMode; }
        }


        /// <summary>
        /// The scoping mode active
        /// </summary>
        public static ScopeMode ScopeMode
        {
            get { return Core.Config.scopeMode; }
        }

        public static bool IsRunning
        {
            get { return IsServer || IsClient || (Core.Mode == NetworkModes.Shutdown); }
        }

        /// <summary>
        /// Returns true if this instance is a server or a client with at least one valid connection.
        /// </summary>
        public static bool IsConnected
        {
            get { return IsServer || (IsClient && Core.connections.Count > 0); }
        }

        /// <summary>
        /// The current entities contained in the simulation
        /// </summary>
        public static IEnumerable<Entity> Entities
        {
            get { return Core.entities; }
        }

        /// <summary>
        /// All the connections connected to this host
        /// </summary>
        public static IEnumerable<Connection> Connections
        {
            get { return Core.connections; }
        }

        /// <summary>
        /// The current server simulation time
        /// </summary>
        public static float ServerTime
        {
            get { return Core.ServerTime; }
        }

        /// <summary>
        /// On the server this returns the local frame, on a client this returns
        /// the currently estimated frame of all server objects we have received
        /// </summary>
        public static int ServerFrame
        {
            get { return Core.ServerFrame; }
        }

        /// <summary>
        /// The local time, same as Time.time
        /// </summary>
        public static float Time
        {
            get { return Core.CurrentTime; }
        }
        /// <summary>
        /// The fixed frame delta, same as Time.fixedDeltaTime
        /// </summary>
        public static float FrameDeltaTime
        {
            get { return Core.FrameDeltaTime; }
        }

        [Conditional("DEBUG")]
        public static void VerifyIsRunning()
        {
            if (IsRunning == false)
            {
                throw new InvalidOperationException("You can't do this if Ascension is not running!");
            }
        }

        /// <summary>
        /// The server connection
        /// </summary>
        public static Connection Server
        {
            get { return Core.Server; }
        }

        /// <summary>
        /// The global object that all global behaviours will be attached to
        /// </summary>
        public static GameObject GlobalObject
        {
            get
            {
                VerifyIsRunning();
                return Core.GlobalObject;
            }
        }

        /// <summary>
        /// How many FixedUpdate frames per second Ascension is configured to run
        /// </summary>
        public static int FramesPerSecond
        {
            get { return Core.FramesPerSecond; }
        }

        public static SocketInterface SocketInterface
        {
            get { return Core.SockInt; }
        }

        #region Methods

        /// <summary>
        /// Whether the local simulation can receive entities instantiated from other connections
        /// </summary>
        public static void SetCanReceiveEntities(bool canReceiveEntities)
        {
            VerifyIsRunning();
            Core.CanReceiveEntities = canReceiveEntities;
        }

        /// <summary>
        /// Find an entity based on unique id
        /// </summary>
        public static AscensionEntity FindEntity(NetworkId id)
        {
            VerifyIsRunning();

            if (id.Packed == 0)
            {
                return null;
            }

            foreach (var itval in Core.entities)
            {
                if (itval.IsAttached && itval.UnityObject && itval.NetworkId.Packed == id.Packed)
                {
                    return itval.UnityObject;
                }
            }

            NetLog.Warn("Could not find entity with {0}", id);
            return null;
        }

        /// <summary>
        /// Registers a type as a potential protocol token
        /// </summary>
        public static void RegisterTokenClass<T>() where T : class, IMessageRider, new()
        {
            VerifyIsRunning();
            Factory.RegisterTokenClass(typeof(T));
        }

        /// <summary>
        /// Sets a custom implementation for pooling prefabs
        /// </summary>
        public static void SetPrefabPool(IPrefabPool pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException("pool");
            }

            Core.PrefabPool = pool;
        }

        /*
         * Instantiate
         * 
         * */

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(GameObject prefab)
        {
            VerifyIsRunning();
            return Instantiate(prefab, null, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(GameObject prefab, IMessageRider token)
        {
            VerifyIsRunning();
            return Instantiate(prefab, token, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            VerifyIsRunning();
            return Instantiate(prefab, null, position, rotation);
        }

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(GameObject prefab, IMessageRider token, Vector3 position, Quaternion rotation)
        {
            VerifyIsRunning();
            AscensionEntity ae = prefab.GetComponent<AscensionEntity>();

            if (!ae)
            {
                NetLog.Error("Prefab '{0}' does not have a Ascension Entity component attached", prefab.name);
                return null;
            }

            if (ae.SerializerGuid == UniqueId.None)
            {
                NetLog.Error("Prefab '{0}' does not have a serializer assigned", prefab.name);
                return null;
            }

            return Core.Instantiate(new PrefabId(ae.prefabId), Factory.GetFactory(ae.SerializerGuid).TypeId, position, rotation, InstantiateFlags.ZERO, null, token);
        }

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(PrefabId prefabId)
        {
            VerifyIsRunning();
            return Instantiate(prefabId, null, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(PrefabId prefabId, IMessageRider token)
        {
            VerifyIsRunning();
            return Instantiate(prefabId, token, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation)
        {
            VerifyIsRunning();
            return Instantiate(prefabId, null, position, rotation);
        }

        /// <summary>
        /// Create a new entity in the simuation from a prefab
        /// </summary>
        public static AscensionEntity Instantiate(PrefabId prefabId, IMessageRider token, Vector3 position, Quaternion rotation)
        {
            VerifyIsRunning();
            return Instantiate(Core.PrefabPool.LoadPrefab(prefabId), token, position, rotation);
        }

        /*
         * Destroy
         * 
         * */

        /// <summary>
        /// Remove a gameObject from the Ascension simulation.
        /// </summary>
        public static void Destroy(GameObject gameObject)
        {
            Destroy(gameObject, null);
        }

        /// <summary>
        /// Remove a gameObject from the Ascension simulation.
        /// </summary>
        public static void Destroy(GameObject gameObject, IMessageRider token)
        {
            if (IsRunning)
            {
                AscensionEntity entity = gameObject.GetComponent<AscensionEntity>();

                if (entity)
                {
                    Core.Destroy(entity, token);
                }
                else
                {
                    if (token != null)
                    {
                        UnityEngine.Debug.LogWarning("Passing protocol token to destroy call for gameobject without ascension entity, token will be ignored");
                    }

                    UnityEngine.Object.Destroy(gameObject);
                }
            }
            else
            {
                if (token != null)
                {
                    UnityEngine.Debug.LogWarning("Passing protocol token to destroy call for gameobject when ascension is not running, token will be ignored");
                }

                UnityEngine.Object.Destroy(gameObject);
            }
        }

        /*
         * Attach
         * 
         * */

        public static GameObject Attach(GameObject gameObject)
        {
            VerifyIsRunning();
            return Attach(gameObject, null);
        }

        public static GameObject Attach(GameObject gameObject, IMessageRider token)
        {
            VerifyIsRunning();
            return Core.Attach(gameObject, EntityFlags.ZERO, token);
        }

        /*
         * Detach
         * 
         * */

        public static void Detach(GameObject gameObject)
        {
            VerifyIsRunning();
            Detach(gameObject, null);
        }

        public static void Detach(GameObject gameObject, IMessageRider token)
        {
            VerifyIsRunning();
            Core.Detach(gameObject.GetComponent<AscensionEntity>(), token);
        }
        #endregion

        /*
       * Accept
       * 
       * */

        /// <summary>
        /// Signal Ascension to accept an incoming client connection request
        /// </summary>
        /// <param name="endpoint">The UDP address of incoming client connection</param>
        public static void Accept(IPEndPoint endpoint)
        {
            VerifyIsRunning();
            Core.AcceptConnection(endpoint, null, null, null);
        }

        /// <summary>
        /// Signal Ascension to accept an incoming client connection request
        /// </summary>
        /// <param name="endpoint">The UDP address of incoming client connection</param>
        /// <param name="acceptToken">A data token from the server</param> 
        public static void Accept(IPEndPoint endpoint, IMessageRider acceptToken)
        {
            VerifyIsRunning();
            Core.AcceptConnection(endpoint, null, acceptToken, null);
        }

        public static void Accept(IPEndPoint endpoint, object userToken)
        {
            VerifyIsRunning();
            Core.AcceptConnection(endpoint, userToken, null, null);
        }

        public static void Accept(IPEndPoint endpoint, IMessageRider acceptToken, IMessageRider connectToken)
        {
            VerifyIsRunning();
            Core.AcceptConnection(endpoint, null, acceptToken, connectToken);
        }

        public static void Accept(IPEndPoint endpoint, object userToken, IMessageRider acceptToken, IMessageRider connectToken)
        {
            VerifyIsRunning();
            Core.AcceptConnection(endpoint, userToken, acceptToken, connectToken);
        }

        /*
         * Refuse
         * 
         * */

        /// <summary>
        /// Signal Ascension to refuse an incoming connection request
        /// </summary>
        /// <param name="endpoint">The UDP address of incoming client connection</param>
        public static void Refuse(IPEndPoint endpoint)
        {
            VerifyIsRunning();
            Core.RefuseConnection(endpoint, null);
        }

        /// <summary>
        /// Signal Ascension to refuse an incoming connection request
        /// </summary>
        /// <param name="endpoint">The UDP address of incoming client connection</param>
        /// <param name="acceptToken">A data token from the server</param> 
        public static void Refuse(IPEndPoint endpoint, IMessageRider token)
        {
            VerifyIsRunning();
            Core.RefuseConnection(endpoint, token);
        }

        /// <summary>
        /// Manually add a global event listener
        /// </summary>
        /// <param name="mb">The monobehaviour to invoke events on</param>
        public static void AddGlobalEventListener(MonoBehaviour mb)
        {
            VerifyIsRunning();
            Core.GlobalEventDispatcher.Add(mb);
        }

        /// <summary>
        /// Manually add a global event callback
        /// </summary>
        public static void AddGlobalEventCallback<T>(Action<T> callback) where T : Event
        {
            VerifyIsRunning();
            Core.GlobalEventDispatcher.Add<T>(callback);
        }

        /// <summary>
        /// Manually remove a global event listener
        /// </summary>
        /// <param name="mb">The monobehaviour to be removed</param>
        public static void RemoveGlobalEventListener(MonoBehaviour mb)
        {
            VerifyIsRunning();
            Core.GlobalEventDispatcher.Remove(mb);
        }

        /// <summary>
        /// Manually remove a global event callback
        /// </summary>
        public static void RemoveGlobalEventCallback<T>(Action<T> callback) where T : Event
        {
            VerifyIsRunning();
            Core.GlobalEventDispatcher.Remove<T>(callback);
        }

        /// <summary>
        /// Load a scene based on name, only possible on the Server
        /// </summary>
        /// <param name="scene">The scene to load</param>
        public static void LoadScene(string scene)
        {
            VerifyIsRunning();
            LoadScene(scene, null);
        }

        /// <summary>
        /// Load a scene based on name, only possible on the Server
        /// </summary>
        /// <param name="scene">The scene to load</param>
        /// <param name="token">A data token from the server</param>
        public static void LoadScene(string scene, IMessageRider token)
        {
            VerifyIsRunning();

            int sceneIndex = -1;

            try
            {
                sceneIndex = AscensionNetworkInternal.GetSceneIndex(scene);
            }
            catch (Exception exn)
            {
                NetLog.Error("Exception thrown while trying to find index of scene '{0}'", scene);
                NetLog.Exception(exn);
                return;
            }

            Core.LoadScene(sceneIndex, token);
        }

        /// <summary>
        /// Connect to a server
        /// </summary>
        /// <param name="endpoint">Server end point to connect to</param>
        public static void Connect(IPEndPoint endpoint)
        {
            VerifyIsRunning();
            Core.Connect(endpoint, null);
        }

        public static void Connect(string ip, int port)
        {
            VerifyIsRunning();
            Core.Connect(ip, port, null);
        }

        public static void Connect(string ip, int port, IMessageRider token)
        {
            VerifyIsRunning();
            Core.Connect(ip, port, token);
        }

        /// <summary>
        /// Connect to a server
        /// </summary>
        /// <param name="endpoint">Server end point to connect to</param>
        public static void Connect(IPEndPoint endpoint, IMessageRider token)
        {
            VerifyIsRunning();
            Core.Connect(endpoint, token);
        }

        /// <summary>
        /// Sets a scene loading action to fire once initialization and starting up a Socket has completed
        /// </summary>
        /// <param name="mapAction">Scene Loading action</param>
        /// <param name="mapName">Scene name</param>
        public static void SetMapLoadStartAction(Action<string> mapAction, string mapName)
        {
            Core.MapLoadStartAction = mapAction;
            Core.MapLoadStartName = mapName;
        }

        #region AscensionPhysics
        /// <summary>
        /// Perform a raycast against Ascension hitboxes
        /// </summary>
        /// <param name="ray"><The ray to/param>
        /// <returns>The hitboxes that intersected the ray</returns>
        /// <example>
        /// *Example:* Using RaycastAll to detect a hit event and apply damage in a player weapon firing method.
        /// 
        /// ```csharp
        /// void FireWeaponOwner(PlayerCommand cmd, AscensionEntity entity) {
        ///   if(entity.isOwner) {
        ///     using(var hits = AscensionNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos)) {
        ///       var hit = hits.GetHit(0);
        ///       var targetEntity = hit.body.GetComponent&ltAscensionEntity&gt();
        ///       
        ///       if(targetEntity.StateIs&ltILivingEntity&gt()) {
        ///         targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
        ///       }
        ///     }
        ///   }
        /// }
        /// ```
        /// </example> 
        public static AscensionPhysicsHits RaycastAll(Ray ray)
        {
            AscensionNetwork.VerifyIsRunning();
            return AscensionPhysics.Raycast(ray);
        }

        /// <summary>
        /// Perform a raycast against Ascension hitboxes
        /// </summary>
        /// <param name="ray"><The ray to/param>
        /// <param name="frame">The frame to roll back to when performing this raycast</param>
        /// <returns>The hitboxes that intersected the ray</returns>
        /// <example>
        /// *Example:* Using RaycastAll to detect a hit event on a specific previous frame and then apply damage in a player weapon firing method.
        /// 
        /// ```csharp
        /// void FireWeaponOwner(PlayerCommand cmd, AscensionEntity entity) {
        ///   if(entity.isOwner) {
        ///     using(var hits = AscensionNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos),
        ///       cmd.ServerFrame)) {
        ///       var hit = hits.GetHit(0);
        ///       var targetEntity = hit.body.GetComponent&ltAscensionEntity&gt();
        ///       
        ///       if(targetEntity.StateIs&ltILivingEntity&gt()) {
        ///         targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
        ///       }
        ///     }
        ///   }
        /// }
        /// ```
        /// </example>
        public static AscensionPhysicsHits RaycastAll(Ray ray, int frame)
        {
            AscensionNetwork.VerifyIsRunning();
            return AscensionPhysics.Raycast(ray, frame);
        }

        /// <summary>
        /// Perform a sphere overlap against Ascension hiboxes
        /// </summary>
        /// <param name="origin">The origin of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <returns>The hitboxes that overlapped with the sphere</returns>
        /// <example>
        /// *Example:* Calculating the blast radius of a grenade.
        /// 
        /// ```csharp
        /// void GrenadeOwner(PlayerCommand cmd, AscensionEntity entity, IThrownWeapon grenade) {
        ///   if(entity.isOwner) {
        ///     using(var hits = AscensionNetwork.OverlapSphereAll(cmd.targetPos, grenade.explosionRadius)) {
        ///       for(int i = 0; i < hits.count; i++) {
        ///         var hit = hits.GetHit(i);
        ///         var targetEntity = hit.body.GetComponent&ltAscensionEntity&gt();
        ///         
        ///         if(targetEntity != entity && targetEntity.StateIs&ltILivingEntity&gt()) {
        ///             targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= grenade.damage;   
        ///         } 
        ///       }
        ///     }
        ///   }
        /// }
        /// ```
        /// </example>
        public static AscensionPhysicsHits OverlapSphereAll(Vector3 origin, float radius)
        {
            AscensionNetwork.VerifyIsRunning();
            return AscensionPhysics.OverlapSphere(origin, radius);
        }

        /// <summary>
        /// Perform a sphere overlap against Ascension hiboxes
        /// </summary>
        /// <param name="origin">The origin of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="frame">The frame to rollback to for calculation</param>
        /// <returns>The hitboxes that overlapped with the sphere</returns>
        /// <example>
        /// *Example:* Calculating the blast radius of a grenade.
        /// 
        /// ```csharp
        /// void GrenadeOwner(PlayerCommand cmd, AscensionEntity entity, IThrownWeapon grenade) {
        ///   if(entity.isOwner) {
        ///     using(var hits = AscensionNetwork.OverlapSphereAll(cmd.targetPos, grenade.explosionRadius, cmd.ServerFrame)) {
        ///       for(int i = 0; i < hits.count; i++) {
        ///         var hit = hits.GetHit(i);
        ///         var targetEntity = hit.body.GetComponent&ltAscensionEntity&gt();
        ///         
        ///         if(targetEntity != entity && targetEntity.StateIs&ltILivingEntity&gt()) {
        ///             targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= grenade.damage;   
        ///         } 
        ///       }
        ///     }
        ///   }
        /// }
        /// ```
        /// </example>
        public static AscensionPhysicsHits OverlapSphereAll(Vector3 origin, float radius, int frame)
        {
            AscensionNetwork.VerifyIsRunning();
            return AscensionPhysics.OverlapSphere(origin, radius, frame);
        }
        #endregion

        #region UNET LLAPI

        public static void Send(string msgId, object obj, int connIdIndex, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, obj, Core.SockInt.GetPeerIndex(connIdIndex), qosType);
        }

        public static void Send(string msgId, byte[] bytes, int connIdIndex, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, bytes, Core.SockInt.GetPeerIndex(connIdIndex), qosType);
        }

        public static void Send(string msgId, object obj, long peerId, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, obj, Core.SockInt.GetPeer(peerId), qosType);
        }

        public static void Send(string msgId, byte[] bytes, long peerId, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, bytes, Core.SockInt.GetPeer(peerId), qosType);
        }

        public static void Send(string msgId, byte[] bytes, Connection conn, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, bytes, conn.SockConn.ConnectionInfo.Peer, qosType);
        }

        public static void Send(string msgId, object obj, Connection conn, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, obj, conn.SockConn.ConnectionInfo.Peer, qosType);
        }

        public static void Send(string msgId, byte[] bytes, SocketConnection conn, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, bytes, conn.ConnectionInfo.Peer, qosType);
        }

        public static void Send(string msgId, object obj, SocketConnection conn, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, obj, conn.ConnectionInfo.Peer, qosType);
        }

        public static void Send(string msgId, byte[] bytes, Ascender.NetPeer peer, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, bytes, peer, qosType);
        }

        public static void Send(string msgId, object obj, Ascender.NetPeer peer, Ascender.SendOptions qosType)
        {
            VerifyIsRunning();
            Core.SockInt.Send(msgId, obj, peer, qosType);
        }

        #endregion
    }
}
