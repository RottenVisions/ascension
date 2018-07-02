using Ascension.Networking;
using UnityEngine;

namespace Ascension.Networking.Physics
{
    /// <summary>
    ///     Defines a body of hitboxes to be tracked
    /// </summary>
    /// <example>
    ///     *Example:* Adding a hitbox body to a character pre-configured with AscensionHitbox components
    ///     ```csharp
    ///     void AddHitboxBody(AscensionEntity entity) {
    ///     AscensionHitbox[] hitboxes = entity.GetComponentsInChildren&ltAscensionHitbox&gt();
    ///     AscensionHitboxBody body = entity.AddComponent&ltAscensionHitboxBody&gt();
    ///     body.hitboxes = hitboxes;
    ///     }
    ///     ```
    /// </example>
    public class AscensionHitboxBody : MonoBehaviour, IListNode
    {
        [SerializeField] internal AscensionHitbox[] hitboxes = new AscensionHitbox[0];
        [SerializeField] internal AscensionHitbox proximity;

        /// <summary>
        ///     A hitbox which should contain all other hitboxes on this entity
        /// </summary>
        public AscensionHitbox Proximity
        {
            get { return proximity; }
            set { proximity = value; }
        }

        /// <summary>
        ///     An array of hitbox components that compose this body
        /// </summary>
        /// <example>
        ///     *Example:* Finding all hitbox components on an entity and adding them to a hitbox body
        ///     ```csharp
        ///     void AddHitboxBody(AscensionEntity entity) {
        ///     AscensionHitbox[] hitboxes = entity.gameObject.GetComponentsInChildren&ltAscensionHitbox&gt();
        ///     AscensionHitboxBody body = entity.AddComponent&ltAscensionHitboxBody&gt();
        ///     body.hitboxes = hitboxes;
        ///     }
        ///     ```
        /// </example>
        public AscensionHitbox[] Hitboxes
        {
            get { return hitboxes; }
            set { hitboxes = value; }
        }

        object IListNode.Prev { get; set; }
        object IListNode.Next { get; set; }
        object IListNode.List { get; set; }

        private void OnEnable()
        {
            AscensionPhysics.RegisterBody(this);
        }

        private void OnDisable()
        {
            AscensionPhysics.UnregisterBody(this);
        }
    }
}