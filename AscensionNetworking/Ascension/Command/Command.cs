using UnityEngine;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public abstract class NetworkCommand_Data : NetworkObj, INetworkCommandData
    {
        public IMessageRider Token
        {
            get;
            set;
        }

        public Command RootCommand
        {
            get { return (Command)Root; }
        }

        IMessageRider INetworkCommandData.Token
        {
            get { return this.Token; }
            set { this.Token = value; }
        }

        public NetworkCommand_Data(NetworkObj_Meta meta)
          : base(meta)
        {
        }

    }

    public abstract class Command_Meta : NetworkObj_Meta
    {
        public int SmoothFrames;
        public bool CompressZeroValues;
    }

    /// <summary>
    /// Base class that all commands inherit from
    /// </summary>
    public abstract class Command : NetworkObj_Root, IListNode
    {
        object IListNode.Prev { get; set; }
        object IListNode.Next { get; set; }
        object IListNode.List { get; set; }

        public const int SEQ_BITS = 8;
        public const int SEQ_SHIFT = 16 - SEQ_BITS;
        public const int SEQ_MASK = (1 << SEQ_BITS) - 1;

        private NetworkStorage storage;

        public new Command_Meta Meta;

        public override NetworkStorage Storage
        {
            get { return storage; }
        }

        public NetworkCommand_Data InputObject
        {
            get { return (NetworkCommand_Data) Objects[1]; }
        }

        public NetworkCommand_Data ResultObject
        {
            get { return (NetworkCommand_Data) Objects[2]; }
        }

        public int SmoothFrameFrom;
        public int SmoothFrameTo;

        public NetworkStorage SmoothStorageFrom;
        public NetworkStorage SmoothStorageTo;

        public ushort Sequence;
        public CommandFlags Flags;

        /// <summary>
        /// The value of the AscensionNetwork.serverFrame property of the computer this command was created on
        /// </summary>
        public int ServerFrame { get; set; }

        /// <summary>
        /// Returns true if it's the first time this command executed
        /// </summary>
        public bool IsFirstExecution
        {
            get { return !(Flags & CommandFlags.HAS_EXECUTED); }
        }

        /// <summary>
        /// User assignable token that lets you pair arbitrary data with the command, this is not replicated over the network to any remote computers.
        /// </summary>
        public object UserToken { get; set; }

        public Command(Command_Meta meta)
            : base(meta)
        {
            Meta = meta;
            storage = AllocateStorage();
        }

        public void VerifyCanSetInput()
        {
            if (Flags & CommandFlags.HAS_EXECUTED)
            {
                throw new AscensionException("You can not change the Data of a command after it has executed");
            }
        }

        public void VerifyCanSetResult()
        {
            if (Flags & CommandFlags.CORRECTION_RECEIVED)
            {
                throw new AscensionException("You can not change the Data of a command after it has been corrected");
            }
        }

        public void PackInput(Connection connection, Packet packet)
        {
            for (int i = 0; i < InputObject.Meta.Properties.Length; ++i)
            {
                InputObject.Meta.Properties[i].Property.Write(connection, InputObject, Storage, packet);
            }
        }

        public void ReadInput(Connection connection, Packet packet)
        {
            for (int i = 0; i < InputObject.Meta.Properties.Length; ++i)
            {
                InputObject.Meta.Properties[i].Property.Read(connection, InputObject, Storage, packet);
            }
        }

        public void PackResult(Connection connection, Packet packet)
        {
            for (int i = 0; i < ResultObject.Meta.Properties.Length; ++i)
            {
                ResultObject.Meta.Properties[i].Property.Write(connection, ResultObject, Storage, packet);
            }
        }

        public void ReadResult(Connection connection, Packet packet)
        {
            for (int i = 0; i < ResultObject.Meta.Properties.Length; ++i)
            {
                ResultObject.Meta.Properties[i].Property.Read(connection, ResultObject, SmoothStorageTo ?? Storage,
                    packet);
            }
        }

        public void BeginSmoothing()
        {
            SmoothStorageFrom = DuplicateStorage(Storage);
            SmoothStorageTo = DuplicateStorage(Storage);

            SmoothFrameFrom = Core.Frame;
            SmoothFrameTo = SmoothFrameFrom + Meta.SmoothFrames;
        }

        public void SmoothCorrection()
        {
            if (SmoothStorageFrom != null && SmoothStorageTo != null)
            {
                float max = SmoothFrameTo - SmoothFrameFrom;
                float current = Core.Frame - SmoothFrameFrom;
                float t = Mathf.Clamp01(current / max);

                for (int i = 0; i < ResultObject.Meta.Properties.Length; ++i)
                {
                    ResultObject.Meta.Properties[i].Property.SmoothCommandCorrection(ResultObject, SmoothStorageFrom,
                        SmoothStorageTo, Storage, t);
                }
            }
        }

        public void Free()
        {
            FreeStorage(storage);
        }

        public static implicit operator bool(Command cmd)
        {
            return cmd != null;
        }
    }
}