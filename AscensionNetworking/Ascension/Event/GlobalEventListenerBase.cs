//#define SHOW_GLOBAL_INVOKES
using System.Net;
using Ascender;
using Ascension.Networking.Sockets;
using UnityEngine;

namespace Ascension.Networking
{
    /// <summary>
    /// Base class for all AscensionCallbacks objects
    /// </summary>
    public abstract partial class GlobalEventListenerBase : MonoBehaviour, IListNode
    {
        static readonly ListExtended<GlobalEventListenerBase> Callbacks = new ListExtended<GlobalEventListenerBase>();

        object IListNode.Prev { get; set; }
        object IListNode.Next { get; set; }
        object IListNode.List { get; set; }

        protected void OnEnable()
        {
            Core.GlobalEventDispatcher.Add(this);
            Callbacks.AddLast(this);
        }

        protected void OnDisable()
        {
            Core.GlobalEventDispatcher.Remove(this);
            Callbacks.Remove(this);
        }

        /// <summary>
        /// Override this method and return true if you want the event listener to keep being attached to Ascension even when Ascension shuts down and starts again.
        /// </summary>
        /// <returns>True/False</returns>
        public virtual bool PersistBetweenStartupAndShutdown()
        {
            return false;
        }
    }

    partial class GlobalEventListenerBase
    {
        /// <summary>
        /// Callback triggered when the Ascension simulation is shutting down.
        /// </summary>

        public virtual void AscensionShutdownBegin(AddCallback registerDoneCallback) { }

        public static void AscensionShutdownBeginInvoke(AddCallback registerDoneCallback)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback AscensionShutdownBegin");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.AscensionShutdownBegin(registerDoneCallback);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }




        public virtual void AscensionStartBegin() { }

        public static void AscensionStartBeginInvoke()
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback AscensionStartBegin");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.AscensionStartBegin();
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }




        public virtual void AscensionStartDone() { }

        public static void AscensionStartDoneInvoke()
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback AscensionStartDone");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.AscensionStartDone();
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }




        public virtual void AscensionStartFailed() { }

        public static void AscensionStartFailedInvoke()
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback AscensionStartFailed");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.AscensionStartFailed();
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when binary stream data is received 
        /// </summary>
        /// <param name="connection">The sender connection</param>
        /// <param name="data">The binary stream data</param>

        public virtual void StreamDataReceived(Connection connection, StreamData data) { }

        public static void StreamDataReceivedInvoke(Connection connection, StreamData data)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback StreamDataReceived");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.StreamDataReceived(connection, data);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback when network router port mapping has been changed
        /// </summary>
        /// <param name="device">The current network routing device</param>
        /// <param name="portMapping">The new port mapping</param>

        /*public virtual void PortMappingChanged(INatDevice device, IPortMapping portMapping) { }

        public static void PortMappingChangedInvoke(INatDevice device, IPortMapping portMapping)
        {
            NetLog.Debug("Invoking callback PortMappingChanged");
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.PortMappingChanged(device, portMapping);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }*/


        /// <summary>
        /// Callback triggered before the new local scene is loaded
        /// </summary>
        /// <param name="map">Name of scene being loaded</param>

        public virtual void SceneLoadLocalBegin(string map) { }

        public static void SceneLoadLocalBeginInvoke(string map)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback SceneLoadLocalBegin");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.SceneLoadLocalBegin(map);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }




        public virtual void SceneLoadLocalBegin(string scene, IMessageRider token) { }

        public static void SceneLoadLocalBeginInvoke(string scene, IMessageRider token)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback SceneLoadLocalBegin");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.SceneLoadLocalBegin(scene, token);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered before the new local scene has been completely loaded
        /// </summary>
        /// <param name="map">Name of scene that has loaded</param>

        public virtual void SceneLoadLocalDone(string map) { }

        public static void SceneLoadLocalDoneInvoke(string map)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback SceneLoadLocalDone");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.SceneLoadLocalDone(map);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }




        public virtual void SceneLoadLocalDone(string scene, IMessageRider token) { }

        public static void SceneLoadLocalDoneInvoke(string scene, IMessageRider token)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback SceneLoadLocalDone");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.SceneLoadLocalDone(scene, token);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when a remote connection has completely loaded the current scene
        /// </summary>
        /// <param name="connection">The remote connection</param>

        public virtual void SceneLoadRemoteDone(Connection connection) { }

        public static void SceneLoadRemoteDoneInvoke(Connection connection)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback SceneLoadRemoteDone");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.SceneLoadRemoteDone(connection);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }




        public virtual void SceneLoadRemoteDone(Connection connection, IMessageRider token) { }

        public static void SceneLoadRemoteDoneInvoke(Connection connection, IMessageRider token)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback SceneLoadRemoteDone");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.SceneLoadRemoteDone(connection, token);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when a client has become connected to this instance
        /// </summary>
        /// <param name="connection">Endpoint of the connected client</param>

        public virtual void Connected(Connection connection) { }

        public static void ConnectedInvoke(Connection connection)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback Connected");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.Connected(connection);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when a connection to remote server has failed
        /// </summary>
        /// <param name="endpoint">The remote address</param>

        public virtual void ConnectFailed(IPEndPoint endpoint) { }

        public static void ConnectFailedInvoke(IPEndPoint endpoint)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ConnectFailed");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ConnectFailed(endpoint);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when this instance receives an incoming client connection
        /// </summary>
        /// <param name="endpoint">The incoming client endpoint</param>

        public virtual void ConnectRequest(IPEndPoint endpoint) { }

        public static void ConnectRequestInvoke(IPEndPoint endpoint)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ConnectRequest");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ConnectRequest(endpoint);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when this instance receives an incoming client connection
        /// </summary>
        /// <param name="endpoint">The incoming client endpoint</param>
        /// <param name="token">A data token sent from the incoming client</param>

        public virtual void ConnectRequest(IPEndPoint endpoint, IMessageRider token) { }

        public static void ConnectRequestInvoke(IPEndPoint endpoint, IMessageRider token)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ConnectRequest");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ConnectRequest(endpoint, token);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when the connection to a remote server has been refused.
        /// </summary>
        /// <param name="endpoint">The remote server endpoint</param>

        public virtual void ConnectRefused(IPEndPoint endpoint) { }

        public static void ConnectRefusedInvoke(IPEndPoint endpoint)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ConnectRefused");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ConnectRefused(endpoint);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when the connection to a remote server has been refused.
        /// </summary>
        /// <param name="endpoint">The remote server endpoint</param>
        /// <param name="token">Data token sent by refusing server</param>

        public virtual void ConnectRefused(IPEndPoint endpoint, IMessageRider token) { }

        public static void ConnectRefusedInvoke(IPEndPoint endpoint, IMessageRider token)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ConnectRefused");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ConnectRefused(endpoint, token);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when trying to connect to a remote endpoint
        /// </summary>
        /// <param name="endpoint">The remote server address</param>
        /// <example>
        /// *Example:* Logging a message when initializing a connection to server.
        /// 
        /// ```csharp
        /// public override void ConnectAttempt((IPEndPoint endpoint) {
        ///   AscensionConsole.Write(string.Format("To Remote Server At ({0}:{1})", endpoint.Address, endpoint.Port);
        /// }
        /// ```
        /// </example>

        public virtual void ConnectAttempt(IPEndPoint endpoint) { }

        public static void ConnectAttemptInvoke(IPEndPoint endpoint)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ConnectAttempt");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ConnectAttempt(endpoint);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }


        /// <summary>
        /// Callback triggered when disconnected from remote server
        /// </summary>
        /// <param name="connection">The remote server endpoint</param>

        public virtual void Disconnected(Connection connection) { }

        public static void DisconnectedInvoke(Connection connection)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback Disconnected");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.Disconnected(connection);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        /// <summary>
        /// Callback triggered when this instance of Ascension loses control of a Ascension entity
        /// </summary>
        /// <param name="entity">The controlled entity</param>

        public virtual void ControlOfEntityLost(AscensionEntity entity) { }

        public static void ControlOfEntityLostInvoke(AscensionEntity entity)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ControlOfEntityLost");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ControlOfEntityLost(entity);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        /// <summary>
        /// Callback triggered when this instance of Ascension receieves control of a Ascension entity
        /// </summary>
        /// <param name="entity">The controlled entity</param>

        public virtual void ControlOfEntityGained(AscensionEntity entity) { }

        public static void ControlOfEntityGainedInvoke(AscensionEntity entity)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback ControlOfEntityGained");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.ControlOfEntityGained(entity);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        /// <summary>
        /// Callback triggered when a new entity is attached to the Ascension simulation
        /// </summary>
        /// <param name="entity">The attached entity</param>

        public virtual void EntityAttached(AscensionEntity entity) { }

        public static void EntityAttachedInvoke(AscensionEntity entity)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback EntityAttached");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.EntityAttached(entity);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        /// <summary>
        /// Callback triggered when a new entity is detached from the Ascension simulation
        /// </summary>
        /// <param name="entity">The detached entity</param>

        public virtual void EntityDetached(AscensionEntity entity) { }

        public static void EntityDetachedInvoke(AscensionEntity entity)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback EntityDetached");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.EntityDetached(entity);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        /// <summary>
        /// Callback triggered when a Ascension entity is recieved from the network
        /// </summary>
        /// <param name="entity">The recieved Ascension entity</param>

        public virtual void EntityReceived(AscensionEntity entity) { }

        public static void EntityReceivedInvoke(AscensionEntity entity)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback EntityReceived");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.EntityReceived(entity);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        public virtual void EntityFrozen(AscensionEntity entity) { }

        public static void EntityFrozenInvoke(AscensionEntity entity)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback EntityFrozen");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.EntityFrozen(entity);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }




        public virtual void EntityThawed(AscensionEntity entity) { }

        public static void EntityThawedInvoke(AscensionEntity entity)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback EntityThawed");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.EntityThawed(entity);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        public virtual void OnMessageReceieved(NetPeer peer, SocketConnection c, int dataSize, byte[] data, string msgName) { }

        public static void OnMessageReceievedInvoke(NetPeer peer, SocketConnection c, int dataSize, byte[] data, string msgName)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback OnMessageReceieved");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.OnMessageReceieved(peer, c, dataSize, data, msgName);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }

        public virtual void OnMessageReceieved(NetPeer peer, SocketConnection c, int dataSize, object data, string msgName) { }

        public static void OnMessageReceievedInvoke(NetPeer peer, SocketConnection c, int dataSize, object data, string msgName)
        {
#if SHOW_GLOBAL_INVOKES
            NetLog.Debug("Invoking callback OnMessageReceieved");
#endif
            foreach (GlobalEventListenerBase cb in Callbacks)
            {
                try
                {
                    cb.OnMessageReceieved(peer, c, dataSize, data, msgName);
                }
                catch (System.Exception exn)
                {
                    NetLog.Exception(exn);
                }
            }
        }
    }
}