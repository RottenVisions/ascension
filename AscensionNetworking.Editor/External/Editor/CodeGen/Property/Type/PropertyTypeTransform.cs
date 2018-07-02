using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeTransform : PropertyType
    {
        [ProtoMember(10)] public ExtrapolationVelocityModes ExtrapolationVelocityMode =
            ExtrapolationVelocityModes.CalculateFromPosition;

        [ProtoMember(8, OverwriteList = true)] public FloatCompression[] PositionCompression = new FloatCompression[3]
        {
            new FloatCompression {MinValue = -1024, MaxValue = +1024, Accuracy = 0.01f},
            new FloatCompression {MinValue = -1024, MaxValue = +1024, Accuracy = 0.01f},
            new FloatCompression {MinValue = -1024, MaxValue = +1024, Accuracy = 0.01f}
        };

        [ProtoMember(6)] public AxisSelections PositionSelection = AxisSelections.Disabled;
        [ProtoMember(21)] public bool PositionStrictCompare;

        [ProtoMember(9, OverwriteList = true)] public FloatCompression[] RotationCompression = new FloatCompression[3]
        {
            new FloatCompression {MinValue = 0, MaxValue = +360, Accuracy = 0.01f},
            new FloatCompression {MinValue = 0, MaxValue = +360, Accuracy = 0.01f},
            new FloatCompression {MinValue = 0, MaxValue = +360, Accuracy = 0.01f}
        };

        [ProtoMember(4)] public FloatCompression RotationCompressionQuaternion =
            new FloatCompression {MinValue = -1, MaxValue = +1, Accuracy = 0.01f};

        [ProtoMember(7)] public AxisSelections RotationSelection = AxisSelections.Disabled;
        [ProtoMember(22)] public bool RotationStrictCompare;
        [ProtoMember(24)] public TransformSpaces Space = TransformSpaces.Local;

        public override bool InterpolateAllowed
        {
            get { return true; }
        }

        public override bool CallbackAllowed
        {
            get { return false; }
        }

        public override bool HasSettings
        {
            get { return true; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorTransform();
        }

        public override void OnCreated()
        {
            PositionSelection = AxisSelections.XYZ;
            RotationSelection = AxisSelections.XYZ;
        }
    }
}