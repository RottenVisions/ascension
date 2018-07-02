using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace Ascension.Networking.Sockets
{
    public class NetworkDiscovery : MonoBehaviour
    {
        //Broadcasting
        public int broadcastingPort = 47777;
        public int broadcastKey = 1000;
        public int broadcastVersion = 1;
        public int broadcastSubversion = 1;
        public int broadcastTimeout = 1000;
        //Semi important vars
        public int broadcastHostId = -1;
        public bool broadcasting;
        public bool broadcastingAsServer;
        //Info
        public int serverPort = 5000;
        public string message = "SERVER";
        public string extra = "";
        public bool dontDestroyClientOnLoad;
        //Private
        const int maxBroadcastMsgSize = 1024;
        [HideInInspector]
        public string broadcastData = "";
        byte[] msgOutBuffer = null;
        byte[] msgInBuffer = null;
        private HostTopology ht;

        public Dictionary<string, int> ReceivedAddresses = new Dictionary<string, int>();

        public bool debugMsgs;

        static byte[] StringToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string BytesToString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        #region Unity
        void Update()
        {
            if (broadcastHostId == -1) return;
            if (broadcastingAsServer) return;

            PollBroadCastEvent();
        }
        /// <summary>
        /// Disables broadcasting on objects disabling or destruction
        /// </summary>
        void OnDisable()
        {
            //BroadcastEnd();
        }
        #endregion

        #region Network
        void PollBroadCastEvent()
        {
            int connectionId;
            int channelId;
            int receivedSize;
            byte error;
            byte[] buffer = new byte[1024];

            NetworkEventType networkEvent = NetworkEventType.DataEvent;

            do
            {
                networkEvent = NetworkTransport.ReceiveFromHost(broadcastHostId, out connectionId, out channelId, msgInBuffer, maxBroadcastMsgSize, out receivedSize, out error);

                if (networkEvent == NetworkEventType.BroadcastEvent)
                {
                    NetworkTransport.GetBroadcastConnectionMessage(broadcastHostId, msgInBuffer, maxBroadcastMsgSize, out receivedSize, out error);

                    string senderAddr;
                    int senderPort;
                    NetworkTransport.GetBroadcastConnectionInfo(broadcastHostId, out senderAddr, out senderPort, out error);

                    OnReceivedBroadcast(senderAddr, senderPort, BytesToString(msgInBuffer));
                }
            } 
            while (networkEvent != NetworkEventType.Nothing);
        }

        void OnReceivedBroadcast(string recAddress, int recPort, string data)
        {
            //Seperate the ':ffff:' part from the address
            var addressParts = recAddress.Split(':');
            var trimmedAddress = addressParts[addressParts.Length - 1];
            
            string address = "";
            int port = 0;
            string message = "";
            string extra = "";
            //Split the data
            var items = data.Split(':');
            //Decode the data
            if (items.Length == 5 && items[0] == "Net")
            {
                address = items[1];
                port = int.Parse(items[2]);
                message = items[3];
                extra = items[4];
            }
            //Add it to our list if it does not contain
            if (!ReceivedAddresses.ContainsKey(address))
            {
                ReceivedAddresses.Add(address, port);

                if(debugMsgs)
                    Debug.Log("<color=cyan>Added</color> " + address + ":" + port + "Msg: " + message + " Extra: " + extra);
            }
            if (debugMsgs)
                Debug.Log("<color=yellow>Got broadcast from [</color>" + recAddress + ":" + recPort + "<color=yellow>]</color>! Data: " + data);
        }
        #endregion


        #region Broadcasting
        /// <summary>
        /// Initializes broadcasting as either server (sending) or client (receiving)
        /// </summary>
        /// <param name="server">Are we the server or client?</param>
        /// <returns>True if successful, false if failure</returns>
        public bool InitializeBroadcasting(bool server)
        {
            if(!NetworkTransport.IsStarted)
                NetworkTransport.Init();

            if (broadcastData.Length >= maxBroadcastMsgSize)
            {
                Debug.LogError("NetworkDiscovery Initialize - data too large. max is " + maxBroadcastMsgSize);
                return false;
            }

            broadcastData = String.Format("Net:{0}:{1}:{2}:{3}", LocalIPAddress(), serverPort, message, extra);
            msgOutBuffer = StringToBytes(broadcastData);
            msgInBuffer = new byte[maxBroadcastMsgSize];

            ConnectionConfig cc = new ConnectionConfig();
            cc.AddChannel(QosType.Unreliable);
            ht = new HostTopology(cc, 1);

            bool success;
            //SERVER
            if (server)
            {
                success = BeginBroadcastStartServer();
            }
            //CLIENT
            else
            {
                success = BeginBroadcastStartClient();
            }
            return success;
        }

        bool BeginBroadcastStartServer()
        {
            bool success;

            if (broadcastHostId != -1 || broadcasting)
            {
                Debug.LogWarning("NetworkDiscovery StartAsClient already started");
                return false;
            }

            broadcastHostId = NetworkTransport.AddHost(ht, 0);
            if (broadcastHostId == -1)
            {
                Debug.LogError("NetworkDiscovery StartAsClient - addHost failed");
                return false;
            }
            //Set vars
            broadcasting = true;
            broadcastingAsServer = true;
            //Begin broadcasting
            success = BroadcastStartServer();

            DontDestroyOnLoad(gameObject);

            if (debugMsgs)
                Debug.Log("Broadcasting Discovery as Server");

            return success;
        }

        bool BroadcastStartServer()
        {
            byte error;

            bool started;

            started = NetworkTransport.StartBroadcastDiscovery(broadcastHostId, broadcastingPort, broadcastKey, broadcastVersion,
                broadcastSubversion, msgOutBuffer, msgOutBuffer.Length, broadcastTimeout, out error);

            if (error != (byte)NetworkError.Ok)
            {
                HandleError(error);
                return false;
            }
            return started;
        }

        bool BeginBroadcastStartClient()
        {
            bool success;

            if (broadcastHostId != -1 || broadcasting)
            {
                Debug.LogWarning("NetworkDiscovery StartAsServer already started");
                return false;
            }

            broadcastHostId = NetworkTransport.AddHost(ht, broadcastingPort);
            if (broadcastHostId == -1)
            {
                Debug.LogError("NetworkDiscovery StartAsServer - addHost failed");
                return false;
            }
            //Set vars
            broadcasting = true;
            broadcastingAsServer = false;
            //Survive the load if we want to
            if (dontDestroyClientOnLoad)
                DontDestroyOnLoad(gameObject);

            //Begin listening
            success = BroadcastStartClient();

            if (debugMsgs)
                Debug.Log("Broadcast Discovery listening as Client");

            return success;
        }

        bool BroadcastStartClient()
        {
            byte error;

            NetworkTransport.SetBroadcastCredentials(broadcastHostId, broadcastKey, broadcastVersion,
                broadcastSubversion, out error);

            if (error != (byte)NetworkError.Ok)
            {
                HandleError(error);
                return false;
            }
            return true;
        }

        public void BroadcastEnd()
        {
            if (broadcastHostId == -1)
            {
                Debug.LogError("NetworkDiscovery StopBroadcast not initialized");
                return;
            }

            if (!broadcasting)
            {
                Debug.LogWarning("NetworkDiscovery StopBroadcast not started");
                return;
            }
            if (broadcastingAsServer)
            {
                NetworkTransport.StopBroadcastDiscovery();
            }

            NetworkTransport.RemoveHost(broadcastHostId);
            ReceivedAddresses.Clear();

            broadcastHostId = -1;
            broadcastingAsServer = false;
            broadcasting = false;
            msgInBuffer = null;

            if (debugMsgs)
                Debug.Log("Stopped Discovery broadcasting");
        }
        #endregion

        #region Handlers

        /// <summary>
        /// Handles error cases
        /// </summary>
        /// <param name="error">Error to handle</param>
        private void HandleError(byte error)
        {
            NetworkError errorCase = (NetworkError)error;
            switch (errorCase)
            {
                case NetworkError.BadMessage:
                    HandleErrorBadMessage();
                    break;
                case NetworkError.CRCMismatch:
                    HandleErrorCRCMismatch();
                    break;
                case NetworkError.DNSFailure:
                    HandleErrorDNSFailure();
                    break;
                case NetworkError.MessageToLong:
                    HandleErrorMessageToLong();
                    break;
                case NetworkError.NoResources:
                    HandleErrorNoResources();
                    break;
                case NetworkError.Ok:
                    HandleErrorOk();
                    break;
                case NetworkError.Timeout:
                    HandleErrorTimeout();
                    break;
                case NetworkError.VersionMismatch:
                    HandleErrorVersionMismatch();
                    break;
                case NetworkError.WrongChannel:
                    HandleErrorWrongChannel();
                    break;
                case NetworkError.WrongConnection:
                    HandleErrorWrongConnection();
                    break;
                case NetworkError.WrongHost:
                    HandleErrorWrongHost();
                    break;
                case NetworkError.WrongOperation:
                    HandleErrorWrongOperation();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Error Handlers

        void HandleErrorBadMessage()
        {
            string.Format("Received error: {0}", "BadMessage");
        }

        void HandleErrorCRCMismatch()
        {
            string.Format("Received error: {0}", "CRCMismatch");
        }

        void HandleErrorDNSFailure()
        {
            string.Format("Received error: {0}", "DNSFailure");
        }

        void HandleErrorMessageToLong()
        {
            string.Format("Received error: {0}", "MessageToLong");
        }

        void HandleErrorNoResources()
        {
            string.Format("Received error: {0}", "NoResources");
        }

        void HandleErrorOk()
        {
            string.Format("Received error: {0}", "Ok");
        }

        void HandleErrorTimeout()
        {
            string.Format("Received error: {0}", "Timeout");
        }

        void HandleErrorVersionMismatch()
        {
            string.Format("Received error: {0}", "VersionMismatch");
        }

        void HandleErrorWrongChannel()
        {
            string.Format("Received error: {0}", "WrongChannel");
        }

        void HandleErrorWrongConnection()
        {
            string.Format("Received error: {0}", "WrongConnection");
        }

        void HandleErrorWrongHost()
        {
            string.Format("Received error: {0}", "WrongHost");
        }

        void HandleErrorWrongOperation()
        {
            string.Format("Received error: {0}", "WrongOperation");
        }
        #endregion

        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
