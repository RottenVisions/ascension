using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeMatrix4x4 : PropertyType
    {
        public override bool HasSettings
        {
            get { return false; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorMatrix4x4();
        }
    }
}