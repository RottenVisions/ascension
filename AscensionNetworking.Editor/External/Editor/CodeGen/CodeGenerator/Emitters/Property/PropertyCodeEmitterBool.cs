namespace Ascension.Compiler
{
    public class PropertyCodeEmitterBool : PropertyCodeEmitter<PropertyDecoratorBool>
    {
        public override bool AllowSetter
        {
            get
            {
                PropertyStateSettings s = Decorator.Definition.StateAssetSettings;
                if (s != null)
                {
                    return s.MecanimMode == MecanimMode.Disabled ||
                           s.MecanimDirection == MecanimDirection.UsingAscensionProperties;
                }

                return true;
            }
        }
    }
}