using System;
using System.Runtime.InteropServices;

namespace Ascension.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteConverter
    {
        [FieldOffset(0)]
        public Int16 Signed16;
        [FieldOffset(0)]
        public UInt16 Unsigned16;
        [FieldOffset(0)]
        public Char Char;
        [FieldOffset(0)]
        public Int32 Signed32;
        [FieldOffset(0)]
        public UInt32 Unsigned32;
        [FieldOffset(0)]
        public Int64 Signed64;
        [FieldOffset(0)]
        public UInt64 Unsigned64;
        [FieldOffset(0)]
        public Single Float32;
        [FieldOffset(0)]
        public Double Float64;

        [FieldOffset(0)]
        public Byte Byte0;
        [FieldOffset(1)]
        public Byte Byte1;
        [FieldOffset(2)]
        public Byte Byte2;
        [FieldOffset(3)]
        public Byte Byte3;
        [FieldOffset(4)]
        public Byte Byte4;
        [FieldOffset(5)]
        public Byte Byte5;
        [FieldOffset(6)]
        public Byte Byte6;
        [FieldOffset(7)]
        public Byte Byte7;

        public static implicit operator ByteConverter(Int16 val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Signed16 = val;
            return bytes;
        }

        public static implicit operator ByteConverter(UInt16 val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Unsigned16 = val;
            return bytes;
        }

        public static implicit operator ByteConverter(Char val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Char = val;
            return bytes;
        }

        public static implicit operator ByteConverter(UInt32 val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Unsigned32 = val;
            return bytes;
        }

        public static implicit operator ByteConverter(Int32 val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Signed32 = val;
            return bytes;
        }

        public static implicit operator ByteConverter(UInt64 val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Unsigned64 = val;
            return bytes;
        }

        public static implicit operator ByteConverter(Int64 val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Signed64 = val;
            return bytes;
        }

        public static implicit operator ByteConverter(Single val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Float32 = val;
            return bytes;
        }

        public static implicit operator ByteConverter(Double val)
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Float64 = val;
            return bytes;
        }
    }
}
