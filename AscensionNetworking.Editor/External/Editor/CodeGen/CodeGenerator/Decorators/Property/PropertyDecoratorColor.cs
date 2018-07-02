namespace Ascension.Compiler
{
    public class PropertyDecoratorColor : PropertyDecorator<PropertyTypeColor>
    {
        public override string ClrType
        {
            get { return "UE.Color"; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterColor();
        }
    }
}