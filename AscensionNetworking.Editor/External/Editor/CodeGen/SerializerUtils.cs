using System.IO;
using ProtoBuf;

namespace Ascension.Compiler
{
    public static class SerializerUtils
    {
        public static byte[] ToByteArray<T>(this T obj)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static T ToObject<T>(this byte[] data) where T : class
        {
            MemoryStream ms;

            ms = new MemoryStream(data);
            ms.Position = 0;

            return Serializer.Deserialize<T>(ms);
        }

        public static T DeepClone<T>(this T obj)
        {
            return Serializer.DeepClone(obj);
        }
    }
}