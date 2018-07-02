using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ascension.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UniqueId
    {
        public class EqualityComparer : IEqualityComparer<UniqueId>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            EqualityComparer()
            {

            }

            bool IEqualityComparer<UniqueId>.Equals(UniqueId x, UniqueId y)
            {
                return x.guid == y.guid;
            }

            int IEqualityComparer<UniqueId>.GetHashCode(UniqueId x)
            {
                return x.guid.GetHashCode();
            }
        }

        [FieldOffset(0)]
        public Guid guid;

        [FieldOffset(0)]
        public uint uint0;
        [FieldOffset(4)]
        public uint uint1;
        [FieldOffset(8)]
        public uint uint2;
        [FieldOffset(12)]
        public uint uint3;

        [FieldOffset(0)]
        byte byte0;
        [FieldOffset(1)]
        byte byte1;
        [FieldOffset(2)]
        byte byte2;
        [FieldOffset(3)]
        byte byte3;

        [FieldOffset(4)]
        byte byte4;
        [FieldOffset(5)]
        byte byte5;
        [FieldOffset(6)]
        byte byte6;
        [FieldOffset(7)]
        byte byte7;

        [FieldOffset(8)]
        byte byte8;
        [FieldOffset(9)]
        byte byte9;
        [FieldOffset(10)]
        byte byte10;
        [FieldOffset(11)]
        byte byte11;

        [FieldOffset(12)]
        byte byte12;
        [FieldOffset(13)]
        byte byte13;
        [FieldOffset(14)]
        byte byte14;
        [FieldOffset(15)]
        byte byte15;

        public string IdString
        {
            get
            {
                if (IsNone)
                {
                    return "NONE";
                }

                return guid.ToString();
            }
        }

        public bool IsNone
        {
            get { return guid == Guid.Empty; }
        }

        public UniqueId(string guid)
        {
            NetAssert.NotNull(guid);
            this = default(UniqueId);
            this.guid = new Guid(guid);
        }

        public UniqueId(byte[] bytes)
        {
            NetAssert.NotNull(bytes);
            NetAssert.True(bytes.Length == 16);

            this = default(UniqueId);

            this.byte0 = bytes[0];
            this.byte1 = bytes[1];
            this.byte2 = bytes[2];
            this.byte3 = bytes[3];
            this.byte4 = bytes[4];
            this.byte5 = bytes[5];
            this.byte6 = bytes[6];
            this.byte7 = bytes[7];
            this.byte8 = bytes[8];
            this.byte9 = bytes[9];
            this.byte10 = bytes[10];
            this.byte11 = bytes[11];
            this.byte12 = bytes[12];
            this.byte13 = bytes[13];
            this.byte14 = bytes[14];
            this.byte15 = bytes[15];
        }

        public UniqueId(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7, byte byte8, byte byte9, byte byte10, byte byte11, byte byte12, byte byte13, byte byte14, byte byte15)
        {
            this = default(UniqueId);

            this.byte0 = byte0;
            this.byte1 = byte1;
            this.byte2 = byte2;
            this.byte3 = byte3;
            this.byte4 = byte4;
            this.byte5 = byte5;
            this.byte6 = byte6;
            this.byte7 = byte7;
            this.byte8 = byte8;
            this.byte9 = byte9;
            this.byte10 = byte10;
            this.byte11 = byte11;
            this.byte12 = byte12;
            this.byte13 = byte13;
            this.byte14 = byte14;
            this.byte15 = byte15;
        }

        public byte[] ToByteArray()
        {
            return guid.ToByteArray();
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is UniqueId)
            {
                return ((UniqueId)obj).guid == guid;
            }

            return false;
        }

        public override string ToString()
        {
            if (IsNone)
            {
                return "[UniqueId NONE]";
            }

            return string.Format("[UniqueId {0}]", guid.ToString());
        }

        public static UniqueId None
        {
            get { return default(UniqueId); }
        }

        public static UniqueId New()
        {
            UniqueId id;

            id = default(UniqueId);
            id.guid = Guid.NewGuid();

            return id;
        }

        public static UniqueId Parse(string text)
        {
            if (string.IsNullOrEmpty(text) || text == "NONE")
            {
                return None;
            }

            try
            {
                UniqueId id;
                id = default(UniqueId);
                id.guid = new Guid(text);
                return id;
            }
            catch
            {
                NetLog.Warn("Could not parse '{0}' as a UniqueId", text);
                return UniqueId.None;
            }
        }

        public static bool operator ==(UniqueId a, UniqueId b)
        {
            return a.guid == b.guid;
        }

        public static bool operator !=(UniqueId a, UniqueId b)
        {
            return a.guid != b.guid;
        }
    }
}