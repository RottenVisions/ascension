using System;
using System.Collections.Generic;

namespace Ascension.Compiler
{
    public abstract class AssetDecorator
    {
        public int CountObjects;
        public int CountProperties;
        public int CountStorage;
        public AssetDefinition Definition;
        public CodeGenerator Generator;
        public uint TypeId;

        public virtual Guid Guid
        {
            get { return Definition.Guid; }
        }

        public virtual string Name
        {
            get { return Definition.Name; }
        }

        public virtual string NameMeta
        {
            get { return Definition.Name + "_Meta"; }
        }

        public virtual string BaseInterface
        {
            get { throw new NotSupportedException(); }
        }

        public virtual IEnumerable<string> ParentInterfaces
        {
            get { yield break; }
        }

        public virtual string BaseClass
        {
            get { return "Ascension.Networking.NetworkObj"; }
        }

        public virtual string BaseClassMeta
        {
            get { return BaseClass + "_Meta"; }
        }

        public virtual string NameInterface
        {
            get { return "I" + Name; }
        }

        public virtual bool EmitLegacyModifyMethod
        {
            get { return false; }
        }

        public virtual bool EmitAsInterface
        {
            get { return false; }
        }

        public virtual bool EmitPropertyChanged
        {
            get { return true; }
        }

        public abstract string FactoryInterface { get; }
        public abstract List<PropertyDecorator> Properties { get; set; }
    }

    public abstract class AssetDecorator<T> : AssetDecorator where T : AssetDefinition
    {
        public new T Definition
        {
            get { return (T) base.Definition; }
            set { base.Definition = value; }
        }
    }
}