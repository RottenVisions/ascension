using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeFloat : PropertyType
    {
        [ProtoMember(1)] public FloatCompression Compression;
        [ProtoMember(2)] public bool IsAngle;

        public override bool HasSettings
        {
            get { return true; }
        }

        public override bool MecanimApplicable
        {
            get { return true; }
        }

        public override bool InterpolateAllowed
        {
            get { return true; }
        }

        public override bool CanSmoothCorrections
        {
            get { return true; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorFloat();
        }
    }
}