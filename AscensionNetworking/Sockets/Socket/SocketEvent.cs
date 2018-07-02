using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Ascension.Networking.Sockets
{
    public class SocketEvent
    {
        public NetworkEventType eventType = NetworkEventType.Nothing;
        public int hostId;
        public int connId;
        public int chanId;
        public int dataSize;
        public byte[] dataBuff = new byte[1500];
        public byte error;

        public SocketEvent()
        {
            hostId = -1;
            connId = -1;
            chanId = -1;
            dataSize = 0;
            error = 0;
        }

        public SocketEvent(int dataSize)
        {
            dataBuff = new byte[dataSize];
            this.dataSize = dataSize;
        }

        public SocketEvent(SocketEvent ev)
        {
            Copy(ev);
        }

        public void Copy(SocketEvent ev)
        {
            eventType = ev.eventType;
            hostId = ev.hostId;
            connId = ev.connId;
            chanId = ev.chanId;
            dataSize = ev.dataSize;
            dataBuff = ev.dataBuff;
            error = ev.error;
        }
    }
} 
