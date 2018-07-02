using System;
using ProtoBuf;

namespace Ascension.Compiler
{
    [ProtoContract]
    public class FloatCompression
    {
        [ProtoMember(5)] public float Accuracy;
        [ProtoMember(6)] public bool Enabled;
        [ProtoMember(2)] public int MaxValue;
        [ProtoMember(1)] public int MinValue;

        public float Pack
        {
            get { return 1f / Accuracy; }
        }

        public float Read
        {
            get { return Accuracy; }
        }

        public float Shift
        {
            get { return -MinValue; }
        }

        public int BitsRequired
        {
            get
            {
                float pack = 1f / Accuracy;
                int shift = -MinValue;
                return BitsForNumber((int) Math.Round((MaxValue + shift) * pack));
            }
        }

        public static FloatCompression Default()
        {
            return new FloatCompression
            {
                MinValue = -2048,
                MaxValue = +2048,
                Accuracy = 0.01f
            };
        }

        public static FloatCompression DefaultAngle()
        {
            return new FloatCompression
            {
                MinValue = 0,
                MaxValue = 360,
                Accuracy = 0.1f
            };
        }

        private static int BitsForNumber(int number)
        {
            if (number < 0)
            {
                return 32;
            }
            if (number == 0)
            {
                return 1;
            }

            for (int i = 31; i >= 0; --i)
            {
                int b = 1 << i;

                if ((number & b) == b)
                {
                    return i + 1;
                }
            }

            throw new Exception();
        }
    }
}