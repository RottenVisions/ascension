using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class EventDefinition : AssetDefinition
    {
        [ProtoMember(54)] private int _globalTargets;
        [ProtoMember(53)] public EntityEventSenders EntitySenders;
        [ProtoMember(52)] public EntityEventTargets EntityTargets;
        [ProtoMember(57)] public int Filters;
        [ProtoMember(56)] public bool Global;
        [ProtoMember(55)] public GlobalEventSenders GlobalSenders;
        [ProtoMember(58)] public int Priority;
        [ProtoMember(50)] public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

        [ProtoIgnore]
        public GlobalEventTargets GlobalTargets
        {
            get { return (GlobalEventTargets) _globalTargets; }
            set { _globalTargets = (int) value; }
        }

        public bool Entity
        {
            get { return !Global; }
        }

        public override IEnumerable<Type> AllowedPropertyTypes
        {
            get { return AllowedEventAndCommandPropertyTypes(); }
        }

        public static IEnumerable<Type> AllowedEventAndCommandPropertyTypes()
        {
            yield return typeof (PropertyTypeEntity);
            yield return typeof (PropertyTypeFloat);
            yield return typeof (PropertyTypeBool);
            yield return typeof (PropertyTypeInteger);
            yield return typeof (PropertyTypeString);
            yield return typeof (PropertyTypeVector);
            yield return typeof (PropertyTypeQuaternion);
            yield return typeof (PropertyTypeColor);
            yield return typeof (PropertyTypeColor32);
            yield return typeof (PropertyTypePrefabId);
            yield return typeof (PropertyTypeNetworkId);
            yield return typeof (PropertyTypeGuid);
            yield return typeof (PropertyTypeMatrix4x4);
            yield return typeof (PropertyTypeProcotolToken);
        }
    }
}