using ProtoBuf;

namespace Ascension.Compiler
{
    public static class Axis
    {
        public const int X = 0;
        public const int Y = 1;
        public const int Z = 2;
    }

    [ProtoContract]
    public class PropertyTypeVector : PropertyType
    {
        [ProtoMember(18, OverwriteList = true)] public FloatCompression[] Compression = new FloatCompression[3]
        {
            new FloatCompression {MinValue = -1024, MaxValue = +1024, Accuracy = 0.01f},
            new FloatCompression {MinValue = -1024, MaxValue = +1024, Accuracy = 0.01f},
            new FloatCompression {MinValue = -1024, MaxValue = +1024, Accuracy = 0.01f}
        };

        [ProtoMember(16)] public AxisSelections Selection = AxisSelections.XYZ;
        [ProtoMember(19)] public bool StrictEquality;

        public override bool StrictCompare
        {
            get { return StrictEquality; }
        }

        public override bool InterpolateAllowed
        {
            get { return true; }
        }

        public override bool HasSettings
        {
            get { return true; }
        }

        public override bool CanSmoothCorrections
        {
            get { return true; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorVector();
        }

        public override void OnCreated()
        {
            Selection = AxisSelections.XYZ;
        }
    }
}