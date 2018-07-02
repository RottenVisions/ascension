namespace Ascension.Networking
{
    public struct PropertyTransformCompressionSettings
    {
        public PropertyVectorCompressionSettings Position;
        public PropertyQuaternionCompression Rotation;

        public static PropertyTransformCompressionSettings Create(PropertyVectorCompressionSettings position, PropertyQuaternionCompression rotation)
        {
            return new PropertyTransformCompressionSettings
            {
                Position = position,
                Rotation = rotation,
            };
        }
    }
}
