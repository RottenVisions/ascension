
namespace Ascension.Networking
{
    public struct PropertyExtrapolationSettings
    {
        public bool Enabled;
        public int MaxFrames;
        public float ErrorTolerance;
        public float SnapMagnitude;
        public ExtrapolationVelocityModes VelocityMode;

        public static PropertyExtrapolationSettings Create(int maxFrames, float errorTolerance, float snapMagnitude, ExtrapolationVelocityModes velocityMode)
        {
            PropertyExtrapolationSettings s;

            s.Enabled = true;
            s.MaxFrames = maxFrames;
            s.ErrorTolerance = errorTolerance;
            s.SnapMagnitude = snapMagnitude;
            s.VelocityMode = velocityMode;

            return s;
        }
    }
}
