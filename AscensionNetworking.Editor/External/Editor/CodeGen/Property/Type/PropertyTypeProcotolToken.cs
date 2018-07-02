namespace Ascension.Compiler
{
    public class PropertyTypeProcotolToken : PropertyType
    {
        public override bool HasSettings
        {
            get { return false; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorProtocolToken();
        }
    }
}