namespace Ascension.Compiler
{
    public class PropertyDecoratorPrefabId : PropertyDecorator<PropertyTypePrefabId>
    {
        public override string ClrType
        {
            get { return "Ascension.Networking.PrefabId"; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterPrefabId();
        }
    }
}