namespace Ascension.Compiler
{
    public class PropertyDecoratorNetworkId : PropertyDecorator<PropertyTypeNetworkId>
    {
        public override string ClrType
        {
            get { return "Ascension.Networking.NetworkId"; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterNetworkId();
        }
    }
}