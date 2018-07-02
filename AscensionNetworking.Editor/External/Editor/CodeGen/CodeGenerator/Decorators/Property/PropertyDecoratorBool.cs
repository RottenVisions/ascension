namespace Ascension.Compiler
{
    public class PropertyDecoratorBool : PropertyDecorator<PropertyTypeBool>
    {
        public override string ClrType
        {
            get { return typeof (bool).FullName; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterBool();
        }
    }
}