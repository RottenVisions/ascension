namespace Ascension.Networking
{
    public struct PropertyFloatCompressionSettings
    {
        int _bits;

        float _pack;
        float _read;
        float _shift;

        public int BitsRequired
        {
            get { return _bits; }
        }

        public static PropertyFloatCompressionSettings Create()
        {
            PropertyFloatCompressionSettings f;

            f = new PropertyFloatCompressionSettings();
            f._bits = 32;

            return f;
        }

        public static PropertyFloatCompressionSettings Create(int bits, float shift, float pack, float read)
        {
            PropertyFloatCompressionSettings c;

            c._bits = bits;
            c._pack = pack;
            c._read = read;
            c._shift = shift;

            return c;
        }

        public void Pack(Packet stream, float value)
        {
            switch (_bits)
            {
                case 0:
                    break;

                case 32:
                    stream.WriteFloat(value);
                    break;

                default:
                    stream.WriteInt((int)((value + _shift) * _pack), _bits);
                    break;
            }
        }

        public float Read(Packet stream)
        {
            switch (_bits)
            {
                case 0:
                    return 0f;

                case 32:
                    return stream.ReadFloat();

                default:
                    return (stream.ReadInt(_bits) * _read) + -_shift;
            }
        }
    }
}
