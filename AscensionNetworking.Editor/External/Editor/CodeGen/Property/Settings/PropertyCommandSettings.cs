using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyCommandSettings : PropertyAssetSettings
    {
        [ProtoMember(1)] public bool SmoothCorrection;
        [ProtoMember(2)] public float SnapMagnitude;
    }
}