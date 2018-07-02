using ProtoBuf;

namespace Ascension.Compiler
{
    public class PropertyDecoratorArray : PropertyDecorator<PropertyTypeArray>
    {
        public override int RequiredObjects
        {
            get { return 1 + (ElementDecorator.RequiredObjects * PropertyType.ElementCount); }
        }

        public override int RequiredStorage
        {
            get { return (ElementDecorator.RequiredStorage * PropertyType.ElementCount); }
        }

        public override int RequiredProperties
        {
            get { return (ElementDecorator.RequiredProperties * PropertyType.ElementCount); }
        }

        public override string PropertyClassName
        {
            get { return null; }
        }

        public PropertyDecorator ElementDecorator
        {
            get
            {
                PropertyDefinition elementDefinition;

                elementDefinition = Serializer.DeepClone(Definition);
                elementDefinition.IsArrayElement = true;
                elementDefinition.PropertyType = PropertyType.ElementType;

                return Decorate(elementDefinition, DefiningAsset);
            }
        }

        public override string ClrType
        {
            get
            {
                if (ElementDecorator is PropertyDecoratorStruct)
                {
                    return "Ascension.Networking.NetworkArray_Objects<" + ElementDecorator.ClrType + ">";
                }

                return "Ascension.Networking.NetworkArray_" + ElementDecorator.GetType().Name.Replace("PropertyDecorator", "");
            }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterArray();
        }
    }
}