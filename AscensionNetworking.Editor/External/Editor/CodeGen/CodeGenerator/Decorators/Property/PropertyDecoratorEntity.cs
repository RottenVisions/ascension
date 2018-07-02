namespace Ascension.Compiler
{
    public class PropertyDecoratorEntity : PropertyDecorator<PropertyTypeEntity>
    {
        public override string ClrType
        {
            get { return "AscensionEntity"; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterEntity();
        }
    }
}