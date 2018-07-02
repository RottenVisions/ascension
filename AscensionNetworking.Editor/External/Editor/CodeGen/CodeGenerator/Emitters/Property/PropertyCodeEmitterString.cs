using System.CodeDom;

namespace Ascension.Compiler
{
    public class PropertyCodeEmitterString : PropertyCodeEmitter<PropertyDecoratorString>
    {
        public override void AddSettings(CodeExpression expr, CodeStatementCollection statements)
        {
            statements.Call(expr, "AddStringSettings", Decorator.PropertyType.Encoding.Literal());
        }
    }
}