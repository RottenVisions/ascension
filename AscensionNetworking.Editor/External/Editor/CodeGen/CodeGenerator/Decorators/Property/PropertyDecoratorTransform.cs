namespace Ascension.Compiler
{
    public class PropertyDecoratorTransform : PropertyDecorator<PropertyTypeTransform>
    {
        public override int RequiredStorage
        {
            get { return 3; }
        }

        public override int RequiredObjects
        {
            get { return 0; }
        }

        public override string ClrType
        {
            get { return "Ascension.Networking.NetworkTransform"; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterTransform();
        }
    }
}