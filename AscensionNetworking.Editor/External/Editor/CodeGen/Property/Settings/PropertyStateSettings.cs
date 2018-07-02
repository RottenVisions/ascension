using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyStateSettings : PropertyAssetSettings
    {
        [ProtoMember(18)] public int _ExtrapolationCorrectionFrames = 6;
        [ProtoMember(6)] public float ExtrapolationErrorTolerance = 0.25f;
        [ProtoMember(19)] public int ExtrapolationMaxFrames = 9;
        [ProtoMember(12)] public float MecanimDamping;
        [ProtoMember(17)] public MecanimDirection MecanimDirection;
        [ProtoMember(14)] public int MecanimLayer;
        [ProtoMember(8)] public MecanimMode MecanimMode;
        [ProtoMember(5)] public SmoothingAlgorithms SmoothingAlgorithm;
        [ProtoMember(20)] public float SnapMagnitude;
    }
}