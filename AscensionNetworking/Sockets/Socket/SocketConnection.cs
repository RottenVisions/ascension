using UnityEngine;
using System.Collections;
using System.Net;
using System.Threading;
using Ascender;

namespace Ascension.Networking.Sockets
{
    public class SocketConnection
    {
        #region Custom

        float networkRtt = 0.1f;
        float aliasedRtt = 0.1f;

        private int actualRtt;
        private int packetReceivedRate;
        private int packetSentRate;
        private int remoteDelayTime;
        private int remotePacketReceivedRate;

        public byte[] ConnectToken;
        public byte[] AcceptToken;
        public byte[] AcceptTokenWithPrefix;
        public byte[] RefuseToken;
        public byte[] DisconnectToken;

        private IPEndPoint endPoint;
        private SocketInterface socketInterface;
        private SocketConnectionInfo connectionInfo;

        //Creation time of this connection
        private float creationTime;
        //The current connection mode
        private ConnectionMode mode;
        //The current connection state
        private ConnectionState state;
        //Awaiting a token, usually this connection is in the pending connections list
        private bool awaitingToken;
        //Communication can commence
        private bool communicationReady;
        //Accepted by the server
        private bool accepted;

        private object resetObject;

        /// <summary>
        /// A user-assignable object
        /// </summary>
        public object UserToken
        {
            get;
            set;
        }

        public float PacketReceivedRate
        {
            get { return packetReceivedRate; }
        }

        public float PacketSentRate
        {
            get { return packetSentRate; }
        }

        public float RemoteDelayTime
        {
            get { return remoteDelayTime; }
        }

        public float RemotePacketReceivedRate
        {
            get { return remotePacketReceivedRate; }
        }

        /// <summary>
        /// The round-trip time of the network layer, excluding processing delays and ack time
        /// </summary>
        public float NetworkPing
        {
            //get { return networkRtt; }
            get { return connectionInfo.Peer.Ping; }
        }

        /// <summary>
        /// The total round-trip time, including processing delays and ack
        /// </summary>
        public float NetworkPingInterval
        {
            //get { return aliasedRtt; }
            get { return connectionInfo.Peer.NetManager.PingInterval; }
        }

        public float TimeSinceLastPacket
        {
            //get { return aliasedRtt; }
            get { return connectionInfo.Peer.TimeSinceLastPacket; }
        }

        public float AliasedPing
        {
            get { return aliasedRtt; }
        }

        //public float ActualRoundTripTime
        //{
        //    get { return actualRtt; }
        //}

        public IPEndPoint RemoteEndPoint
        {
            get { return endPoint; }
        }

        public SocketInterface SockInterface
        {
            get { return socketInterface; }
        }

        public SocketConnectionInfo ConnectionInfo
        {
            get { return connectionInfo; }
        }

        public ManualResetEvent ResetEvent
        {
            get { return (ManualResetEvent)resetObject; }
            set { NetAssert.Null(resetObject); resetObject = value; }
        }

        public bool Ready
        {
            get { return communicationReady; }
        }

        public bool Accepted
        {
            get { return accepted; }
        }

        public bool IsClient
        {
            get { return mode == ConnectionMode.Client; }
        }

        public bool IsHost
        {
            get { return mode == ConnectionMode.Host; }
        }

        public ConnectionState State
        {
            get { return state; }
        }

        public float CreationTime
        {
            get { return creationTime; }
        }

        //The default ping for new connections: .1 seconds
        private float defaultNetworkPing = 0.1f;
        //The default aliased ping for new connections: .15 seconds
        private float defaultAliasedPing = 0.15f;

        #endregion

        //Default Constructor
        public SocketConnection()
        {
            networkRtt = defaultNetworkPing;
            aliasedRtt = defaultAliasedPing;

            creationTime = Time.time;

            SetMode();
        }

        public SocketConnection(SocketInterface socketInterface, IPEndPoint endPoint)
        {
            networkRtt = defaultNetworkPing;
            aliasedRtt = defaultAliasedPing;

            creationTime = Time.time;

            this.endPoint = endPoint;
            this.socketInterface = socketInterface;

            SetMode();

            //connectionInfo = new SocketConnectionInfo(endPoint.Address.ToString(), endPoint.Port);
        }

        public SocketConnection(SocketInterface socketInterface, SocketConnectionInfo connectionInfo)
        {
            networkRtt = defaultNetworkPing;
            aliasedRtt = defaultAliasedPing;

            creationTime = Time.time;

            this.connectionInfo = connectionInfo;
            this.socketInterface = socketInterface;

            SetMode();

            endPoint = new IPEndPoint(IPAddress.Parse(connectionInfo.Address), connectionInfo.Port);
        }

        /// <summary>
        /// Sends a packet across the network, this defaults to reliable sequenced or reliable fragmented
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <returns>If sending was successful or not</returns>
        public bool Send(Packet packet)
        {
            //Finish writing before we send 
            packet.FinishWriting();
            //Send
            socketInterface.Send(packet, connectionInfo.Peer, SendOptions.ReliableUnordered);
            //Force truth signature
            return true;
        }

        public void Disconnect()
        {
            SockInterface.Disconnect(connectionInfo.Peer);
        }

        public void Disconnect(IMessageRider token)
        {
            SockInterface.Disconnect(this, token);
        }


        /// <summary>
        /// This is the old way of updating a ping based on the original logic, this has been deprecated
        /// </summary>
        /// <param name="recvTime">Receive Tie</param>
        /// <param name="sendTime">Send Time</param>
        /// <param name="ackTime">Acknowledgement time / delay</param>
        public void UpdatePing(uint recvTime, uint sendTime, uint ackTime)
        {
            uint aliased = recvTime - sendTime;
            aliasedRtt = (aliasedRtt * 0.9f) + ((float)aliased / 1000f * 0.1f);

            uint network = aliased - NetMath.Clamp(ackTime, 0, aliased);
            networkRtt = (networkRtt * 0.9f) + ((float)network / 1000f * 0.1f);
        }

        /// <summary>
        /// Sets both ping and aliased ping to be the round trip time
        /// </summary>
        /// <param name="rtt">Round trip time</param>
        public void UpdatePingNaive(int rtt)
        {
            networkRtt = rtt;
            aliasedRtt = rtt;
        }

        public void UpdateStats(int actualRtt, int packetReceivedRate, int packetSentRate, int remoteDelayTime,
            int remotePacketReceivedRate)
        {
            //Debug.Log(string.Format("Rtt: {0}, PRR: {1}, PSR: {2} RDT: {3}, RPRR: {4}", actualRtt, packetReceivedRate, packetSentRate, remoteDelayTime, remotePacketReceivedRate));
            this.actualRtt = actualRtt;
            this.packetReceivedRate = packetReceivedRate;
            this.packetSentRate = packetSentRate;
            this.remoteDelayTime = remoteDelayTime;
            this.remotePacketReceivedRate = remotePacketReceivedRate;
        }

        /// <summary>
        /// Replaces the given connection info with the new
        /// </summary>
        /// <param name="info">The new info to replace the old with</param>
        public void UpdateConnectionInfo(SocketConnectionInfo info)
        {
            connectionInfo = info;
        }

        #region NetworkMsg

        public NetworkMsg GetPacketType(Packet packet)
        {
            return (NetworkMsg)packet.Type;
        }

        public byte GetPacketTypeAsByte(Packet packet)
        {
            return packet.Type;
        }
        #endregion

        #region Setters

        /// <summary>
        /// Set the type of this connection, whether it is a host or client
        /// </summary>
        /// <param name="mode">Client or host</param>
        public void SetType(ConnectionMode mode)
        {
            this.mode = mode;
        }
        /// <summary>
        /// Sets the state of this connection
        /// </summary>
        /// <param name="state">State to set to</param>
        public void SetState(ConnectionState state)
        {
            this.state = state;
        }

        /// <summary>
        /// Set the ready state for communication to true, send a packet to the connection
        /// </summary>
        /// <param name="sent">If the sending of the packet succeeds or fails</param>
        public void SetConnectionReady()
        {
            communicationReady = true;

            Packet p = new Packet();
            p.WriteByte((byte)NetworkMsg.Ready);
            p.WriteByte((byte)ConnectionState.Connecting);
            p.FinishWriting();

            SockInterface.Send(p, connectionInfo.Peer, SendOptions.ReliableOrdered);

            SocketLog.Info("Communication socket ready and open sent to {0}", connectionInfo.Peer.ConnectId);
        }
        /// <summary>
        /// If the proper signature is received, sets this client to the ready state for being able to send and receive messages to and from the host
        /// </summary>
        /// <param name="data">Byte array of the data contained in the packet</param>
        /// <param name="dataSize">Packet size</param>
        /// <returns>If communication lines are open true, if not false</returns>
        public bool GetConnectionReady(byte[] data, int dataSize)
        {
            Packet p = new Packet(data, dataSize);
            byte nMsg = p.ReadByte();
            byte cState = p.ReadByte();

            if (nMsg == (byte)NetworkMsg.Ready)
            {
                communicationReady = true;
                state = (ConnectionState)cState;
                SocketLog.Info("Communication socket ready and opened by hosting server");
                return true;
            }
            SocketLog.Error("Communication socket failed to be opened by hosting server - invalid Network Message {0} | Mode: {1}", nMsg, mode);
            return false;
        }

        public bool SetConnectionUnready()
        {
            if (communicationReady)
            {
                communicationReady = false;
                return true;
            }
            return false;
        }
        #endregion

        #region Tokens
        public void SetToken(int dataSize, byte[] data)
        {
            //Initialization
            NetworkMsg msgType = NetworkMsg.Unknown;
            byte type;
            int size;
            //Read the packet and convert it to a byte array
            IMessageRider token = SockInterface.ReadToken(dataSize, data, out type, out size);
            //Convert the type into a enum readable type
            msgType = (NetworkMsg)type;
            //Handle the token
            HandleToken(msgType, token);
            SocketLog.Info("Set token type {0} for connection {1}", msgType, ConnectionInfo.Peer.ConnectId);
        }

        void HandleToken(NetworkMsg type, IMessageRider token)
        {
            byte[] data = token.ToByteArray();

            switch (type)
            {
                case NetworkMsg.Connect:
                    ConnectToken = data;
                    break;
                case NetworkMsg.Accept:
                    AcceptToken = data;
                    break;
                case NetworkMsg.Disconnect:
                    DisconnectToken = data;
                    break;
                case NetworkMsg.Refuse:
                    RefuseToken = data;
                    break;
                default:
                    break;
            }
        }

        public bool SendToken(byte[] data, int dataSize, NetworkMsg type)
        {
            byte[] newData = socketInterface.WriteToken(data, type);

            return SockInterface.SendToken(connectionInfo.Peer, newData);
        }

        public bool ReceiveToken(byte[] data, int dataSize, out IMessageRider riderToken, out NetworkMsg type)
        {
            byte tokType;
            int size;

            riderToken = socketInterface.ReadToken(dataSize, data, out tokType, out size);

            //Null token received
            if (size == 0)
            {
                //NetLog.Info("<color=green> GOT A NULL TOKEN </color>");
            }

            type = (NetworkMsg)tokType;

            if (type != NetworkMsg.Unknown)
            {
                //Handle null tokens
                if (size == 0)
                    return true;
                if (riderToken != null)
                {
                    SocketLog.Info("{0} token successfully received from host", type);
                    return true;
                }
            }
            SocketLog.Error("Failed to unpack {0} token from host", type);
            return false;
        }

        void HandleReceivedToken(NetworkMsg type, IMessageRider riderToken)
        {
            switch (type)
            {
                case NetworkMsg.Accept:
                    HandleAccepted(riderToken);
                    break;
                case NetworkMsg.Connect:
                    break;
                case NetworkMsg.Disconnect:
                    break;
                case NetworkMsg.Ready:
                    break;
                case NetworkMsg.Refuse:
                    break;
                default:
                    break;
            }
        }

        void HandleAccepted(IMessageRider riderToken)
        {
            AcceptToken = riderToken.ToByteArray();
            accepted = true;
        }
        #endregion

        void SetMode()
        {
            if (Core.Mode == NetworkModes.Host)
            {
                mode = ConnectionMode.Host;
            }
            if (Core.Mode == NetworkModes.Client)
            {
                mode = ConnectionMode.Client;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}:{2}", ConnectionInfo.Peer.ConnectId, ConnectionInfo.Address,
                ConnectionInfo.Port);
        }
    }

    public enum ConnectionState : int
    {
        None = 0,
        Connecting = 1,
        Connected = 2,
        Disconnected = 3,
        Destroy = 4
    }

    public enum ConnectionMode : int
    {
        Client = 1,
        Host = 2
    }
}
