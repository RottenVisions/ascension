namespace Ascension.Compiler
{
    public class PropertyDecoratorVector : PropertyDecorator<PropertyTypeVector>
    {
        public override string ClrType
        {
            get { return "UE.Vector3"; }
        }

        public override int RequiredStorage
        {
            get
            {
                if (Definition.StateAssetSettings != null &&
                    (Definition.StateAssetSettings.SmoothingAlgorithm != SmoothingAlgorithms.None))
                {
                    return 2;
                }

                return base.RequiredStorage;
            }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterVector();
        }
    }
}