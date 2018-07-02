namespace Ascension.Networking.Physics
{
    /// <summary>
    ///     What type of shape to use in a Ascension hitbox
    /// </summary>
    /// *Example:* Sorting the hitboxes in a body based on shape.
    /// 
    /// ```csharp
    /// void ConfigureHitboxes(AscensionHitboxBody body) {
    /// foreach(AscensionHitbox hitbox in body.hitboxes) {
    /// switch(hitbox.hitboxShape) {
    /// case AscensionHitboxShape.Sphere: ConfigureSphere(hitbox); break;
    /// case AscensionHitboxShape.Box: ConfigureBox(hitbox); break;
    /// }
    /// }
    /// }
    /// ```
    /// </example>
    public enum AscensionHitboxShape
    {
        Box,
        Sphere
    }
}