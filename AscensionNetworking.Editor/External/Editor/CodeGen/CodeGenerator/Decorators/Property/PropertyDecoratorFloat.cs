namespace Ascension.Compiler
{
    public class PropertyDecoratorFloat : PropertyDecorator<PropertyTypeFloat>
    {
        public override string ClrType
        {
            get { return typeof (float).FullName; }
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
            return new PropertyCodeEmitterFloat();
        }
    }
}