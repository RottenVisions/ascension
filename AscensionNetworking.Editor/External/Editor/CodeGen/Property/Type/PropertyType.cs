using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    [ProtoInclude(100, typeof (PropertyTypeFloat))]
    [ProtoInclude(200, typeof (PropertyTypeObject))]
    [ProtoInclude(300, typeof (PropertyTypeArray))]
    [ProtoInclude(400, typeof (PropertyTypeVector))]
    [ProtoInclude(500, typeof (PropertyTypeString))]
    [ProtoInclude(600, typeof (PropertyTypeTrigger))]
    [ProtoInclude(700, typeof (PropertyTypeTransform))]
    [ProtoInclude(800, typeof (PropertyTypeInteger))]
    [ProtoInclude(900, typeof (PropertyTypeEntity))]
    [ProtoInclude(1000, typeof (PropertyTypeBool))]
    [ProtoInclude(1100, typeof (PropertyTypeColor))]
    [ProtoInclude(1200, typeof (PropertyTypeQuaternion))]
    [ProtoInclude(1400, typeof (PropertyTypePrefabId))]
    [ProtoInclude(1500, typeof (PropertyTypeNetworkId))]
    [ProtoInclude(1600, typeof (PropertyTypeProcotolToken))]
    [ProtoInclude(1700, typeof (PropertyTypeGuid))]
    [ProtoInclude(1800, typeof (PropertyTypeMatrix4x4))]
    [ProtoInclude(1900, typeof (PropertyTypeColor32))]
    public abstract class PropertyType
    {
        [ProtoIgnore] public Project Context;

        [ProtoIgnore]
        public virtual IEnumerable<Type> Excludes
        {
            get { yield break; }
        }

        [ProtoIgnore]
        public virtual bool Compilable
        {
            get { return true; }
        }

        [ProtoIgnore]
        public virtual bool IsValue
        {
            get { return true; }
        }

        [ProtoIgnore]
        public virtual bool HasPriority
        {
            get { return true; }
        }

        [ProtoIgnore]
        public virtual bool CallbackAllowed
        {
            get { return true; }
        }

        [ProtoIgnore]
        public virtual bool InterpolateAllowed
        {
            get { return false; }
        }

        [ProtoIgnore]
        public virtual bool HasSettings
        {
            get { return true; }
        }

        [ProtoIgnore]
        public virtual bool MecanimApplicable
        {
            get { return false; }
        }

        [ProtoIgnore]
        public virtual bool CanSmoothCorrections
        {
            get { return false; }
        }

        [ProtoIgnore]
        public virtual bool StrictCompare
        {
            get { return false; }
        }

        public virtual void OnCreated()
        {
        }

        public virtual PropertyDecorator CreateDecorator()
        {
            throw new NotImplementedException();
        }
    }
}