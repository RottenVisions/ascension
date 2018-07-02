using System;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public enum EventTypes
    {
        Global = 0,
        Entity = 1
    }

    [ProtoContract]
    public enum EntityEventSenders
    {
        Everyone = 0,
        OnlyOwner = 1,
        OnlyController = 2,
        None = 3
    }

    [ProtoContract]
    public enum EntityEventTargets
    {
        Everyone = 0,
        EveryoneExceptController = 1,
        OnlyController = 6
    }

    [Flags]
    [ProtoContract]
    public enum GlobalEventTargets
    {
        Everyone = 1 << 0,
        Others = 1 << 1,
        Server = 1 << 2,
        Client = 1 << 3,
        AllClients = 1 << 4
    }

    [ProtoContract]
    public enum GlobalEventSenders
    {
        Everyone = 0,
        OnlyServer = 1,
        OnlyClients = 2,
        None = 3
    }

    [ProtoContract]
    public enum TransformRotationModes
    {
        Quaternion = 0,
        EulerAngles = 1
    }
}