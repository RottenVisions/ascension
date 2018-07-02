using System.CodeDom;

namespace Ascension.Compiler
{
    public class CommandCodeEmitter : AssetCodeEmitter<CommandDecorator>
    {
        protected override void EmitObjectMembers(CodeTypeDeclaration type, bool inherited)
        {
            base.EmitObjectMembers(type, inherited);

            type.DeclareMethod("I" + Decorator.Name + "Input", "Create", method =>
            {
                method.Attributes |= MemberAttributes.Static;
                method.Statements.Expr("return new {0}().Input", Decorator.Name);
            });
        }

        protected override void EmitMetaInit(CodeMemberMethod method)
        {
            method.Statements.Expr("this.SmoothFrames = {0}", Decorator.Definition.SmoothFrames);
            method.Statements.Expr("this.CompressZeroValues = {0}",
                Decorator.Definition.CompressZeroValues.ToString().ToLowerInvariant());

            base.EmitMetaInit(method);
        }
    }
}