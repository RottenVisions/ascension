using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public class ControlCommandShutdown : ControlCommand
    {
        public List<Action> Callbacks = new List<Action>();

        public override void Run()
        {
            Core.BeginShutdown(this);
        }

        public override void Done()
        {
            Core.Mode = NetworkModes.None;

            for (int i = 0; i < Callbacks.Count; ++i)
            {
                try
                {
                    Callbacks[i]();
                }
                catch (Exception exn)
                {
                    Debug.LogException(exn);
                }
            }
        }
    }
}
