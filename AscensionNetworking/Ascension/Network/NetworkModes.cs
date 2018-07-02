using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{
    /// <summary>
    /// The network mode of this network simulation (i.e. client or server)
    /// </summary>
    public enum NetworkModes
    {
        None = 0,
        Host = 1,
        Client = 2,
        Shutdown = 3,
    }
}

