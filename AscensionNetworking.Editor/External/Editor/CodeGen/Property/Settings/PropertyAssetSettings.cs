using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    [ProtoInclude(100, typeof (PropertyStateSettings))]
    [ProtoInclude(200, typeof (PropertyEventSettings))]
    [ProtoInclude(300, typeof (PropertyCommandSettings))]
    public abstract class PropertyAssetSettings
    {
    }
}