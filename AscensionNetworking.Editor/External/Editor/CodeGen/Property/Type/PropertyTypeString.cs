using System;
using System.Text;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class PropertyTypeString : PropertyType
    {
        [ProtoMember(51)] public StringEncodings Encoding;
        [ProtoMember(50)] public int MaxLength = 1;

        public Encoding EncodingClass
        {
            get
            {
                switch (Encoding)
                {
                    case StringEncodings.ASCII:
                        return System.Text.Encoding.ASCII;
                    case StringEncodings.UTF8:
                        return System.Text.Encoding.UTF8;
                }

                throw new NotSupportedException(Encoding.ToString());
            }
        }

        public override PropertyDecorator CreateDecorator()
        {
            return new PropertyDecoratorString();
        }
    }
}