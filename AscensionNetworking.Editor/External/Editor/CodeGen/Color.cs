using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public struct Color4
    {
        [ProtoMember(4)] public float A;
        [ProtoMember(3)] public float B;
        [ProtoMember(2)] public float G;
        [ProtoMember(1)] public float R;

        public Color4(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}