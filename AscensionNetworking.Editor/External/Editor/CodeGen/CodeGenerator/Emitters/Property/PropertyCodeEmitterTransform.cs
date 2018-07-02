using System.CodeDom;

namespace Ascension.Compiler
{
    public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform>
    {
        public override string StorageField
        {
            get { return "Transform"; }
        }

        public override void AddSettings(CodeExpression expr, CodeStatementCollection stmts)
        {
            PropertyTypeTransform pt = Decorator.PropertyType;

            stmts.Call(expr, "Settings_Space", "Ascension.Networking.TransformSpaces.{0}".Expr(Decorator.PropertyType.Space));

            EmitVectorSettings(expr, stmts, pt.PositionCompression, pt.PositionSelection, pt.PositionStrictCompare);
            EmitQuaternionSettings(expr, stmts, pt.RotationCompression, pt.RotationCompressionQuaternion,
                pt.RotationSelection, pt.RotationStrictCompare);

            switch (Decorator.Definition.StateAssetSettings.SmoothingAlgorithm)
            {
                case SmoothingAlgorithms.Interpolation:
                    EmitInterpolationSettings(expr, stmts);
                    break;

                case SmoothingAlgorithms.Extrapolation:
                    EmitExtrapolationSettings(expr, stmts);
                    break;
            }
        }

        public override void EmitObjectMembers(CodeTypeDeclaration type)
        {
            type.DeclareProperty("Ascension.Networking.NetworkTransform", Decorator.Definition.Name,
                get =>
                {
                    get.Expr("return Storage.Values[this.OffsetStorage + {0}].Transform", Decorator.OffsetStorage);
                });
        }

        private void EmitExtrapolationSettings(CodeExpression expr, CodeStatementCollection stmts)
        {
            PropertyStateSettings s = Decorator.Definition.StateAssetSettings;

            stmts.Call(expr, "Settings_Extrapolation",
                "Ascension.Networking.PropertyExtrapolationSettings".Expr().Call("Create",
                    s.ExtrapolationMaxFrames.Literal(),
                    s.ExtrapolationErrorTolerance.Literal(),
                    s.SnapMagnitude.Literal(),
                    Decorator.PropertyType.ExtrapolationVelocityMode.Literal()
                    )
                );
        }
    }
}