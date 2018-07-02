using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class FilterDefinition
    {
        [ProtoMember(4)] public Color4 Color;
        [ProtoMember(5)] public bool Enabled;
        [ProtoMember(1)] public int Index;
        [ProtoMember(3)] public string Name;

        public int Bit
        {
            get { return 1 << Index; }
        }

        public bool IsOn(int bits)
        {
            return (bits & Bit) == Bit;
        }
    }
}