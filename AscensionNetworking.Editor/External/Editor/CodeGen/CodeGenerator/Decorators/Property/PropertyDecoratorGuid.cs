using System;

namespace Ascension.Compiler
{
    public class PropertyDecoratorGuid : PropertyDecorator<PropertyTypeGuid>
    {
        public override string ClrType
        {
            get { return typeof (Guid).FullName; }
        }

        public override PropertyCodeEmitter CreateEmitter()
        {
            return new PropertyCodeEmitterGuid();
        }
    }
}