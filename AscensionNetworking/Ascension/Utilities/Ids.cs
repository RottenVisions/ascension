using System.Collections.Generic;

namespace Ascension.Networking
{
    static class IdExtensions
    {
        public static void WritePrefabId(this BasePacket stream, PrefabId id)
        {
            stream.WriteIntVB(id.Value);
        }

        public static PrefabId ReadPrefabId(this BasePacket stream)
        {
            return new PrefabId(stream.ReadIntVB());
        }
        public static void WriteTypeId(this BasePacket stream, TypeId id)
        {
            stream.WriteIntVB(id.Value);
        }

        public static TypeId ReadTypeId(this BasePacket stream)
        {
            return new TypeId(stream.ReadIntVB());
        }
    }

    [System.Serializable]
    public struct PrefabId
    {
        public class Comparer : IComparer<PrefabId>
        {
            public static readonly Comparer Instance = new Comparer();

            Comparer()
            {

            }

            int IComparer<PrefabId>.Compare(PrefabId x, PrefabId y)
            {
                return x.Value - y.Value;
            }
        }

        public class EqualityComparer : IEqualityComparer<PrefabId>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            EqualityComparer()
            {

            }

            bool IEqualityComparer<PrefabId>.Equals(PrefabId x, PrefabId y)
            {
                return x.Value == y.Value;
            }

            int IEqualityComparer<PrefabId>.GetHashCode(PrefabId x)
            {
                return x.Value;
            }
        }

        public int Value;

        public PrefabId(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is PrefabId)
            {
                return this.Value == ((PrefabId)obj).Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[PrefabId:{0}]", Value);
        }

        public static bool operator ==(PrefabId a, PrefabId b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(PrefabId a, PrefabId b)
        {
            return a.Value != b.Value;
        }
    }
}
namespace Ascension.Networking
{
    [System.Serializable]
    public struct TypeId
    {
        public class Comparer : IComparer<TypeId>
        {
            public static readonly Comparer Instance = new Comparer();

            Comparer()
            {

            }

            int IComparer<TypeId>.Compare(TypeId x, TypeId y)
            {
                return x.Value - y.Value;
            }
        }

        public class EqualityComparer : IEqualityComparer<TypeId>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            EqualityComparer()
            {

            }

            bool IEqualityComparer<TypeId>.Equals(TypeId x, TypeId y)
            {
                return x.Value == y.Value;
            }

            int IEqualityComparer<TypeId>.GetHashCode(TypeId x)
            {
                return x.Value;
            }
        }

        public int Value;

        public TypeId(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeId)
            {
                return this.Value == ((TypeId)obj).Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[TypeId:{0}]", Value);
        }

        public static bool operator ==(TypeId a, TypeId b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(TypeId a, TypeId b)
        {
            return a.Value != b.Value;
        }
    }
}
