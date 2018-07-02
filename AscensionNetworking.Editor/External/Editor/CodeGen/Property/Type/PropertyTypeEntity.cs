using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeEntity : PropertyType
    {
        [ProtoMember(1)] public bool IsParent;

        public override bool HasSettings
        {
            get { return true; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorEntity();
        }
    }
}