using System;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeObject : PropertyType
    {
        [ProtoMember(50)] public Guid StructGuid;

        public override bool Compilable
        {
            get { return StructGuid != Guid.Empty; }
        }

        public override bool HasPriority
        {
            get { return false; }
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
            return new PropertyDecoratorStruct();
        }
    }
}