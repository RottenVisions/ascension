using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascension.Compiler
{
    public class StateDecorator : AssetDecorator<StateDefinition>
    {
        public StateDecorator(StateDefinition def)
        {
            Definition = def;
        }

        public bool HasParent
        {
            get { return Definition.ParentGuid != Guid.Empty; }
        }

        public override string FactoryInterface
        {
            get { return "Ascension.Networking.ISerializerFactory"; }
        }

        public override string BaseClass
        {
            get { return "Ascension.Networking.NetworkState"; }
        }

        public override bool EmitAsInterface
        {
            get { return true; }
        }

        public override bool EmitLegacyModifyMethod
        {
            get { return true; }
        }

        public override string BaseInterface
        {
            get
            {
                if (HasParent)
                {
                    return Parent.NameInterface;
                }

                return "Ascension.Networking.IState";
            }
        }

        public override IEnumerable<string> ParentInterfaces
        {
            get { return ParentList.Select(x => x.NameInterface); }
        }

        public override List<PropertyDecorator> Properties { get; set; }

        public StateDecorator Parent
        {
            get { return Generator.FindState(Definition.ParentGuid); }
        }

        public IEnumerable<StateDecorator> ParentList
        {
            get
            {
                if (HasParent)
                {
                    StateDecorator parent = Generator.FindState(Definition.ParentGuid);

                    foreach (StateDecorator def in parent.ParentList)
                    {
                        yield return def;
                    }

                    yield return parent;
                }
            }
        }
    }
}