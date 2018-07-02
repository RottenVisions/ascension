using System;
using System.CodeDom;

namespace Ascension.Compiler
{
    public class PropertyCodeEmitterStruct : PropertyCodeEmitter<PropertyDecoratorStruct>
    {
        private void DeclareProperty(CodeTypeDeclaration type, bool emitSetter)
        {
            Action<CodeStatementCollection> getter =
                get =>
                {
                    get.Expr("return ({0})(Objects[this.OffsetObjects + {1}])", Decorator.ClrType,
                        Decorator.OffsetObjects);
                };

            type.DeclareProperty(
                Decorator.Object.EmitAsInterface ? Decorator.Object.NameInterface : Decorator.Object.Name,
                Decorator.Definition.Name, getter, null);
        }

        public override void EmitObjectMembers(CodeTypeDeclaration type)
        {
            DeclareProperty(type, false);
        }

        public override void EmitMetaSetup(DomBlock block, Offsets offsets, CodeExpression indexExpression)
        {
            block.Add("this".Expr().Call("CopyProperties",
                offsets.OffsetProperties,
                offsets.OffsetObjects,
                Decorator.Object.NameMeta.Expr().Field("Instance"),
                Decorator.Definition.Name.Literal(),
                indexExpression ?? (-1).Literal()
                ));
        }

        public override void EmitObjectSetup(DomBlock block, Offsets offsets)
        {
            EmitInitObject(Decorator.ClrType, block, offsets);
        }
    }
}