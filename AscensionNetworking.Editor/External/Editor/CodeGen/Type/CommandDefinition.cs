using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class CommandDefinition : AssetDefinition
    {
        [ProtoMember(60)] public bool CompressZeroValues;
        [ProtoMember(50)] public List<PropertyDefinition> Input = new List<PropertyDefinition>();
        [ProtoIgnore] public List<PropertyDefinition> Properties = new List<PropertyDefinition>();
        [ProtoMember(51)] public List<PropertyDefinition> Result = new List<PropertyDefinition>();
        [ProtoMember(52)] public int SmoothFrames;

        public override IEnumerable<Type> AllowedPropertyTypes
        {
            get { return EventDefinition.AllowedEventAndCommandPropertyTypes(); }
        }
    }
}