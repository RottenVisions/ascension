using System.CodeDom;

namespace Ascension.Compiler
{
    public class PropertyCodeEmitterFloat : PropertyCodeEmitter<PropertyDecoratorFloat>
    {
        public override string StorageField
        {
            get { return "Float0"; }
        }

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

        public override void AddSettings(CodeExpression expr, CodeStatementCollection statements)
        {
            if (Decorator.DefiningAsset is StateDecorator)
            {
                statements.Call(expr, "Settings_Float",
                    "new Ascension.Networking.PropertyFloatSettings {{ IsAngle = {0} }}".Expr(
                        Decorator.PropertyType.IsAngle.ToString().ToLowerInvariant()));
            }

            EmitFloatSettings(expr, statements, Decorator.PropertyType.Compression);
            EmitInterpolationSettings(expr, statements);
        }

        protected override void EmitSetPropertyValidator(CodeStatementCollection stmts, CodeTypeDeclaration type,
            CodeSnippetExpression storage, CodeTypeReference interfaceType, bool changed, string name)
        {
            FloatCompression c = Decorator.PropertyType.Compression;

            if (c != null && c.Enabled && c.BitsRequired < 32)
            {
#if DEBUG
                stmts.If("value < {0}f || value > {1}f".Expr(c.MinValue, c.MaxValue),
                    ifBody =>
                    {
                        ifBody.Expr(
                            "Ascension.Networking.NetLog.Warn(\"Property '{0}' is being set to a value larger than the compression settings, it will be clamped to [{1}f, {2}f]\")",
                            Decorator.Definition.Name, c.MinValue.ToStringSigned(), c.MaxValue.ToStringSigned());
                    });
#endif

                stmts.Expr("value = UnityEngine.Mathf.Clamp(value, {0}f, {1}f)", c.MinValue, c.MaxValue);
            }
        }
    }
}