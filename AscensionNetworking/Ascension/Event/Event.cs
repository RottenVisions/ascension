using System;
using UnityEngine;
using System.Collections;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    /// <summary>
    /// Base class that all events inherit from
    /// </summary>
    public abstract class Event : NetworkObj_Root, IDisposable
    {
        public const byte ENTITY_EVERYONE = 1;
        public const byte ENTITY_EVERYONE_EXCEPT_OWNER = 3;
        public const byte ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER = 13;
        public const byte ENTITY_EVERYONE_EXCEPT_CONTROLLER = 5;
        public const byte ENTITY_ONLY_CONTROLLER = 7;
        public const byte ENTITY_ONLY_CONTROLLER_AND_OWNER = 15;
        public const byte ENTITY_ONLY_OWNER = 9;
        public const byte ENTITY_ONLY_SELF = 11;

        public const byte GLOBAL_EVERYONE = 2;
        public const byte GLOBAL_OTHERS = 4;
        public const byte GLOBAL_ONLY_SERVER = 6;
        public const byte GLOBAL_ALL_CLIENTS = 8;
        public const byte GLOBAL_SPECIFIC_CONNECTION = 10;
        public const byte GLOBAL_ONLY_SELF = 12;

        public const int RELIABLE_WINDOW_BITS = 10;
        public const int RELIABLE_SEQUENCE_BITS = 12;

        public uint Sequence;
        public ReliabilityModes Reliability;

        public int Targets;
        public bool Reliable;
        public Entity TargetEntity;
        public Connection TargetConnection;
        public Connection SourceConnection;

        NetworkStorage storage;
        public new Event_Meta Meta;

        /// <summary>
        /// Returns true if this event was sent from own connection
        /// </summary>
        public bool FromSelf
        {
            get { return ReferenceEquals(SourceConnection, null); }
        }

        /// <summary>
        /// The connection which raised this event
        /// </summary>
        public Connection RaisedBy
        {
            get { return SourceConnection; }
        }

        /// <summary>
        /// Returns true if this is a global event / not an entity event
        /// </summary>
        public bool IsGlobalEvent
        {
            get { return !IsEntityEvent; }
        }

        public override NetworkStorage Storage
        {
            get { return storage; }
        }

        /// <summary>
        /// The raw bytes of the event data
        /// </summary>
        public byte[] BinaryData
        {
            get;
            set;
        }

        public bool IsEntityEvent
        {
            get
            {
                return
                  Targets == ENTITY_EVERYONE ||
                  Targets == ENTITY_EVERYONE_EXCEPT_OWNER ||
                  Targets == ENTITY_EVERYONE_EXCEPT_CONTROLLER ||
                  Targets == ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER ||
                  Targets == ENTITY_ONLY_CONTROLLER ||
                  Targets == ENTITY_ONLY_SELF ||
                  Targets == ENTITY_ONLY_CONTROLLER_AND_OWNER ||
                  Targets == ENTITY_ONLY_OWNER;
            }
        }

        public Event(Event_Meta meta)
  : base(meta)
        {
            Meta = meta;
            storage = AllocateStorage();
        }

        public void FreeStorage()
        {
            if (storage != null)
            {
                Meta.FreeStorage(storage);
            }
        }

        public void IncrementRefs()
        {

        }

        public void DecrementRefs()
        {

        }

        public bool Pack(Connection connection, Packet packet)
        {
            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                if (Meta.Properties[i].Property.Write(connection, this, storage, packet) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public void Read(Connection connection, Packet packet)
        {
            for (int i = 0; i < Meta.Properties.Length; ++i)
            {
                Meta.Properties[i].Property.Read(connection, this, storage, packet);
            }
        }

        public void Send()
        {
            EventDispatcher.Enqueue(this);
        }



        public void Dispose()
        {
            
        }
    }

    public abstract class Event_Meta : NetworkObj_Meta
    {

    }
}
