namespace Ascension.Networking
{
    public struct SequenceGenerator
    {
        private uint mask;
        private uint sequence;

        public SequenceGenerator(int bits)
            : this(bits, 0u)
        {
        }

        public SequenceGenerator(int bits, uint start)
        {
            mask = (1u << bits) - 1u;
            sequence = start & mask;
        }

        public uint Next()
        {
            sequence += 1u;
            sequence &= mask;
            return sequence;
        }
    }
}