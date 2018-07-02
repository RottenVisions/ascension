namespace Ascension.Compiler
{
    public class PropertyDecoratorColor32 : PropertyDecorator<PropertyTypeColor>
    {
        public override string ClrType
        {
            get { return "UE.Color32"; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterColor32();
        }
    }
}