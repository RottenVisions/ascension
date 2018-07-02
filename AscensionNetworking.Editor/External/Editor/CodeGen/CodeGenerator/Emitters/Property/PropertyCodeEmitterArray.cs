using System.CodeDom;

namespace Ascension.Compiler
{
    public class PropertyCodeEmitterArray : PropertyCodeEmitter<PropertyDecoratorArray>
    {
        public override void EmitObjectMembers(CodeTypeDeclaration type)
        {
            type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name,
                get =>
                {
                    get.Expr("return ({0}) (Objects[this.OffsetObjects + {1}])", Decorator.ClrType,
                        Decorator.OffsetObjects);
                });
        }

        public override void EmitObjectSetup(DomBlock block, Offsets offsets)
        {
            PropertyDecorator element = Decorator.ElementDecorator;

            if (element is PropertyDecoratorStruct)
            {
                PropertyDecoratorStruct structDecorator = (PropertyDecoratorStruct) element;
                EmitInitObject(Decorator.ClrType, block, offsets, /* size */
                    Decorator.PropertyType.ElementCount.Literal(), /* stride */
                    structDecorator.RequiredObjects.Literal());

                var tmp = block.TempVar();
                element.Definition.Name += "[]";

                offsets.OffsetStorage = "offsets.OffsetStorage + {0} + ({1} * {2})".Expr(Decorator.OffsetStorage, tmp,
                    element.RequiredStorage);
                offsets.OffsetObjects = "offsets.OffsetObjects + {0} + ({1} * {2})".Expr(Decorator.OffsetObjects + 1,
                    tmp, element.RequiredObjects);
                offsets.OffsetProperties =
                    "offsets.OffsetProperties + {0} + ({1} * {2})".Expr(Decorator.OffsetProperties, tmp,
                        element.RequiredProperties);

                block.Stmts.For(tmp, tmp + " < " + Decorator.PropertyType.ElementCount,
                    body => { Create(element).EmitObjectSetup(new DomBlock(body, tmp + "_"), offsets); });
            }
            else
            {
                EmitInitObject(Decorator.ClrType, block, offsets, /* size */
                    Decorator.PropertyType.ElementCount.Literal(), /* stride */ element.RequiredStorage.Literal());
            }
        }

        public override void EmitMetaSetup(DomBlock block, Offsets offsets)
        {
            var tmp = block.TempVar();
            PropertyDecorator element = Decorator.ElementDecorator;
            element.Definition.Name += "[]";

            offsets.OffsetStorage = "{0} + ({1} * {2}) /*required-storage:{2}*/".Expr(Decorator.OffsetStorage, tmp,
                element.RequiredStorage);
            offsets.OffsetProperties = "{0} + ({1} * {2}) /*required-properties:{2}*/".Expr(Decorator.OffsetProperties,
                tmp, element.RequiredProperties);

            if (element.RequiredObjects == 0)
            {
                offsets.OffsetObjects = "0 /*required-objects:{0}*/".Expr(element.RequiredObjects);
            }
            else
            {
                offsets.OffsetObjects = "{0} + ({1} * {2}) /*required-objects:{2}*/".Expr(Decorator.OffsetObjects + 1,
                    tmp, element.RequiredObjects);
            }

            block.Stmts.For(tmp, tmp + " < " + Decorator.PropertyType.ElementCount,
                body => { Create(element).EmitMetaSetup(new DomBlock(body, tmp + "_"), offsets, tmp.Expr()); });
        }
    }
}