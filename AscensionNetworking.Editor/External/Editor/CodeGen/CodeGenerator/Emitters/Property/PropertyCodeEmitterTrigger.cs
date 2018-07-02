using System.CodeDom;

namespace Ascension.Compiler
{
    public class PropertyCodeEmitterTrigger : PropertyCodeEmitter<PropertyDecoratorTrigger>
    {
        public override string StorageField
        {
            get { return "Action"; }
        }

        public override bool VerifyModify
        {
            get { return false; }
        }

        public override void EmitObjectMembers(CodeTypeDeclaration type)
        {
            EmitSimplePropertyMembers(type, new CodeSnippetExpression("Storage"), null, false, Decorator.TriggerListener);

            PropertyStateSettings s = Decorator.Definition.StateAssetSettings;

            // don't emit this method if we we are pulling data from mecanim
            if (s.MecanimMode == MecanimMode.Disabled || s.MecanimDirection == MecanimDirection.UsingAscensionProperties)
            {
                type.DeclareMethod(typeof (void).FullName, Decorator.TriggerMethod, method =>
                {
                    // make sure this peer is allowed to modify this property
                    EmitAllowedCheck(method.Statements);

                    // update local trigger
                    method.Statements.Expr(
                        "Storage.Values[this.OffsetStorage + {0}].TriggerLocal.Update(Ascension.Networking.Core.Frame, true)",
                        Decorator.OffsetStorage);

                    // flag this property as changed
                    EmitPropertyChanged(method.Statements, new CodeSnippetExpression("Storage"));
                });
            }
        }
    }
}