using UnityEngine;
using System.Collections;

namespace Ascension.Networking.Sockets
{

    public class PacketStatistic
    {
        public short msgType;
        public int count;
        public int bytes;

        public override string ToString()
        {
            return string.Format("{0} : count = {1} bytes = {2}", MessageType.MsgTypeToString(msgType), count, bytes);
        }

    }
}