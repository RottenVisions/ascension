using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Ascender;

namespace Ascension.Networking.Sockets
{
    public class SocketInterface
    {
        #region Ascender
        public NetPeer socketPeer;
        public NetManager server;
        public NetManager client;
        public EventBasedNetListener socketEventListener;
        public string key = "password";

        public bool isReady;
        public string address;
        public int port;

        public int incomingMsgAmount;
        public int outgoingMsgAmount;

        public bool countLLAPIMessagesBandwidth;

        private bool host;
        private bool connecting;
        private bool connected;
        private IPEndPoint socketEndPoint;

        private bool currentlyPolling = true;

        public List<SocketConnection> Connections = new List<SocketConnection>();
        public List<SocketConnection> PendingConnections = new List<SocketConnection>();

        private BasePacketPool packetPool;

        private bool debugMsgs;

        /// <summary>
        /// Current packet pool for this socket
        /// </summary>
        public BasePacketPool PacketPool
        {
            get { return packetPool; }
        }

        public bool IsHost
        {
            get { return host; }
        }

        public bool IsClient
        {
            get { return !host; }
        }


        public bool IsConnected
        {
            get { return connected; }
        }

        public IPEndPoint NewRemoteEndPoint
        {
            get { return Utils.CreateIPEndPoint(address + ":" + port); }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return socketEndPoint; }
        }

        public int NetworkTimeStamp
        {
            get { return socketPeer.TimeSinceLastPacket; }
        }

        public bool DebugMsgs
        {
            get { return debugMsgs; }
        }

        /// <summary>
        /// PacketSize defines full possible packet size and includes, 
        /// packet header, message(s) header(s), message(s) payload(s). So  maximum message length cannot be > 
        /// then packet length -(packet header + message Header)... 
        /// </summary>
        public int ReducedPacketSize
        {
            get { return RuntimeSettings.Instance.packetSize - 100; }
        }

        public SocketInterface()
        {
            socketEventListener = new EventBasedNetListener();
        }

        #endregion

        #region Initialization
        public bool Initialize(IPEndPoint endPoint, ManualResetEvent finishedEvent)
        {
            host = Core.IsServer;

            Core.SetNetEventListener(socketEventListener);

            if (Core.IsServer)
            {
                server = new NetManager(socketEventListener, RuntimeSettings.Instance.maxDefaultConnections, key);
                server.Start(endPoint.Port);
            }
            else
            {
                client = new NetManager(socketEventListener, key);
                client.Start();
            }

            //Reset the transport layer if it has already been started
            if (server != null || client != null)
            {
                //Set address and port
                address = endPoint.Address.ToString();
                port = endPoint.Port;
                //If successful set out endpoint
                socketEndPoint = endPoint;
                //Set up packet pool
                packetPool = new BasePacketPool(this);
                //Socket log
                SocketLog.Info("Socket bound to {0}", endPoint);
                //Set network update time
                SetUpdateTime(Core.FramesPerSecond);
                //After we have initialized the Socket, start done
                Core.StartDone();
                //...finally free our thread to begin again
                if (finishedEvent != null)
                    finishedEvent.Set();
                return true;
            }
            //Socket initialization failed
            Core.StartFailed();
            return false;
        }
        #endregion

        #region Connections
        public bool Connect(IMessageRider token, string networkAddress = "127.0.0.1", int networkPort = 5000)
        {
            //Checks before preceeding
            if (IsHost)
            {
                SocketLog.Error("Cannot connect when we are a host!");
                return false;
            }
            if (connected)
            {
                SocketLog.Error("Cannot connect when we are already connected!");
                return false;
            }
            if (connecting)
            {
                SocketLog.Error("Cannot connect when we are already connecting!");
                return false;
            }

            client.Connect(networkAddress, networkPort);

            //Set Address and Port
            address = networkAddress;
            port = networkPort;
            //Set the values of this connection
            SetConnectionValues(networkAddress, networkPort);
            //Create a new instance of connection info
            SocketConnectionInfo info = new SocketConnectionInfo(address, port);
            //Initialize a new connection
            SocketConnection conn;
            //Add a connection
            AddConnection(info, out conn);
            //Cache our connect token to be sent once the communication lines have been established
            conn.ConnectToken = WriteToken(token, NetworkMsg.Connect);
            //Log our succeeded attempt
            SocketLog.Info("Connection Attempt to: {0} : {1}", networkAddress, networkPort);
            //Set connecting
            connecting = true;

            return true;
        }

        public bool Connect(SocketEndPoint endPoint, IMessageRider token)
        {
            //Checks before preceeding
            if (IsHost)
            {
                SocketLog.Error("Cannot connect when we are a host!");
                return false;
            }
            if (connected)
            {
                SocketLog.Error("Cannot connect when we are already connected!");
                return false;
            }
            if (connecting)
            {
                SocketLog.Error("Cannot connect when we are already connecting!");
                return false;
            }

            client.Connect(endPoint.address, endPoint.port);

            //Set Address and Port
            address = endPoint.address;
            port = endPoint.port;
            //Set the values of this connection
            SetConnectionValues(endPoint.address, endPoint.port);
            //Create a new instance of connection info
            SocketConnectionInfo info = new SocketConnectionInfo(address, port);
            //Initialize a new connection
            SocketConnection conn;
            //Add a connection
            AddConnection(info, out conn);
            //Cache our connect token to be sent once the communication lines have been established
            conn.ConnectToken = WriteToken(token, NetworkMsg.Connect);
            //Log our succeeded attempt
            SocketLog.Info("Connection Attempt to: {0} : {1}", endPoint.address, endPoint.port);
            //Set connecting
            connecting = true;

            return true;
        }

        public bool Connect(IPEndPoint endPoint, IMessageRider token)
        {
            //Checks before preceeding
            if (IsHost)
            {
                SocketLog.Error("Cannot connect when we are a host!");
                return false;
            }
            if (connected)
            {
                SocketLog.Error("Cannot connect when we are already connected!");
                return false;
            }
            if (connecting)
            {
                SocketLog.Error("Cannot connect when we are already connecting!");
                return false;
            }
            //Connect
            client.Connect(endPoint.Address.ToString(), endPoint.Port);
            //Set Address and Port
            address = endPoint.Address.ToString();
            port = endPoint.Port;
            //Set connection values
            SetConnectionValues(endPoint.Address.ToString(), endPoint.Port);
            //Create a new instance of connection info
            SocketConnectionInfo info = new SocketConnectionInfo(address, port);
            //Initialize a new connection
            SocketConnection conn;
            //Add a connection
            AddConnection(info, out conn);
            //Cache our connect token to be sent once the communication lines have been established
            conn.ConnectToken = WriteToken(token, NetworkMsg.Connect);
            //Log our succeeded attempt
            SocketLog.Info("Connection Attempt to: {0} : {1} | Succeeded", endPoint.Address.ToString(), endPoint.Port);
            //Set connecting
            connecting = true;

            return true;
        }

        public void Disconnect(SocketConnection conn, IMessageRider token)
        {
            if (AscensionNetwork.IsClient && !connected)
            {
                SocketLog.Error("Cannot disconnect if we are not connected!");
                return;
            }
            //Construct byte array of the token
            byte[] tokenBytes = WriteToken(token, NetworkMsg.Disconnect);
            //Send Token
            conn.SendToken(tokenBytes, tokenBytes.Length, NetworkMsg.Disconnect);
            //Disconnect
            if (server != null)
            {
                server.DisconnectPeer(conn.ConnectionInfo.Peer);
            }
        }

        public void Disconnect(NetPeer peer)
        {
            if (AscensionNetwork.IsClient && !connected)
            {
                SocketLog.Error("Cannot disconnect if we are not connected!");
                return;
            }
            //Disconnect
            if (server != null)
            {
                server.DisconnectPeer(peer);
            }
        }

        public void Disconnect(byte[] tokenBytes)
        {
            if (AscensionNetwork.IsClient && !connected)
            {
                SocketLog.Error("Cannot disconnect if we are not connected!");
                return;
            }
            //Send Disconnect token
            SendDisconnectToken(tokenBytes);
            //Disconnect
            if (server != null)
            {
                server.DisconnectPeer(socketPeer);
            }

            //TODO: Do something with the token here
            //tokenBytes

        }

        public void Disconnect()
        {
            if (socketPeer == null) return;

            Disconnect(socketPeer);
        }

        public void DisconnectNetworkHost()
        {
            if (server != null)
            {
                server.Stop();
            }
        }
        #endregion

        #region Accept / Refuse
        public bool Accept(IPEndPoint endPoint)
        {
            SocketConnection conn = GetPendingConnection(endPoint);
            if (conn != null)
            {
                //Remove from pending
                RemovePendingConnection(conn);
                //Add to current
                AddConnection(conn);
                SocketLog.Info("Accepted connection from {0}:{1}", endPoint.Address, endPoint.Port);
                return true;
            }
            SocketLog.Error("Failed to accept connection {0}:{1}", endPoint.Address, endPoint.Port);
            return false;
        }

        public bool Accept(IPEndPoint endPoint, IMessageRider acceptToken, IMessageRider connectToken)
        {
            SocketConnection conn = GetPendingConnection(endPoint);
            if (conn != null)
            {
                //Remove from pending
                RemovePendingConnection(conn);
                //Add to current
                AddConnection(conn);
                //Send Tokens
                SendToken(conn.ConnectionInfo.Peer, acceptToken, NetworkMsg.Accept);
                SendToken(conn.ConnectionInfo.Peer, connectToken, NetworkMsg.Connect);
                SocketLog.Info("Accepted connection from {0}:{1}", endPoint.Address, endPoint.Port);
                return true;
            }
            SocketLog.Error("Failed to accept connection {0}:{1}", endPoint.Address, endPoint.Port);
            return false;
        }

        public bool Refuse(IPEndPoint endPoint)
        {
            bool refused = false;

            SocketConnection conn = GetPendingConnection(endPoint);
            if (conn != null)
            {
                refused = RefuseConnection(conn, null);
            }
            else
            {
                SocketLog.Error("Refuse failed! No connection with the endpoint: {0} found.", endPoint);
                return false;
            }
            return refused;
        }

        public bool Refuse(IPEndPoint endPoint, IMessageRider token)
        {
            bool refused = false;

            SocketConnection conn = GetPendingConnection(endPoint);
            if (conn != null)
            {
                refused = RefuseConnection(conn, token);
            }
            else
            {
                SocketLog.Error("Refuse failed! No connection with the endpoint: {0} found.", endPoint);
                return false;
            }
            return refused;
        }

        public bool Refuse(int connId)
        {
            bool refused = false;

            SocketConnection conn = GetPendingConnection(connId);
            if (conn != null)
            {
                refused = RefuseConnection(conn, null);
            }
            else
            {
                SocketLog.Error("Refuse failed! No connection with the ID: {0} found.", connId);
                return false;
            }
            return refused;
        }

        public bool RefuseConnection(SocketConnection conn, IMessageRider token)
        {
            if (AscensionNetwork.IsClient)
            {
                SocketLog.Error("Client cannot refuse!");
                return false;
            }
            //COnstruct byte array of the token
            byte[] tokenBytes = WriteToken(token, NetworkMsg.Refuse);
            //Send Token
            conn.SendToken(tokenBytes, tokenBytes.Length, NetworkMsg.Refuse);
            //Disconnect
            server.DisconnectPeer(conn.ConnectionInfo.Peer);
            return true;
        }

        public void RefuseConnectionWithBytes(SocketConnection conn, byte[] tokenBytes)
        {
            if (AscensionNetwork.IsClient)
            {
                SocketLog.Error("Client cannot refuse!");
            }
            //Send Token
            conn.SendToken(tokenBytes, tokenBytes.Length, NetworkMsg.Refuse);
            //Disconnect
            server.DisconnectPeer(conn.ConnectionInfo.Peer);
        }
        #endregion       

        #region Sending
        public void Send(Packet packet, NetPeer peer, SendOptions qosType)
        {
            peer.Send(packet.ByteBuffer, qosType);
        }

        public void Send(string msgId, byte[] data, NetPeer peer, SendOptions qosType)
        {
            Packet p = new Packet(data.Length + 8 + 16);
            p.WriteByte((byte)NetworkMsg.Message);
            p.WriteString(msgId);
            p.WriteByteArray(data);
            p.FinishWriting();

            if (countLLAPIMessagesBandwidth)
            {
                SocketConnection sConnection = GetConnection(peer);
                if (sConnection != null && sConnection.GetAscensionConnection() != null)
                {
                    sConnection.GetAscensionConnection().AddBitsPerSecondOut(p.Position);
                }
            }

            Send(p, peer, qosType);
        }

        public void Send(string msgId, object obj, NetPeer peer, SendOptions qosType)
        {
            Send(msgId, ToByteArray(obj), peer, qosType);
        }

        public void Send(string msgId, object obj, Connection conn, SendOptions qosType)
        {
            Send(msgId, ToByteArray(obj), conn.SockConn.ConnectionInfo.Peer, qosType);
        }

        public void Send(string msgId, object obj, SocketConnection conn, SendOptions qosType)
        {
            Send(msgId, ToByteArray(obj), conn.ConnectionInfo.Peer, qosType);
        }
        #endregion

        #region Shutdown
        public void Shutdown(ManualResetEvent finishedEvent)
        {
            //Called finished for this command to free our thread
            finishedEvent.Set();
            //Shutdown
            Shutdown();
        }

        /// <summary>
        /// Attempts to unbind the socket and then shutdown the transport layer. If we fail to unbind we will still shutdown.
        /// </summary>
        /// <returns>True is unbinding is successful, false if not</returns>
        public void Shutdown()
        {
            if (server != null)
            {
                server.Stop();
            }
            if (client != null)
            {
                client.Stop();
            }
            SocketLog.Info("Unbinding socket {0} : {1}", socketEndPoint.Address, socketEndPoint.Port);
        }
        #endregion

        #region Polling

        public void PollNetwork()
        {
            if (server != null)
            {
                server.PollEvents();
            }
            if (client != null)
            {
                client.PollEvents();
            }
        }

        /// <summary>
        /// Calculates the current stats of all of our socket connections
        /// </summary>
        public void PollConnections()
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                ////Round Trip Time
                //int netRtt = NetworkTransport.GetCurrentRtt(Connections[i].ConnectionInfo.HostId,
                //    Connections[i].ConnectionInfo.ConnectionId, out error);

                ////Packet Received Rate
                //int packetReceivedRate = NetworkTransport.GetPacketReceivedRate(Connections[i].ConnectionInfo.HostId,
                //    Connections[i].ConnectionInfo.ConnectionId, out error);

                ////Packet Sent Rate
                //int packetSentRate = NetworkTransport.GetPacketSentRate(Connections[i].ConnectionInfo.HostId,
                //    Connections[i].ConnectionInfo.ConnectionId, out error);

                ////Remote Delay Time
                //int remoteDelayTime = NetworkTransport.GetRemoteDelayTimeMS(Connections[i].ConnectionInfo.HostId,
                //    Connections[i].ConnectionInfo.ConnectionId, NetworkTimeStamp, out error);

                ////Remote Packet Received Rate
                //int remotePacketReceivedRate = NetworkTransport.GetRemotePacketReceivedRate(Connections[i].ConnectionInfo.HostId,
                //    Connections[i].ConnectionInfo.ConnectionId, out error);

                //Update the current stats of the connection
                //Connections[i].UpdateStats(netRtt, packetReceivedRate, packetSentRate, remoteDelayTime, remotePacketReceivedRate);

                //Update and calculate our ping
                //Connections[i].UpdatePing((uint)NetworkTimeStamp, (uint)remoteDelayTime, RuntimeSettings.Instance.ackDelay);
                //Connections[i].UpdatePingNaive(netRtt);
            }
        }
        #endregion

        #region Connection
        /// <summary>
        /// Adds a connection to the list of connections on this socket interface
        /// </summary>
        /// <param name="newSocketConnection">If successful, the newly created connection</param>
        /// <returns>If connection is added successfully, true, if not false - also newSocketConnection will be null</returns>
        public bool AddConnection(SocketConnectionInfo connInfo, out SocketConnection newSocketConnection)
        {
            newSocketConnection = new SocketConnection(this, connInfo);
            if (!Connections.Contains(newSocketConnection))
            {
                Connections.Add(newSocketConnection);
                SocketLog.Info("Added connection {0}:{1}", newSocketConnection.ConnectionInfo.Address, newSocketConnection.ConnectionInfo.Port);
                return true;
            }
            SocketLog.Error("Failed to add connection {0}:{1} from {2}", connInfo.Peer.EndPoint.Host, connInfo.Peer.EndPoint.Port, connInfo.Peer.ConnectId);
            newSocketConnection = null;
            return false;
        }

        public bool AddConnection(SocketConnection conn)
        {
            if (!Connections.Contains(conn))
            {
                Connections.Add(conn);
                SocketLog.Info("Added connection {0}:{1} as ID: {2}", conn.ConnectionInfo.Address, conn.ConnectionInfo.Port, conn.ConnectionInfo.Peer.ConnectId);
                return true;
            }
            SocketLog.Error("Failed to add connection {0} : {1} | ID: {2}", conn.ConnectionInfo.Address, conn.ConnectionInfo.Port, conn.ConnectionInfo.Peer.ConnectId);
            return false;
        }

        /// <summary>
        /// Removes a connection from the current socket interface
        /// </summary>
        /// <param name="connection">The connection to remove</param>
        /// <returns>If successful true (exists in our list of connections), if not false</returns>
        public bool RemoveConnection(SocketConnection connection)
        {
            if (Connections.Contains(connection))
            {
                SocketLog.Info("Removed connection {0}:{1}| ID: {2}", connection.ConnectionInfo.Address, connection.ConnectionInfo.Port, connection.ConnectionInfo.Peer.ConnectId);
                Connections.Remove(connection);
                return true;
            }
            SocketLog.Error("Failed to remove connection {0} | It does not exist.", connection.ConnectionInfo.Peer.EndPoint);
            return false;
        }

        public bool AddPendingConnection(SocketConnectionInfo connInfo, out SocketConnection potentialSocketConnection)
        {
            potentialSocketConnection = new SocketConnection(this, connInfo);
            if (!PendingConnections.Contains(potentialSocketConnection))
            {
                PendingConnections.Add(potentialSocketConnection);
                SocketLog.Info("Connection {0}:{1} currently pending approval | ID: {2}", potentialSocketConnection.ConnectionInfo.Address, potentialSocketConnection.ConnectionInfo.Port, potentialSocketConnection.ConnectionInfo.Peer.ConnectId);
                return true;
            }
            SocketLog.Error("Failed to place connection {0}:{1} | ID: {2} in pending queue.", potentialSocketConnection.ConnectionInfo.Address, potentialSocketConnection.ConnectionInfo.Port, potentialSocketConnection.ConnectionInfo.Peer.ConnectId);
            potentialSocketConnection = null;
            return false;
        }

        /// <summary>
        /// Removes a connection from the current socket interface
        /// </summary>
        /// <param name="connection">The connection to remove</param>
        /// <returns>If successful true (exists in our list of connections), if not false</returns>
        public bool RemovePendingConnection(SocketConnection connection)
        {
            if (PendingConnections.Contains(connection))
            {
                SocketLog.Info("Removed Pending connection {0}:{1}| ID: {2}", connection.ConnectionInfo.Address, connection.ConnectionInfo.Port, connection.ConnectionInfo.Peer.ConnectId);
                PendingConnections.Remove(connection);
                return true;
            }
            SocketLog.Error("Failed to remove Pending connection {0} | It does not exist.", connection.ConnectionInfo.Peer.EndPoint);
            return false;
        }

        public bool UpdateConnection(IPEndPoint endpoint, SocketConnectionInfo connInfo, out SocketConnection updatedConnection)
        {
            updatedConnection = GetConnection(endpoint);
            if (updatedConnection != null)
            {
                updatedConnection.UpdateConnectionInfo(connInfo);
                SocketLog.Info("Sucessfully updated connection {0} with new connection information.", endpoint);
                return true;
            }
            SocketLog.Error("Failed to update connection {0} with new connection information. Connection not found in Socket Interface!", endpoint);
            return false;
        }

        public bool UpdateConnection(NetPeer peer, SocketConnectionInfo connInfo, out SocketConnection updatedConnection)
        {
            updatedConnection = GetConnection(peer);
            if (updatedConnection != null)
            {
                updatedConnection.UpdateConnectionInfo(connInfo);
                SocketLog.Info("Sucessfully updated connection {0} with new connection information.", peer.EndPoint);
                return true;
            }
            SocketLog.Error("Failed to update connection {0} with new connection information. Connection not found in Socket Interface!", peer.EndPoint);
            return false;
        }

        //Create new connection if none is found
        public SocketConnection CreateConnection(NetPeer peer, SocketConnectionInfo connInfo)
        {
            var newConnection = GetConnection(peer);
            if (newConnection == null)
            {
                AddConnection(connInfo, out newConnection);
            }
            return newConnection;
        }

        public bool ClearConnection(SocketConnection connection)
        {
            if (connection != null)
            {
                connection = null;
                return true;
            }
            return false;
        }

        public bool PurgeConnection(SocketConnection connection)
        {
            return RemoveConnection(connection) && ClearConnection(connection);
        }

        public bool PurgePendingConnection(SocketConnection connection)
        {
            return RemovePendingConnection(connection) && ClearConnection(connection);
        }
        #endregion

        #region Token

        public byte[] WriteToken(byte[] token, NetworkMsg type)
        {
            //Handle null tokens
            int tokenSize = 0;
            if (token != null)
                tokenSize = token.Length;
            else
                token = new byte[0];
            //The size is a byte in bits (8) + the size of the token to transmit
            Packet p = new Packet(8 + tokenSize);

            p.WriteByte((byte)type);
            p.WriteByteArray(token);
            p.FinishWriting(); // Not necessary as our defination of this packet was precise, but still good form

            return p.ByteBuffer;
        }

        public byte[] WriteToken(IMessageRider token, NetworkMsg type)
        {
            byte[] tokenArray = token.ToByteArray();
            //Handle null tokens
            int tokenSize = 0;
            if (tokenArray != null)
                tokenSize = tokenArray.Length;
            //The size is a byte in bits (8) + the size of the token to transmit
            Packet p = new Packet(8 + tokenSize);

            p.WriteByte((byte)type);
            p.WriteToken(token);
            p.FinishWriting(); // Not necessary as our defination of this packet was precise, but still good form

            return p.ByteBuffer;
        }

        public IMessageRider ReadToken(int dataSize, byte[] data, out byte type, out int size)
        {
            Packet p = new Packet(data, dataSize);
            IMessageRider token;

            type = p.ReadByte();
            token = p.ReadToken();
            //Handle null tokens
            if (token == null || dataSize == 0 || data == null)
                size = 0;
            else
                size = token.ToByteArray().Length;

            return token;
        }

        public byte[] ReadArray(int dataSize, byte[] data, out byte type, out int size)
        {
            BasePacket p = new BasePacket(data, dataSize);
            type = p.ReadByte();
            size = p.ReadInt();

            return p.ReadByteArray(size);
        }

        public byte[] WriteArray(NetworkMsg type, byte[] tokenBytes)
        {
            BasePacket p = new BasePacket(8 + 32 + tokenBytes.Length);

            p.WriteByte((byte)type);
            p.WriteInt(tokenBytes.Length);
            p.WriteByteArray(tokenBytes);

            return p.ByteBuffer;
        }

        #endregion

        #region Ready
        /// <summary>
        /// Sets the current connection to the ready state and attempts to send this state change across the wire
        /// </summary>
        /// <param name="connId">Connection to ready up</param>
        /// <returns>If the state change send was successful. If it is already ready true also</returns>
        public bool SetConnectionReady(NetPeer peer)
        {
            SocketConnection conn = GetPendingConnection(peer);
            bool sent;
            if (conn != null)
            {
                if (conn.Ready)
                {
                    //Skip this if we are already set to ready
                    SocketLog.Error("Socket connection {0} was already in the ready state! [Pending Connection]", peer.ConnectId);
                    return false;
                }
                conn.SetConnectionReady();

                SocketLog.Info("Set pending connection {0} to the ready state", peer.ConnectId);
                return true;
            }

            conn = GetConnection(peer);

            if (conn != null)
            {
                if (conn.Ready)
                {
                    //Skip this if we are already set to ready
                    SocketLog.Error("Socket connection {0} was already in the ready state! [Connection]", peer.ConnectId);
                    return false;
                }
                conn.SetConnectionReady();

                SocketLog.Info("Set connection {0} to the ready state", peer.ConnectId);
                return true;
            }
            SocketLog.Error("Failed to set connection {0} to the Ready State", peer.ConnectId);
            return false;
        }

        /// <summary>
        /// Receives a state change from the host we are connected to
        /// </summary>
        /// <param name="connId">Connection this change is concerned with</param>
        /// <param name="data">The packet data</param>
        /// <param name="size">The packet size</param>
        /// <returns>True if proper signature is received and ready state is changed, false otherwise</returns>
        public bool ReceiveConnectionReady(NetPeer peer, byte[] data, int size)
        {
            SocketConnection conn = GetConnection(peer);
            if (conn != null)
            {
                //Do not attempt to set the ready state again if we are already in the ready state
                if (conn.Ready) return false;
                //Attempt to set the connection to a ready state
                if (conn.GetConnectionReady(data, size))
                {
                    SocketLog.Info("Received ready state for connection {0}", peer.ConnectId);
                    return true;
                }
            }
            SocketLog.Error("Received ready state failure for connection {0}", peer.ConnectId);
            return false;
        }

        public bool SetConnected()
        {
            if (IsClient)
            {
                if (!connecting)
                {
                    SocketLog.Error("Failed to set connected, we are not connecting!");
                    return false;
                }
                connecting = false;
                connected = true;
                return true;
            }
            SocketLog.Error("Failed to set connected, we are not a client!");
            return false;
        }

        public bool SetDisconnected()
        {
            if (IsClient)
            {
                if (!connected)
                {
                    SocketLog.Error("Failed to set disconnected, we are not connected!");
                    return false;
                }
                connected = false;
                return true;
            }
            SocketLog.Error("Failed to set disconnected, we are not a client!");
            return false;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets connection info from the newly received connection
        /// </summary>
        public SocketConnectionInfo GetConnectionInfo(NetPeer peer)
        {
            return new SocketConnectionInfo(peer);
        }

        public SocketConnection GetConnection(IPEndPoint endPoint)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Equals(Connections[i].RemoteEndPoint, endPoint))
                    return Connections[i];
            }
            if (debugMsgs)
                Debug.Log(string.Format("No socket connection with the endpoint {0} found {1} | {2} - returning null...", endPoint, endPoint.Address, endPoint.Port));
            return null;
        }

        public SocketConnection GetConnection(long id)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].ConnectionInfo.Peer.ConnectId == id)
                    return Connections[i];
            }
            if (debugMsgs)
                Debug.Log(string.Format("No socket connection with the connection Id {0} found - returning null...", id));
            return null;
        }

        public SocketConnection GetConnection(NetPeer peer)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].ConnectionInfo.Peer == peer)
                    return Connections[i];
            }
            if (debugMsgs)
                Debug.Log(string.Format("No socket connection with the peer {0} found - returning null...", peer));
            return null;
        }

        public SocketConnection GetPendingConnection(long id)
        {
            for (int i = 0; i < PendingConnections.Count; i++)
            {
                if (PendingConnections[i].ConnectionInfo.Peer.ConnectId == id)
                    return PendingConnections[i];
            }
            if (debugMsgs)
                Debug.Log(string.Format("No pending socket connection with the connection Id {0} found - returning null...", id));
            return null;
        }

        public SocketConnection GetPendingConnection(NetPeer peer)
        {
            for (int i = 0; i < PendingConnections.Count; i++)
            {
                if (PendingConnections[i].ConnectionInfo.Peer == peer)
                    return PendingConnections[i];
            }
            if (debugMsgs)
                Debug.Log(string.Format("No pending socket connection with the connection Id {0} found - returning null...", peer.ConnectId));
            return null;
        }

        public SocketConnection GetPendingConnection(IPEndPoint endPoint)
        {
            for (int i = 0; i < PendingConnections.Count; i++)
            {
                if (Equals(PendingConnections[i].RemoteEndPoint, endPoint))
                    return PendingConnections[i];
            }
            Debug.Log(string.Format("No socket connection with the endpoint {0} found {1} | {2} - returning null...", endPoint, endPoint.Address, endPoint.Port));
            return null;
        }

        public bool PendingConnectionContains(long id)
        {
            for (int i = 0; i < PendingConnections.Count; i++)
            {
                if (PendingConnections[i].ConnectionInfo.Peer.ConnectId == id)
                    return true;
            }
            if (debugMsgs)
                Debug.Log(string.Format("No socket connection {0} found", id));
            return false;
        }

        public bool PendingConnectionContains(NetPeer peer)
        {
            for (int i = 0; i < PendingConnections.Count; i++)
            {
                if (PendingConnections[i].ConnectionInfo.Peer == peer)
                    return true;
            }
            if (debugMsgs)
                Debug.Log(string.Format("No socket connection {0} found", peer.ConnectId));
            return false;
        }

        public bool PendingConnectionContains(IPEndPoint endPoint)
        {
            for (int i = 0; i < PendingConnections.Count; i++)
            {
                if (Equals(PendingConnections[i].RemoteEndPoint, endPoint))
                    return true;
            }
            if (debugMsgs)
                Debug.Log(string.Format("No socket connection {0} found", endPoint));
            return false;
        }

        public bool ConnectionContains(long id)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].ConnectionInfo.Peer.ConnectId == id)
                    return true;
            }
            if (debugMsgs)
                Debug.Log(string.Format("No socket connection {0} found", id));
            return false;
        }

        public NetPeer GetPeer(long id)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].ConnectionInfo.Peer.ConnectId == id)
                    return Connections[i].ConnectionInfo.Peer;
            }
            if (debugMsgs)
                Debug.Log(string.Format("No Peer with the connection Id {0} found - returning null...", id));
            return null;
        }

        public NetPeer GetPeerIndex(int index)
        {
            if (index >= Connections.Count) return null;
            return Connections[index].ConnectionInfo.Peer;
        }

        void SetConnectionValues(string address, int port)
        {
            this.address = address;
            this.port = port;

            socketEndPoint = Utils.CreateIPEndPoint(address, port);
        }

        /// <summary>
        /// Set update time of the network. If is in ms is checked, this value should be a ms int, such as 15, normally without this checked it would be say 60, or 66 ticks
        /// </summary>
        /// <param name="framesPerSecond">Frames per second value</param>
        /// <param name="isInMs"></param>
        public void SetUpdateTime(int framesPerSecond, bool isInMs = false)
        {
            //Convert FPS to time, ms (1000ms in a second)
            int newValue = (int)((1 / (float)framesPerSecond) * 1000);

            if (server != null)
            {
                server.UpdateTime = isInMs ? framesPerSecond : newValue;
            }
            if (client != null)
            {
                client.UpdateTime = isInMs ? framesPerSecond : newValue;
            }
        }

        #region Sending Tokens

        void SendConnectToken(byte[] token)
        {
            SendToken(token);
        }

        void SendDisconnectToken(byte[] token)
        {
            SendToken(token);
        }

        public bool SendToken(byte[] token)
        {
            //If token is empty, don't send
            if (token == null) return false;
            //If we don't have a proper connection, don't send
            if ((server == null && client == null) || socketPeer == null) return false;

            //Send token
            socketPeer.Send(token, SendOptions.ReliableOrdered);
            SocketLog.Info("Sending token to {0} on socket {1}, channel {2} | Token Length: {3}", socketPeer.EndPoint, socketPeer.ConnectId, SendOptions.ReliableOrdered, token.Length);
            return true;
        }

        public bool SendToken(NetPeer peer, IMessageRider token, NetworkMsg tokenType)
        {
            return SendToken(peer, WriteToken(token, tokenType));
        }

        public bool SendToken(NetPeer peer, byte[] token)
        {
            //If token is empty, don't send
            if (token == null) return false;
            //If we don't have a proper connection, don't send
            if ((server == null && client == null) || socketPeer == null) return false;

            //Send token
            peer.Send(token, SendOptions.ReliableOrdered);
            SocketLog.Info("Sending token to {0} on socket {1}, channel {2} | Token Length: {3}", peer.ConnectId, socketPeer.EndPoint, socketPeer.ConnectId, SendOptions.ReliableOrdered, token.Length);
            return true;
        }

        #endregion

        public void SetSimulation(bool simLatency, bool simLoss, int simMaxLatency, int simLossChance)
        {
            if (server != null)
            {
                server.SimulateLatency = simLatency;
                server.SimulatePacketLoss = simLoss;
                server.SimulationMaxLatency = simMaxLatency;
                server.SimulationPacketLossChance = simLossChance;
                return;
            }

            if (client != null)
            {
                client.SimulateLatency = simLatency;
                client.SimulatePacketLoss = simLoss;
                client.SimulationMaxLatency = simMaxLatency;
                client.SimulationPacketLossChance = simLossChance;
            }
        }

        /// <summary>
        /// Turns on or off the regular debug logging
        /// </summary>
        /// <param name="value"></param>
        public void SetDebugMsgs(bool value)
        {
            debugMsgs = value;
        }

        private bool IsReliableQoS(SendOptions qos)
        {
            return qos == SendOptions.ReliableOrdered || qos == SendOptions.ReliableUnordered;
        }

        public byte[] ToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        public object FromByteArray(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
        #endregion
    }
}
