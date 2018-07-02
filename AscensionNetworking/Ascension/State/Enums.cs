namespace Ascension.Networking
{
    public enum AxisSelections
    {
        XYZ = X | Y | Z,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        X = 1 << 1,
        Y = 1 << 2,
        Z = 1 << 3,
        Disabled = 0,
    }

    public enum ReplicationMode
    {
        EveryoneExceptController = 0,
        Everyone = 1,
        OnlyOwnerAndController = 2,
        LocalForEachPlayer = 3
    }

    public enum ExtrapolationVelocityModes
    {
        CalculateFromPosition = 0,
        CopyFromRigidbody = 1,
        CopyFromRigidbody2D = 2,
        CopyFromCharacterController = 3
    }

    public enum SmoothingAlgorithms
    {
        None = 0,
        Interpolation = 1,
        Extrapolation = 2,
    }

    public enum TransformSpaces
    {
        Local = 0,
        World = 1,
    }

    public enum TransformRotationMode
    {
        QuaternionComponents = 0,
        EulerAngles = 1,
    }

    public enum MecanimMode
    {
        Disabled,
        Parameter,
        LayerWeight
    }


    public enum MecanimDirection
    {
        UsingAnimatorMethods,
        UsingAscensionProperties
    }

    public enum StringEncodings
    {
        ASCII = 0,
        UTF8 = 1
    }
}
