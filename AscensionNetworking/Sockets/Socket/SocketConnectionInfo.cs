using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Ascender;

namespace Ascension.Networking.Sockets
{
    [Serializable]
    public class SocketConnectionInfo
    {
        private string address;
        private int port;
        private NetPeer peer;

        /// <summary>
        /// This gives the proper address
        /// </summary>
        public string Address
        {
            get { return address; }
            set { address = value; }
        }
        /// <summary>
        /// Gives the raw address from the transport layer
        /// </summary>
        public string RawAddress { get { return address; } }

        /// <summary>
        /// Port this connection exists on
        /// </summary>
        public int Port
        {
            get { return port; }
        }

        public NetPeer Peer
        {
            get { return peer; }
        }

        public SocketConnectionInfo(NetPeer peer)
        {
            address = peer.EndPoint.Host;
            port = peer.EndPoint.Port;
            this.peer = peer;
        }

        public SocketConnectionInfo(string address, int port)
        {
            this.address = address;
            this.port = port;
        }

        string RemoveBetween(string s, char begin, char end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }

        string RemoveAfter(string s, char after)
        {
            return s.Substring(0, s.LastIndexOf(after) + 1);
        }
    }
}

