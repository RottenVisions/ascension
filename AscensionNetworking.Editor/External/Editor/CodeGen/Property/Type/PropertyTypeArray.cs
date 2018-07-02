using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeArray : PropertyType
    {
        [ProtoMember(51)] public int ElementCount;
        [ProtoMember(52)] public PropertyType ElementType;

        public IEnumerable<Type> AllowedElementTypes
        {
            get
            {
                yield return typeof (PropertyTypeFloat);
                yield return typeof (PropertyTypeObject);
                yield return typeof (PropertyTypeEntity);
                yield return typeof (PropertyTypeInteger);
                yield return typeof (PropertyTypeString);
                yield return typeof (PropertyTypeVector);
                yield return typeof (PropertyTypeQuaternion);
                yield return typeof (PropertyTypePrefabId);
                yield return typeof (PropertyTypeTransform);
                yield return typeof (PropertyTypeProcotolToken);
            }
        }

        public override bool Compilable
        {
            get
            {
                if (ElementType == null)
                {
                    return false;
                }

                if (ElementType is PropertyTypeObject)
                {
                    return ((PropertyTypeObject) ElementType).StructGuid != Guid.Empty;
                }

                return true;
            }
        }

        public override bool HasPriority
        {
            get { return ElementType != null && ElementType.GetType() != typeof (PropertyTypeObject); }
        }

        public override bool IsValue
        {
            get { return false; }
        }

        public override bool CallbackAllowed
        {
            get { return false; }
        }

        public override bool InterpolateAllowed
        {
            get { return false; }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorArray();
        }

        public override void OnCreated()
        {
            ElementCount = 8;
        }
    }
}