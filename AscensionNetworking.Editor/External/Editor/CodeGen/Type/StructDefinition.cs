using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class ObjectDefinition : AssetDefinition
    {
        [ProtoMember(50)] public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

        public override IEnumerable<Type> AllowedPropertyTypes
        {
            get { return StateDefinition.AllowedStateAndStructPropertyTypes(); }
        }
    }
}