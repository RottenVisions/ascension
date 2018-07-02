using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ascension.Networking
{
    static class NetworkIdAllocator
    {
        static uint EntityId;
        static uint ConnectionId;

        public static uint LocalConnectionId
        {
            get { return ConnectionId; }
        }

        public static void Reset(uint connectionId)
        {
            NetLog.Debug("Id allocator reset with id {0}", connectionId);
            EntityId = 0u;
            ConnectionId = connectionId;
        }

        public static void Assigned(long connectionId)
        {
            //Converts the long connection id to a unsigned integer with data loss, as well as with overflow protection
            //This is useful if long.MaxValue is passed to the previous System.Convert.ToInt32
            uint convertedConnection = unchecked((uint)connectionId);

            NetAssert.True(Core.IsClient, "Core.IsClient");
            NetAssert.True(convertedConnection > 0U, "connectionId > 0U");
            NetAssert.True(convertedConnection != uint.MaxValue, "connectionId != uint.MaxValue");

            // verify connection id
            NetAssert.True(ConnectionId == uint.MaxValue, "ConnectionId == uint.MaxValue");

            NetLog.Debug("Assigned id {0} from server", connectionId);
            ConnectionId = convertedConnection;
        }

        public static NetworkId Allocate()
        {
            if (ConnectionId == 0)
            {
                throw new System.InvalidOperationException("Connection id not assigned");
            }

            return new NetworkId(ConnectionId, ++EntityId);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkId
    {
        public class EqualityComparer : IEqualityComparer<NetworkId>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            EqualityComparer()
            {

            }

            bool IEqualityComparer<NetworkId>.Equals(NetworkId x, NetworkId y)
            {
                return x.Packed == y.Packed;
            }

            int IEqualityComparer<NetworkId>.GetHashCode(NetworkId x)
            {
                return x.Packed.GetHashCode();
            }
        }

        public bool IsZero
        {
            get { return Packed == 0UL; }
        }

        [FieldOffset(0)]
        public ulong Packed;

        [FieldOffset(0)]
        public readonly uint Connection;

        [FieldOffset(4)]
        public readonly uint Entity;

        public ulong PackedValue
        {
            get { return Packed; }
        }

        public NetworkId(ulong packed)
        {
            Entity = 0;
            Connection = 0;

            Packed = packed;
        }

        public NetworkId(long connection, uint entity)
        {
            Packed = 0UL;
            Entity = entity;
            Connection = unchecked((uint)connection);
        }

        public override string ToString()
        {
            byte b0 = (byte)(Packed >> 56);
            byte b1 = (byte)(Packed >> 48);
            byte b2 = (byte)(Packed >> 40);
            byte b3 = (byte)(Packed >> 32);
            byte b4 = (byte)(Packed >> 24);
            byte b5 = (byte)(Packed >> 16);
            byte b6 = (byte)(Packed >> 8);
            byte b7 = (byte)(Packed >> 0);
            return string.Format("[NetworkId {0:X0}-{1:X0}-{2:X0}-{3:X0}-{4:X0}-{5:X0}-{6:X0}-{7:X0}]", b0, b1, b2, b3, b4, b5, b6, b7);
        }

        public static bool operator ==(NetworkId a, NetworkId b)
        {
            return a.Packed == b.Packed;
        }

        public static bool operator !=(NetworkId a, NetworkId b)
        {
            return a.Packed != b.Packed;
        }

        public override int GetHashCode()
        {
            return Packed.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is NetworkId)
            {
                return ((NetworkId)obj).Packed == this.Packed;
            }

            return false;
        }
    }
}

