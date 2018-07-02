namespace Ascension.Networking.Physics
{
    /// <summary>
    ///     The body area represented by a Ascension hitbox
    /// </summary>
    /// <example>
    ///     *Example:* Modifying a base damage value depending on the area of the hit.
    ///     ```csharp
    ///     float CalculateDamage(AscensionHitbox hit, float baseDamage) {
    ///     switch(hit.hitboxType) {
    ///     case AscensionHitboxType.Head: return 2.0f * baseDamage;
    ///     case AscensionHitboxType.Leg:
    ///     case AscensionHitboxType.UpperArm: return 0.7f * baseDamage;
    ///     default: return baseDamage;
    ///     }
    ///     }
    ///     ```
    /// </example>
    public enum AscensionHitboxType
    {
        Unknown,
        Proximity,
        Body,
        Head,
        Throat,
        Shoulder,
        UpperArm,
        Forearm,
        Hand,
        Chest,
        Stomach,
        Pelvis,
        Buttocks,
        Thigh,
        Knee,
        Leg,
        Foot,
        Elbow,
        Ankle,
        Wrist,
        Finger,
        Heel
    }
}