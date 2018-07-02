using System;
using UnityEngine;
using System.Collections;
using System.Net;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class ControlCommandStart : ControlCommand
    {
        public RuntimeSettings Config;
        public NetworkModes Mode;

        public IPEndPoint EndPoint;

        public Action<string> MapLoadAction;
        public string MapLoadActionName;

        public override void Run()
        {
            Core.BeginStart(this);
        }

        public override void Done()
        {
            if (MapLoadAction != null)
                MapLoadAction.Invoke(MapLoadActionName);
        }
    }
}
