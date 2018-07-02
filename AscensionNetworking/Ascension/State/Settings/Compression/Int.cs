
namespace Ascension.Networking
{
    public struct PropertyIntCompressionSettings
    {
        int _bits;
        int _shift;

        public int BitsRequired
        {
            get { return _bits; }
        }

        public static PropertyIntCompressionSettings Create()
        {
            PropertyIntCompressionSettings f;

            f = new PropertyIntCompressionSettings();
            f._bits = 32;

            return f;
        }

        public static PropertyIntCompressionSettings Create(int bits, int shift)
        {
            PropertyIntCompressionSettings f;

            f = new PropertyIntCompressionSettings();
            f._bits = 32;
            f._shift = shift;

            return f;
        }

        public void Pack(Packet stream, int value)
        {
            stream.WriteInt(value);
            //stream.WriteInt_Shifted(value, _shift);
        }

        public int Read(Packet stream)
        {
            return stream.ReadInt(); //+-_shift;
            //TODO: ADD to fix bitshift for compressing integers (udpkit removed, it would contain method to do this
            //See: http://stackoverflow.com/questions/35167/is-there-a-way-to-perform-a-circular-bit-shift-in-c
            //return stream.ReadInt_Shifted(_bits, _shift); //+-_shift;
        }
    }

}
