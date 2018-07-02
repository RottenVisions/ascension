using System;
using System.Collections;

namespace Ascension.Networking
{
    public delegate void AddCallback(Action callback);

    public enum ControlState
    {
        Pending,
        Started,
        Failed,
        Finished
    }
}
