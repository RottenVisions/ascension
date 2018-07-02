using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeNetworkId : PropertyType
    {
        public override bool HasSettings
        {
            get { return false; }
        }

        public override bool MecanimApplicable
        {
            get { return false; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorNetworkId();
        }
    }
}