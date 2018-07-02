using System;
using System.Net;
using System.Text;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public class BasePacket : IDisposable
    {
        public bool isPooled = true;
        public BasePacketPool pool { get; set; }

        protected int position;
        protected int length;
        protected byte[] data;

        private int realLength;
        private bool write;

        /// <summary>
        /// A user-assignable object
        /// </summary>
        public object UserToken
        {
            get;
            set;
        }

        public int Size
        {
            get { return length; }
            set { length = NetMath.Clamp(value, 0, data.Length << 3); }
        }

        public int ActualSize
        {
            get { return length >> 3; }
        }

        public int Position
        {
            get { return position; }
            set { position = NetMath.Clamp(value, 0, length); }
        }

        public byte[] ByteBuffer
        {
            get { return data; }
            set { data = value; }
        }

        public bool Done
        {
            get { return position == ActualSize; }
        }

        public bool Overflowing
        {
            //get { return position > ActualSize; }
            //Added 1 bit grace for send rate throttling
            get { return position > ActualSize + 1; }
        }

        public bool Write
        {
            get { return write; }
        }

        public BasePacket()
        {
            position = 0;
            data = new byte[1500]; //Give an normal number for a packet size
            length = data.Length;
        }

        public BasePacket(int size)
        {
            position = 0;
            data = new byte[size];
            length = data.Length;
        }

        public BasePacket(byte[] arr)
      : this(arr, arr.Length) {
        }

        public BasePacket(byte[] arr, int size)
        {
            position = 0;
            data = arr;
            length = size << 3;
        }

        public void FinishWriting()
        {
            Array.Resize(ref data, position);
            length = data.Length;
        }

        public bool CanWrite()
        {
            return CanWrite(1);
        }

        public bool CanRead()
        {
            return CanRead(1);
        }

        public bool CanWrite(int bits)
        {
            return position + bits <= length;
        }

        public bool CanRead(int bits)
        {
            return position + bits <= length;
        }

        public byte[] DuplicateData()
        {
            byte[] duplicate = new byte[NetMath.BytesRequired(position)];
            Array.Copy(data, 0, duplicate, 0, duplicate.Length);
            return duplicate;
        }

        #region Serialization
        public bool WriteBool(bool value)
        {
            InternalWriteByte(value ? (byte)1 : (byte)0, 1);
            return value;
        }

        public bool ReadBool()
        {
            return InternalReadByte(1) == 1;
        }

        public void WriteByte(byte value, int bits)
        {
            InternalWriteByte(value, bits);
        }

        public byte ReadByte(int bits)
        {
            return InternalReadByte(bits);
        }

        public void WriteByte(byte value)
        {
            WriteByte(value, 8);
        }

        public byte ReadByte()
        {
            return ReadByte(8);
        }

        public void WriteUShort(ushort value, int bits)
        {
            if (bits <= 8)
            {
                InternalWriteByte((byte)(value & 0xFF), bits);
            }
            else
            {
                InternalWriteByte((byte)(value & 0xFF), 8);
                InternalWriteByte((byte)(value >> 8), bits - 8);
            }
        }

        public ushort ReadUShort(int bits)
        {
            if (bits <= 8)
            {
                return InternalReadByte(bits);
            }
            else
            {
                return (ushort)(InternalReadByte(8) | (InternalReadByte(bits - 8) << 8));
            }
        }

        public void WriteUShort(ushort value)
        {
            WriteUShort(value, 16);
        }

        public ushort ReadUShort()
        {
            return ReadUShort(16);
        }

        public void WriteShort(short value, int bits)
        {
            WriteUShort((ushort)value, bits);
        }

        public short ReadShort(int bits)
        {
            return (short)ReadUShort(bits);
        }

        public void WriteShort(short value)
        {
            WriteShort(value, 16);
        }

        public short ReadShort()
        {
            return ReadShort(16);
        }

        public void Serialize(ref uint value, int bits)
        {
            if (Write) { WriteUInt(value, bits); } else { value = ReadUInt(bits); }
        }

        public void Serialize(ref int value, int bits)
        {
            if (Write) { WriteInt(value, bits); } else { value = ReadInt(bits); }
        }

        public void WriteUInt(uint value, int bits)
        {
            byte
                      a = (byte)(value >> 0),
                      b = (byte)(value >> 8),
                      c = (byte)(value >> 16),
                      d = (byte)(value >> 24);

            switch ((bits + 7) / 8)
            {
                case 1:
                    InternalWriteByte(a, bits);
                    break;

                case 2:
                    InternalWriteByte(a, 8);
                    InternalWriteByte(b, bits - 8);
                    break;

                case 3:
                    InternalWriteByte(a, 8);
                    InternalWriteByte(b, 8);
                    InternalWriteByte(c, bits - 16);
                    break;

                case 4:
                    InternalWriteByte(a, 8);
                    InternalWriteByte(b, 8);
                    InternalWriteByte(c, 8);
                    InternalWriteByte(d, bits - 24);
                    break;
            }
        }

        public uint ReadUInt(int bits)
        {
            int
                      a = 0,
                      b = 0,
                      c = 0,
                      d = 0;

            switch ((bits + 7) / 8)
            {
                case 1:
                    a = InternalReadByte(bits);
                    break;

                case 2:
                    a = InternalReadByte(8);
                    b = InternalReadByte(bits - 8);
                    break;

                case 3:
                    a = InternalReadByte(8);
                    b = InternalReadByte(8);
                    c = InternalReadByte(bits - 16);
                    break;

                case 4:
                    a = InternalReadByte(8);
                    b = InternalReadByte(8);
                    c = InternalReadByte(8);
                    d = InternalReadByte(bits - 24);
                    break;
            }

            return (uint)(a | (b << 8) | (c << 16) | (d << 24));
        }

        public void WriteUInt(uint value)
        {
            WriteUInt(value, 32);
        }

        public uint ReadUInt()
        {
            return ReadUInt(32);
        }

        public void WriteInt(int value, int bits)
        {
            WriteUInt((uint)value, bits);
        }

        public int ReadInt(int bits)
        {
            return (int)ReadUInt(bits);
        }

        public void WriteInt(int value)
        {
            WriteInt(value, 32);
        }

        public int ReadInt()
        {
            return ReadInt(32);
        }

        public void WriteULong(ulong value, int bits)
        {
            if (bits <= 32)
            {
                WriteUInt((uint)(value & 0xFFFFFFFF), bits);
            }
            else
            {
                WriteUInt((uint)(value), 32);
                WriteUInt((uint)(value >> 32), bits - 32);
            }
        }

        public ulong ReadULong(int bits)
        {
            if (bits <= 32)
            {
                return ReadUInt(bits);
            }
            else
            {
                ulong a = ReadUInt(32);
                ulong b = ReadUInt(bits - 32);
                return a | (b << 32);
            }
        }

        public void WriteULong(ulong value)
        {
            WriteULong(value, 64);
        }

        public ulong ReadULong()
        {
            return ReadULong(64);
        }

        public void WriteLong(long value, int bits)
        {
            WriteULong((ulong)value, bits);
        }

        public long ReadLong(int bits)
        {
            return (long)ReadULong(bits);
        }

        public void WriteLong(long value)
        {
            WriteLong(value, 64);
        }

        public long ReadLong()
        {
            return ReadLong(64);
        }

        public void WriteFloat(float value)
        {
            ByteConverter bytes = value;
            InternalWriteByte(bytes.Byte0, 8);
            InternalWriteByte(bytes.Byte1, 8);
            InternalWriteByte(bytes.Byte2, 8);
            InternalWriteByte(bytes.Byte3, 8);
        }

        public float ReadFloat()
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Byte0 = InternalReadByte(8);
            bytes.Byte1 = InternalReadByte(8);
            bytes.Byte2 = InternalReadByte(8);
            bytes.Byte3 = InternalReadByte(8);
            return bytes.Float32;
        }

        public void WriteDouble(double value)
        {
            ByteConverter bytes = value;
            InternalWriteByte(bytes.Byte0, 8);
            InternalWriteByte(bytes.Byte1, 8);
            InternalWriteByte(bytes.Byte2, 8);
            InternalWriteByte(bytes.Byte3, 8);
            InternalWriteByte(bytes.Byte4, 8);
            InternalWriteByte(bytes.Byte5, 8);
            InternalWriteByte(bytes.Byte6, 8);
            InternalWriteByte(bytes.Byte7, 8);
        }

        public double ReadDouble()
        {
            ByteConverter bytes = default(ByteConverter);
            bytes.Byte0 = InternalReadByte(8);
            bytes.Byte1 = InternalReadByte(8);
            bytes.Byte2 = InternalReadByte(8);
            bytes.Byte3 = InternalReadByte(8);
            bytes.Byte4 = InternalReadByte(8);
            bytes.Byte5 = InternalReadByte(8);
            bytes.Byte6 = InternalReadByte(8);
            bytes.Byte7 = InternalReadByte(8);
            return bytes.Float64;
        }

        public void WriteByteArray(byte[] from)
        {
            WriteByteArray(from, 0, from.Length);
        }

        public void WriteByteArray(byte[] from, int count)
        {
            WriteByteArray(from, 0, count);
        }

        public void WriteByteArray(byte[] from, int offset, int count)
        {
            int p = position >> 3;
            int bitsUsed = position % 8;
            int bitsFree = 8 - bitsUsed;

            if (bitsUsed == 0)
            {
                Buffer.BlockCopy(from, offset, data, p, count);
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    byte value = from[offset + i];

                    data[p] &= (byte)(0xFF >> bitsFree);
                    data[p] |= (byte)(value << bitsUsed);

                    p += 1;

                    data[p] &= (byte)(0xFF << bitsUsed);
                    data[p] |= (byte)(value >> bitsFree);
                }
            }
            //TODO: IF WE GET PACKET ERRORS LOOK THIS OVER AGAIN
            position += (count * 8);
            //position += count;
        }

        public byte[] ReadByteArray(int size)
        {
            byte[] data = new byte[size];
            ReadByteArray(data);
            return data;
        }

        public void ReadByteArray(byte[] to)
        {
            ReadByteArray(to, 0, to.Length);
        }

        public void ReadByteArray(byte[] to, int count)
        {
            ReadByteArray(to, 0, count);
        }

        public void ReadByteArray(byte[] to, int offset, int count)
        {
            int p = position >> 3;
            int bitsUsed = position % 8;

            if (bitsUsed == 0)
            {
                Buffer.BlockCopy(data, p, to, offset, count);
            }
            else
            {
                int bitsNotUsed = 8 - bitsUsed;

                for (int i = 0; i < count; ++i)
                {
                    int first = data[p] >> bitsUsed;

                    p += 1;

                    int second = data[p] & (255 >> bitsNotUsed);
                    to[offset + i] = (byte)(first | (second << bitsNotUsed));
                }
            }
            //TODO: IF WE GET PACKET ERRORS LOOK THIS OVER AGAIN
            position += (count * 8);
            //position += count;
        }

        public void WriteByteArrayWithPrefix(byte[] array)
        {
            WriteByteArrayLengthPrefixed(array, 1 << 16);
        }

        public void WriteByteArrayLengthPrefixed(byte[] array, int maxLength)
        {
            if (WriteBool(array != null))
            {
                int length = Math.Min(array.Length, maxLength);

                if (length < array.Length)
                {
                    NetLog.Warn("Only sendig {0}/{1} bytes from byte array", length, array.Length);
                }

                WriteUShort((ushort)length);
                WriteByteArray(array, 0, length);
            }
        }

        public byte[] ReadByteArrayWithPrefix()
        {
            if (ReadBool())
            {
                int length = ReadUShort();
                byte[] data = new byte[length];

                ReadByteArray(data, 0, data.Length);

                return data;
            }
            else
            {
                return null;
            }
        }

        public void WriteString(string value, Encoding encoding)
        {
            WriteString(value, encoding, int.MaxValue);
        }

        public void WriteString(string value, Encoding encoding, int length)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteUShort(0);
            }
            else
            {
                if (length < value.Length)
                {
                    value = value.Substring(0, length);
                }

                byte[] bytes = encoding.GetBytes(value);
                WriteUShort((ushort)bytes.Length);
                WriteByteArray(bytes);
            }
        }

        public void WriteString(string value)
        {
            WriteString(value, Encoding.UTF8);
        }

        public string ReadString(Encoding encoding)
        {
            int byteCount = ReadUShort();

            if (byteCount == 0)
            {
                return "";
            }

            var bytes = new byte[byteCount];

            ReadByteArray(bytes);

            return encoding.GetString(bytes, 0, bytes.Length);
        }

        public string ReadString()
        {
            return ReadString(Encoding.UTF8);
        }

        public void WriteGuid(Guid guid)
        {
            WriteByteArray(guid.ToByteArray());
        }

        public Guid ReadGuid()
        {
            byte[] bytes = new byte[16];
            ReadByteArray(bytes);
            return new Guid(bytes);
        }

        public void WriteEndPoint(IPEndPoint endpoint)
        {
            var parts = endpoint.Address.ToString().Split('.');
            if (parts.Length != 4)
            {
                NetLog.Warn(string.Format("IP address: {0} was malformed, could not write.", endpoint.Address));
                return;
            }          

            WriteByte(Convert.ToByte(parts[0]));
            WriteByte(Convert.ToByte(parts[1]));
            WriteByte(Convert.ToByte(parts[2]));
            WriteByte(Convert.ToByte(parts[3]));
            WriteUShort((ushort)endpoint.Port);
        }

        public IPEndPoint ReadEndPoint()
        {

            byte a, b, c, d;

            a = ReadByte();
            b = ReadByte();
            c = ReadByte();
            d = ReadByte();

            ushort port = ReadUShort();

            string newAddress = a.ToString() + "." + b.ToString() + "." + c.ToString() + "." + d.ToString();
            string newEndPoint = newAddress + ":" + port;
            IPEndPoint endPoint = Utils.CreateIPEndPoint(newEndPoint);
            endPoint.Port = port;
            return endPoint;
        }

        void InternalWriteByte(byte value, int bits)
        {
            WriteByteAt(data, position, bits, value);

            position += bits;
        }

        public static void WriteByteAt(byte[] data, int position, int bits, byte value)
        {
            if (bits <= 0)
                return;

            value = (byte)(value & (0xFF >> (8 - bits)));

            int p = position >> 3;
            int bitsUsed = position & 0x7;
            int bitsFree = 8 - bitsUsed;
            int bitsLeft = bitsFree - bits;

            if (bitsLeft >= 0)
            {
                int mask = (0xFF >> bitsFree) | (0xFF << (8 - bitsLeft));
                data[p] = (byte)((data[p] & mask) | (value << bitsUsed));              
            }
            else
            {
                data[p] = (byte)((data[p] & (0xFF >> bitsFree)) | (value << bitsUsed));
                data[p + 1] = (byte)((data[p + 1] & (0xFF << (bits - bitsFree))) | (value >> bitsFree));
            }
        }

        byte InternalReadByte(int bits)
        {
            if (bits <= 0)
                return 0;

            byte value;
            int p = position >> 3;
            int bitsUsed = position % 8;

            if (bitsUsed == 0 && bits == 8)
            {
                value = data[p];
            }
            else
            {
                int first = data[p] >> bitsUsed;
                int remainingBits = bits - (8 - bitsUsed);

                if (remainingBits < 1)
                {
                    value = (byte)(first & (0xFF >> (8 - bits)));
                }
                else
                {
                    int second = data[p + 1] & (0xFF >> (8 - remainingBits));
                    value = (byte)(first | (second << (bits - remainingBits)));
                }
            }

            position += bits;
            return value;
        }
        #endregion

        public void Dispose()
        {
            if (pool != null)
            {
                pool.Release(this);

            }
        }
    }
}
