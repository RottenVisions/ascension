using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class StateDefinition : AssetDefinition
    {
        [ProtoMember(53)] public bool IsAbstract;
        [ProtoMember(54)] public int PacketMaxBits = 512;
        [ProtoMember(55)] public int PacketMaxProperties = 16;
        [ProtoMember(52)] public Guid ParentGuid;
        [ProtoMember(50)] public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

        public override IEnumerable<Type> AllowedPropertyTypes
        {
            get { return AllowedStateAndStructPropertyTypes(); }
        }

        public static StateDefinition Default()
        {
            StateDefinition s;

            s = new StateDefinition();
            s.Guid = Guid.NewGuid();
            s.Enabled = true;

            return s;
        }

        public static IEnumerable<Type> AllowedStateAndStructPropertyTypes()
        {
            yield return typeof (PropertyTypeFloat);
            yield return typeof (PropertyTypeInteger);
            yield return typeof (PropertyTypeArray);
            yield return typeof (PropertyTypeObject);
            yield return typeof (PropertyTypeString);
            yield return typeof (PropertyTypeTrigger);
            yield return typeof (PropertyTypeTransform);
            yield return typeof (PropertyTypeBool);
            yield return typeof (PropertyTypeEntity);
            yield return typeof (PropertyTypeVector);
            yield return typeof (PropertyTypeQuaternion);
            yield return typeof (PropertyTypeColor);
            yield return typeof (PropertyTypeColor32);
            yield return typeof (PropertyTypePrefabId);
            yield return typeof (PropertyTypeNetworkId);
            yield return typeof (PropertyTypeProcotolToken);
            yield return typeof (PropertyTypeGuid);
            yield return typeof (PropertyTypeMatrix4x4);
        }
    }
}