using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Ascension.Compiler
{
    public abstract class PropertyDecorator
    {
        public MemberAttributes Attributes = MemberAttributes.Public | MemberAttributes.Final;
        public AssetDecorator DefiningAsset;
        public PropertyDefinition Definition;
        public CodeGenerator Generator;
        public int OffsetObjects;
        public int OffsetProperties;
        public int OffsetStorage;
        public abstract string ClrType { get; }

        public virtual int RequiredObjects
        {
            get { return 0; }
        }

        public virtual int RequiredStorage
        {
            get { return 1; }
        }

        public virtual int RequiredProperties
        {
            get { return 1; }
        }

        public virtual string PropertyClassName
        {
            get { return "Ascension.Networking.NetworkProperty_" + GetType().Name.Replace("PropertyDecorator", ""); }
        }

        public abstract PropertyCodeEmitter CreateEmitter();

        public static List<PropertyDecorator> Decorate(IEnumerable<PropertyDefinition> definitions, AssetDecorator asset)
        {
            return definitions.Where(x => x.PropertyType.Compilable).Select(p => Decorate(p, asset)).ToList();
        }

        public static PropertyDecorator Decorate(PropertyDefinition definition, AssetDecorator asset)
        {
            PropertyDecorator decorator;

            decorator = definition.PropertyType.CreateDecorator();
            decorator.Generator = asset.Generator;
            decorator.Definition = definition;
            decorator.DefiningAsset = asset;

            return decorator;
        }
    }

    public abstract class PropertyDecorator<T> : PropertyDecorator where T : PropertyType
    {
        public T PropertyType
        {
            get { return (T) Definition.PropertyType; }
        }
    }
}