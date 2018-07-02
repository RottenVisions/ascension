namespace Ascension.Compiler
{
    public class PropertyDecoratorStruct : PropertyDecorator<PropertyTypeObject>
    {
        public ObjectDecorator Object
        {
            get { return Generator.FindStruct(PropertyType.StructGuid); }
        }

        public override int RequiredObjects
        {
            get { return Object.CountObjects; }
        }

        public override int RequiredStorage
        {
            get { return Object.CountStorage; }
        }

        public override int RequiredProperties
        {
            get { return Object.CountProperties; }
        }

        public override string ClrType
        {
            get { return Object.Name; }
        }

        public override string PropertyClassName
        {
            get { return null; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterStruct();
        }
    }
}