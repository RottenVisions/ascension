namespace Ascension.Compiler
{
    public class PropertyDecoratorQuaternion : PropertyDecorator<PropertyTypeQuaternion>
    {
        public override string ClrType
        {
            get { return "UE.Quaternion"; }
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
            return new PropertyCodeEmitterQuaternion();
        }
    }
}