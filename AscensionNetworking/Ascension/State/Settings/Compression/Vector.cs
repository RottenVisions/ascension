using UnityEngine;

namespace Ascension.Networking
{
    public struct PropertyVectorCompressionSettings
    {
        public bool StrictComparison;
        public PropertyFloatCompressionSettings X;
        public PropertyFloatCompressionSettings Y;
        public PropertyFloatCompressionSettings Z;

        public int BitsRequired
        {
            get { return X.BitsRequired + Y.BitsRequired + Z.BitsRequired; }
        }

        public static PropertyVectorCompressionSettings Create(
          PropertyFloatCompressionSettings x,
          PropertyFloatCompressionSettings y,
          PropertyFloatCompressionSettings z)
        {
            return Create(x, y, z, false);
        }

        public static PropertyVectorCompressionSettings Create(
          PropertyFloatCompressionSettings x,
          PropertyFloatCompressionSettings y,
          PropertyFloatCompressionSettings z,
          bool strict)
        {

            return new PropertyVectorCompressionSettings
            {
                X = x,
                Y = y,
                Z = z,
                StrictComparison = strict
            };
        }

        public void Pack(Packet stream, Vector3 value)
        {
            X.Pack(stream, value.x);
            Y.Pack(stream, value.y);
            Z.Pack(stream, value.z);
        }

        public Vector3 Read(Packet stream)
        {
            Vector3 v;

            v.x = X.Read(stream);
            v.y = Y.Read(stream);
            v.z = Z.Read(stream);

            return v;
        }

    }
}
