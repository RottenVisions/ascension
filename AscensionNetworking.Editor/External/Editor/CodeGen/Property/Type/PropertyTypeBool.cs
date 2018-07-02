using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeBool : PropertyType
    {
        public override bool HasSettings
        {
            get { return false; }
        }

        public override bool MecanimApplicable
        {
            get { return true; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorBool();
        }
    }
}