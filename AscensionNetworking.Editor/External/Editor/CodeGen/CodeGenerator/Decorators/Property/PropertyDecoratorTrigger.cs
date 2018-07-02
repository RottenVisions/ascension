namespace Ascension.Compiler
{
    public class PropertyDecoratorTrigger : PropertyDecorator
    {
        public override string ClrType
        {
            get { return "System.Action"; }
        }

        public string TriggerListener
        {
            get { return "On" + Definition.Name; }
        }

        public string TriggerMethod
        {
            get { return Definition.Name; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterTrigger();
        }
    }
}