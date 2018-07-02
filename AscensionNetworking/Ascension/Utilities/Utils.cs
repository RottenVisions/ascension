using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Ascension.Networking.Sockets;
using UnityEngine;

namespace Ascension.Networking
{

    public static class Utils
    {
        public static string LocalNetIp()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        public static IPAddress LocalHost()
        {
            return IPAddress.Parse("127.0.0.1");
        }

        public static IPEndPoint LocalHostEndPoint(int port)
        {
            return CreateIPEndPoint("127.0.0.1" + ":" + port.ToString());
        }

        public static IPEndPoint AnyEndPoint()
        {
            return new IPEndPoint(IPAddress.Any, 0);
        }

        public static IPEndPoint CreateIPEndPoint(string address, int port)
        {
            return CreateIPEndPoint(address + ":" + port);
        }

        // Handles IPv4 and IPv6 notation.
        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public static Connection GetAscensionConnection(this SocketConnection self)
        {
            return (Connection)self.UserToken;
        }

        public static IEnumerable<Type> FindInterfaceImplementationsOld(this Type t)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.IsClass && t.IsAssignableFrom(type))
                    {
                        yield return type;
                    }
                }
            }
        }

        public static List<Type> FindInterfaceImplementations(this Type t)
        {
            List<Type> found = new List<Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.IsClass && t.IsAssignableFrom(type))
                    {
                        if (!found.Contains(type))
                            found.Add(type);
                    }
                }
            }
            return found;
        }

        public static bool NullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }

        public static bool Has<T>(this T[] array, int index) where T : class
        {
            return index < array.Length && array[index] != null;
        }

        public static bool Has<T>(this T[] array, uint index) where T : class
        {
            return index < array.Length && array[index] != null;
        }

        public static bool TryGetIndex<T>(this T[] array, int index, out T value) where T : class
        {
            if (index < array.Length)
                return (value = array[index]) != null;

            value = default(T);
            return false;
        }

        public static bool TryGetIndex<T>(this T[] array, uint index, out T value) where T : class
        {
            if (index < array.Length)
                return (value = array[index]) != null;

            value = default(T);
            return false;
        }

        public static T FindComponent<T>(this Component component) where T : Component
        {
            return FindComponent<T>(component.transform);
        }

        public static T FindComponent<T>(this GameObject gameObject) where T : Component
        {
            return FindComponent<T>(gameObject.transform);
        }

        public static T FindComponent<T>(this Transform transform) where T : Component
        {
            T component = null;

            while (transform && !component)
            {
                component = transform.GetComponent<T>();
                transform = transform.parent;
            }

            return component;
        }

        /// <summary>
        /// Splits an array into several smaller arrays.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="size">The size of the smaller arrays.</param>
        /// <returns>An array containing smaller arrays.</returns>
        /*public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }*/

        public static T[][] Split<T>(this T[] arrayIn, int length)
        {
            bool even = arrayIn.Length % length == 0;
            int totalLength = arrayIn.Length / length;
            if (!even)
                totalLength++;

            T[][] newArray = new T[totalLength][];
            for (int i = 0; i < totalLength; ++i)
            {
                int allocLength = length;
                if (!even && i == totalLength - 1)
                    allocLength = arrayIn.Length % length;

                newArray[i] = new T[allocLength];
                Array.Copy(arrayIn, i * length, newArray[i], 0, allocLength);
            }
            return newArray;
        }

        public static T[] Concat<T>(this T[][] list)
        {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }

        public static string Join<T>(this IEnumerable<T> items, string seperator)
        {
            return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
        }

        public static bool ViewPointIsOnScreen(this Vector3 vp)
        {
            return vp.z >= 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
        }

        public static T[] CloneArray<T>(this T[] array)
        {
            T[] clone = new T[array.Length];
            Array.Copy(array, 0, clone, 0, array.Length);
            return clone;
        }

        public static T[] AddFirst<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[1] { item };
            }

            // duplicate + 1 extra slot
            T[] clone = new T[array.Length + 1];

            // copy old items to index 1 ... n
            Array.Copy(array, 0, clone, 1, array.Length);

            // insert new item at index 0
            clone[0] = item;

            return clone;
        }

        public static void WriteUniqueId(this BasePacket stream, UniqueId id)
        {
            stream.WriteUInt(id.uint0);
            stream.WriteUInt(id.uint1);
            stream.WriteUInt(id.uint2);
            stream.WriteUInt(id.uint3);
        }

        public static UniqueId ReadUniqueId(this BasePacket stream)
        {
            UniqueId id;

            id = default(UniqueId);
            id.uint0 = stream.ReadUInt();
            id.uint1 = stream.ReadUInt();
            id.uint2 = stream.ReadUInt();
            id.uint3 = stream.ReadUInt();

            return id;
        }

        public static void WriteByteArraySimple(this BasePacket stream, byte[] array, int maxLength)
        {
            if (stream.WriteBool(array != null))
            {
                int length = Mathf.Min(array.Length, maxLength);

                if (length < array.Length)
                {
                    NetLog.Warn("Only sending {0}/{1} bytes from byte array", length, array.Length);
                }

                stream.WriteUShort((ushort)length);
                stream.WriteByteArray(array, 0, length);
            }
        }

        public static byte[] ReadByteArraySimple(this BasePacket stream)
        {
            if (stream.ReadBool())
            {
                int length = stream.ReadUShort();
                byte[] data = new byte[length];

                stream.ReadByteArray(data, 0, data.Length);

                return data;
            }
            else
            {
                return null;
            }
        }

        public static void WriteColor32RGBA(this BasePacket stream, Color32 value)
        {
            stream.WriteByte(value.r, 8);
            stream.WriteByte(value.g, 8);
            stream.WriteByte(value.b, 8);
            stream.WriteByte(value.a, 8);
        }

        public static Color32 ReadColor32RGBA(this BasePacket stream)
        {
            return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8));
        }

        public static void WriteColor32RGB(this BasePacket stream, Color32 value)
        {
            stream.WriteByte(value.r, 8);
            stream.WriteByte(value.g, 8);
            stream.WriteByte(value.b, 8);
        }

        public static Color32 ReadColor32RGB(this BasePacket stream)
        {
            return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), 0xFF);
        }

        public static void WriteVector2(this BasePacket stream, Vector2 value)
        {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
        }

        public static Vector2 ReadVector2(this BasePacket stream)
        {
            return new Vector2(stream.ReadFloat(), stream.ReadFloat());
        }


        public static void WriteVector3(this BasePacket stream, Vector3 value)
        {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
            stream.WriteFloat(value.z);
        }

        public static Vector3 ReadVector3(this BasePacket stream)
        {
            return new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }


        public static void WriteColorRGB(this BasePacket stream, Color value)
        {
            stream.WriteFloat(value.r);
            stream.WriteFloat(value.g);
            stream.WriteFloat(value.b);
        }

        public static Color ReadColorRGB(this BasePacket stream)
        {
            return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }


        public static void WriteVector4(this BasePacket stream, Vector4 value)
        {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
            stream.WriteFloat(value.z);
            stream.WriteFloat(value.w);
        }

        public static Vector4 ReadVector4(this BasePacket stream)
        {
            return new Vector4(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }
        public static void WriteColorRGBA(this BasePacket stream, Color value)
        {
            stream.WriteFloat(value.r);
            stream.WriteFloat(value.g);
            stream.WriteFloat(value.b);
            stream.WriteFloat(value.a);
        }

        public static Color ReadColorRGBA(this BasePacket stream)
        {
            return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteQuaternion(this BasePacket stream, Quaternion value)
        {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
            stream.WriteFloat(value.z);
            stream.WriteFloat(value.w);
        }

        public static Quaternion ReadQuaternion(this BasePacket stream)
        {
            return new Quaternion(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteTransform(this BasePacket stream, Transform transform)
        {
            WriteVector3(stream, transform.position);
            WriteQuaternion(stream, transform.rotation);
        }

        public static void ReadTransform(this BasePacket stream, Transform transform)
        {
            transform.position = ReadVector3(stream);
            transform.rotation = ReadQuaternion(stream);
        }

        public static void ReadTransform(this BasePacket stream, out Vector3 position, out Quaternion rotation)
        {
            position = ReadVector3(stream);
            rotation = ReadQuaternion(stream);
        }


        public static void WriteRigidbody(this BasePacket stream, Rigidbody rigidbody)
        {
            WriteVector3(stream, rigidbody.position);
            WriteQuaternion(stream, rigidbody.rotation);
            WriteVector3(stream, rigidbody.velocity);
            WriteVector3(stream, rigidbody.angularVelocity);
        }

        public static void ReadRigidbody(this BasePacket stream, Rigidbody rigidbody)
        {
            rigidbody.position = ReadVector3(stream);
            rigidbody.rotation = ReadQuaternion(stream);
            rigidbody.velocity = ReadVector3(stream);
            rigidbody.angularVelocity = ReadVector3(stream);
        }

        public static void ReadRigidbody(this BasePacket stream, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity)
        {
            position = ReadVector3(stream);
            rotation = ReadQuaternion(stream);
            velocity = ReadVector3(stream);
            angularVelocity = ReadVector3(stream);
        }

        public static void WriteBounds(this BasePacket stream, Bounds b)
        {
            WriteVector3(stream, b.center);
            WriteVector3(stream, b.size);
        }

        public static Bounds ReadBounds(this BasePacket stream)
        {
            return new Bounds(ReadVector3(stream), ReadVector3(stream));
        }

        public static void WriteRect(this BasePacket stream, Rect rect)
        {
            stream.WriteFloat(rect.xMin);
            stream.WriteFloat(rect.yMin);
            stream.WriteFloat(rect.width);
            stream.WriteFloat(rect.height);
        }

        public static Rect ReadRect(this BasePacket stream)
        {
            return new Rect(
                stream.ReadFloat(),
                stream.ReadFloat(),
                stream.ReadFloat(),
                stream.ReadFloat()
            );
        }


        public static void WriteRay(this BasePacket stream, Ray ray)
        {
            WriteVector3(stream, ray.origin);
            WriteVector3(stream, ray.direction);
        }

        public static Ray ReadRay(this BasePacket stream)
        {
            return new Ray(
                ReadVector3(stream),
                ReadVector3(stream)
            );
        }


        public static void WritePlane(this BasePacket stream, Plane plane)
        {
            WriteVector3(stream, plane.normal);
            stream.WriteFloat(plane.distance);
        }

        public static Plane ReadPlane(this BasePacket stream)
        {
            return new Plane(
              ReadVector3(stream),
              stream.ReadFloat()
            );
        }


        public static void WriteLayerMask(this BasePacket stream, LayerMask mask)
        {
            stream.WriteInt(mask.value, 32);
        }

        public static LayerMask ReadLayerMask(this BasePacket stream)
        {
            return stream.ReadInt(32);
        }

        public static void WriteMatrix4x4(this BasePacket stream, Matrix4x4 m)
        {
            stream.WriteFloat(m.m00);
            stream.WriteFloat(m.m01);
            stream.WriteFloat(m.m02);
            stream.WriteFloat(m.m03);
            stream.WriteFloat(m.m10);
            stream.WriteFloat(m.m11);
            stream.WriteFloat(m.m12);
            stream.WriteFloat(m.m13);
            stream.WriteFloat(m.m20);
            stream.WriteFloat(m.m21);
            stream.WriteFloat(m.m22);
            stream.WriteFloat(m.m23);
            stream.WriteFloat(m.m30);
            stream.WriteFloat(m.m31);
            stream.WriteFloat(m.m32);
            stream.WriteFloat(m.m33);
        }

        public static Matrix4x4 ReadMatrix4x4(this BasePacket stream)
        {
            Matrix4x4 m = default(Matrix4x4);
            m.m00 = stream.ReadFloat();
            m.m01 = stream.ReadFloat();
            m.m02 = stream.ReadFloat();
            m.m03 = stream.ReadFloat();
            m.m10 = stream.ReadFloat();
            m.m11 = stream.ReadFloat();
            m.m12 = stream.ReadFloat();
            m.m13 = stream.ReadFloat();
            m.m20 = stream.ReadFloat();
            m.m21 = stream.ReadFloat();
            m.m22 = stream.ReadFloat();
            m.m23 = stream.ReadFloat();
            m.m30 = stream.ReadFloat();
            m.m31 = stream.ReadFloat();
            m.m32 = stream.ReadFloat();
            m.m33 = stream.ReadFloat();
            return m;
        }

        public static void WriteIntVB(this BasePacket packet, int v)
        {
            packet.WriteUIntVB((uint)v);
        }

        public static int ReadIntVB(this BasePacket packet)
        {
            return (int)packet.ReadUIntVB();
        }

        public static void WriteUIntVB(this BasePacket packet, uint v)
        {
            uint b = 0U;

            do
            {
                b = v & 127U;
                v = v >> 7;

                if (v > 0)
                {
                    b |= 128U;
                }

                packet.WriteByte((byte)b);
            } while (v != 0);
        }

        public static uint ReadUIntVB(this BasePacket packet)
        {
            uint v = 0U;
            uint b = 0U;

            int s = 0;

            do
            {
                b = packet.ReadByte();
                v = v | ((b & 127U) << s);
                s = s + 7;

            } while (b > 127U);

            return v;
        }

        public static void WriteLongVB(this BasePacket p, long v)
        {
            p.WriteULongVB((ulong)v);
        }

        public static long ReadLongVB(this BasePacket p)
        {
            return (long)p.ReadULongVB();
        }

        public static void WriteULongVB(this BasePacket p, ulong v)
        {
            ulong b = 0U;

            do
            {
                b = v & 127U;
                v = v >> 7;

                if (v > 0)
                {
                    b |= 128U;
                }

                p.WriteByte((byte)b);
            } while (v != 0);
        }

        public static ulong ReadULongVB(this BasePacket p)
        {
            ulong v = 0U;
            ulong b = 0U;

            int s = 0;

            do
            {
                b = p.ReadByte();
                v = v | ((b & 127U) << s);
                s = s + 7;

            } while (b > 127U);

            return v;
        }

        public static void WriteBoltEntity(this BasePacket packet, AscensionEntity entity)
        {
            WriteEntity(packet, entity == null ? null : entity.AEntity);
        }

        public static AscensionEntity ReadBoltEntity(this BasePacket packet)
        {
            Entity entity = ReadEntity(packet);

            if (entity)
            {
                return entity.UnityObject;
            }

            return null;
        }

        public static void WriteEntity(this BasePacket packet, Entity entity)
        {
            if (packet.WriteBool((entity != null) && entity.IsAttached))
            {
                packet.WriteNetworkId(entity.NetworkId);
            }
        }

        public static Entity ReadEntity(this BasePacket packet)
        {
            if (packet.ReadBool())
            {
                return Core.FindEntity(packet.ReadNetworkId());
            }

            return null;
        }

        public static void WriteNetworkId(this BasePacket packet, NetworkId id)
        {
            NetAssert.True(id.Connection != uint.MaxValue);
            packet.WriteUIntVB(id.Connection);
            packet.WriteUIntVB(id.Entity);
        }

        public static NetworkId ReadNetworkId(this BasePacket packet)
        {
            uint connection = packet.ReadUIntVB();
            uint entity = packet.ReadUIntVB();
            NetAssert.True(connection != uint.MaxValue);
            return new NetworkId(connection, entity);
        }

        public static void WriteContinueMarker(this BasePacket stream)
        {
            if (stream.CanWrite())
            {
                stream.WriteBool(true);
            }
        }

        public static void WriteStopMarker(this BasePacket stream)
        {
            if (stream.CanWrite())
            {
                stream.WriteBool(false);
            }
        }

        public static bool ReadStopMarker(this BasePacket stream)
        {
            if (stream.CanRead())
            {
                return stream.ReadBool();
            }

            return false;
        }
        static void ByteToString(byte value, StringBuilder sb)
        {
            ByteToString(value, 8, sb);
        }

        static void ByteToString(byte value, int bits, StringBuilder sb)
        {
#if DEBUG
            if (bits < 1 || bits > 8)
            {
                throw new ArgumentOutOfRangeException("bits", "Must be between 1 and 8");
            }
#endif

            for (int i = (bits - 1); i >= 0; --i)
            {
                if (((1 << i) & value) == 0)
                {
                    sb.Append('0');
                }
                else
                {
                    sb.Append('1');
                }
            }
        }

        public static string ByteToString(byte value, int bits)
        {
            StringBuilder sb = new StringBuilder(8);
            ByteToString(value, bits, sb);
            return sb.ToString();
        }

        public static string ByteToString(byte value)
        {
            return ByteToString(value, 8);
        }

        public static string UShortToString(ushort value)
        {
            StringBuilder sb = new StringBuilder(17);

            ByteToString((byte)(value >> 8), sb);
            sb.Append(' ');
            ByteToString((byte)value, sb);

            return sb.ToString();
        }

        public static string IntToString(int value)
        {
            return UIntToString((uint)value);
        }

        public static string UIntToString(uint value)
        {
            StringBuilder sb = new StringBuilder(35);

            ByteToString((byte)(value >> 24), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 16), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 8), sb);
            sb.Append(' ');
            ByteToString((byte)value, sb);

            return sb.ToString();
        }

        public static string ULongToString(ulong value)
        {
            StringBuilder sb = new StringBuilder(71);

            ByteToString((byte)(value >> 56), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 48), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 40), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 32), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 24), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 16), sb);
            sb.Append(' ');
            ByteToString((byte)(value >> 8), sb);
            sb.Append(' ');
            ByteToString((byte)value, sb);

            return sb.ToString();
        }

        public static string BytesToString(byte[] values)
        {
            StringBuilder sb = new StringBuilder(
                (values.Length * 8) + System.Math.Max(0, (values.Length - 1))
            );

            for (int i = values.Length - 1; i >= 0; --i)
            {
                sb.Append(ByteToString(values[i]));

                if (i != 0)
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }

    }

}
