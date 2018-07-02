namespace Ascension.Compiler
{
    public class PropertyDecoratorProtocolToken : PropertyDecorator<PropertyTypeProcotolToken>
    {
        public override string ClrType
        {
            get { return "Ascension.Networking.IProtocolToken"; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterProtocolToken();
        }
    }
}