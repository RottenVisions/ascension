using UnityEngine;
using System.Collections;

namespace Ascension.Networking.Sockets
{
    public enum NetworkMsg
    {
        Ready = 1,
        Accept = 2,
        Connect = 3,
        Refuse = 4,
        Disconnect = 5,
        Data = 6,
        Message = 7,
        Unknown = 11,
        Null = 99
    }
}
