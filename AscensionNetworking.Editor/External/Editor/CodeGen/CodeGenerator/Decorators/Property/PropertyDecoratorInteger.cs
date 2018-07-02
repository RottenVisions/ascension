namespace Ascension.Compiler
{
    public class PropertyDecoratorInteger : PropertyDecorator<PropertyTypeInteger>
    {
        public override string ClrType
        {
            get { return typeof (int).FullName; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterInteger();
        }
    }
}