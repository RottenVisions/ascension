using System;
using System.CodeDom;

namespace Ascension.Compiler
{
    public class StateCodeEmitter : AssetCodeEmitter<StateDecorator>
    {
        protected override void EmitMetaInit(CodeMemberMethod method)
        {
            method.Statements.Expr("this.PropertyIdBits = {0}", BitsRequired(Decorator.CountProperties));

            method.Statements.Expr("this.PacketMaxBits = {0}", Decorator.Definition.PacketMaxBits);
            method.Statements.Expr("this.PacketMaxProperties = {0}", Decorator.Definition.PacketMaxProperties);
            method.Statements.Expr("this.PacketMaxPropertiesBits = {0}",
                BitsRequired(Decorator.Definition.PacketMaxProperties));

            base.EmitMetaInit(method);
        }

        private static int BitsRequired(int number)
        {
            if (number < 0)
            {
                return 32;
            }

            if (number == 0)
            {
                return 1;
            }

            for (int i = 31; i >= 0; --i)
            {
                int b = 1 << i;

                if ((number & b) == b)
                {
                    return i + 1;
                }
            }

            throw new Exception();
        }
    }
}