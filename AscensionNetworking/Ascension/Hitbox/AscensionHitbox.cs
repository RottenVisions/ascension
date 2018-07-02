using System;
using UnityEngine;

namespace Ascension.Networking.Physics
{

    /// <summary>
    ///     Defines one hitbox on a AscensionHitboxBody
    /// </summary>
    /// <example>
    ///     *Example:* Resizing a sphere hitbox
    ///     ```csharp
    ///     void ResizeSphereHitbox(AscensionHitbox hitbox, float newRadius) {
    ///     if(hitbox.hitboxShape != AscensionHitboxShape.Sphere) {
    ///     Debug.Log("Attemping to resize a non-sphere hitbox");
    ///     return;
    ///     }
    ///     hitbox.hitboxSphereRadius = newRadius;
    ///     }
    ///     ```
    /// </example>
    public class AscensionHitbox : MonoBehaviour
    {
        [SerializeField] internal Vector3 boxSize = new Vector3(0.25f, 0.25f, 0.25f);
        [SerializeField] internal Vector3 center = Vector3.zero;
        [SerializeField] public AscensionHitboxShape shape = AscensionHitboxShape.Box;
        [SerializeField] internal float sphereRadius = 0.25f;
        [SerializeField] public AscensionHitboxType type = AscensionHitboxType.Unknown;

        /// <summary>
        ///     Shape of the hitbox (box or sphere)
        /// </summary>
        /// <example>
        ///     *Example:* Sorting the hitboxes in a body based on shape.
        ///     ```csharp
        ///     void ConfigureHitboxes(AscensionHitboxBody body) {
        ///     foreach(AscensionHitbox hitbox in body.hitboxes) {
        ///     switch(hitbox.hitboxShape) {
        ///     case AscensionHitboxShape.Sphere: ConfigureSphere(hitbox); break;
        ///     case AscensionHitboxShape.Box: ConfigureBox(hitbox); break;
        ///     }
        ///     }
        ///     }
        ///     ```
        /// </example>
        public AscensionHitboxShape HitboxShape
        {
            get { return shape; }
            set { shape = value; }
        }

        /// <summary>
        ///     Type of the hitbox
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
        public AscensionHitboxType HitboxType
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        ///     Center of the hitbox in local coordinates
        /// </summary>
        /// <example>
        ///     *Example:* Getting a vector that points from the player's weapon to the head of a target entity.
        ///     ```csharp
        ///     Vector3 GetHeadshotVector(AscensionEntity target, IWeapon currentWeapon) {
        ///     AscensionHitboxBody body = target.GetComponent&ltAscensionHitboxBody%gt();
        ///     AscensionHitbox head = body.hitboxes[0];
        ///     foreach(AscensionHitbox hitbox in body.hitboxes) {
        ///     if(hitbox.hitboxType == AscensionHitboxType.Head) {
        ///     head = hitbox;
        ///     }
        ///     }
        ///     return head.hitboxCenter - currentWeapon.fireOrigin;
        ///     }
        ///     ```
        /// </example>
        public Vector3 HitboxCenter
        {
            get { return center; }
            set { center = value; }
        }

        /// <summary>
        ///     Size of the hitbox if this shape is a box
        /// </summary>
        /// <example>
        ///     *Example:* A method to double the size of a player's head hitbox if it is a box.
        ///     ```csharp
        ///     void DoubleHeadSize(AscensionHitboxBody body) {
        ///     foreach(AscensionHitbox hitbox in body.hitboxes) {
        ///     if(hitbox.hitboxType == AscensionHitboxType.Head) {
        ///     hitbox.hitboxBoxSize = hitbox.hitboxBoxSize * 2f;
        ///     }
        ///     }
        ///     }
        ///     ```
        /// </example>
        public Vector3 HitboxBoxSize
        {
            get { return boxSize; }
            set { boxSize = value; }
        }

        /// <summary>
        ///     Type of the hitbox
        /// </summary>
        /// <example>
        ///     *Example:* A method to double the size of a player's head hitbox if it is a sphere.
        ///     ```csharp
        ///     void DoubleHeadSize(AscensionHitboxBody body) {
        ///     foreach(AscensionHitbox hitbox in body.hitboxes) {
        ///     if(hitbox.hitboxType == AscensionHitboxType.Head) {
        ///     hitbox.hitboxSphereRadius = hitbox.hitboxSphereRadius * 2f;
        ///     }
        ///     }
        ///     }
        ///     ```
        /// </example>
        public float HitboxSphereRadius
        {
            get { return sphereRadius; }
            set { sphereRadius = value; }
        }

        private void OnDrawGizmos()
        {
            Draw(transform.localToWorldMatrix);
        }

        internal void Draw(Matrix4x4 matrix)
        {
            Gizmos.color = new Color(255f / 255f, 128f / 255f, 39f / 255f);
            Gizmos.matrix = matrix;

            switch (shape)
            {
                case AscensionHitboxShape.Box:
                    Gizmos.DrawWireCube(center, boxSize);
                    break;

                case AscensionHitboxShape.Sphere:
                    Gizmos.DrawWireSphere(center, sphereRadius);
                    break;
            }

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.white;
        }

        internal bool OverlapSphere(ref Matrix4x4 matrix, Vector3 center, float radius)
        {
            center = matrix.MultiplyPoint(center);

            switch (shape)
            {
                case AscensionHitboxShape.Box:
                    return OverlapSphereOnBox(center, radius);

                case AscensionHitboxShape.Sphere:
                    return OverlapSphereOnSphere(center, radius);

                default:
                    return false;
            }
        }

        internal bool Raycast(ref Matrix4x4 matrix, Vector3 origin, Vector3 direction, out float distance)
        {
            origin = matrix.MultiplyPoint(origin);
            direction = matrix.MultiplyVector(direction);

            switch (shape)
            {
                case AscensionHitboxShape.Box:
                    Bounds b = new Bounds(center, boxSize);
                    return b.IntersectRay(new Ray(origin, direction), out distance);

                case AscensionHitboxShape.Sphere:
                    return RaycastSphere(origin, direction, out distance);

                default:
                    distance = 0f;
                    return false;
            }
        }

        private bool OverlapSphereOnSphere(Vector3 center, float radius)
        {
            return Vector3.Distance(this.center, center) <= sphereRadius + radius;
        }

        private bool OverlapSphereOnBox(Vector3 center, float radius)
        {
            Bounds b = new Bounds(this.center, boxSize);

            Vector3 clampedCenter;
            Vector3 min = b.min;
            Vector3 max = b.max;

            ClampVector(ref center, ref min, ref max, out clampedCenter);

            return Vector3.Distance(center, clampedCenter) <= radius;
        }

        private bool RaycastSphere(Vector3 o, Vector3 d, out float distance)
        {
            Vector3 v = o - center;
            float b = Vector3.Dot(v, d);
            float c = Vector3.Dot(v, v) - (sphereRadius * sphereRadius);

            if (c > 0f && b > 0f)
            {
                distance = 0f;
                return false;
            }

            float disc = b * b - c;

            if (disc < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = -b - (float) Math.Sqrt(disc);

            if (distance < 0f)
            {
                distance = 0f;
            }

            return true;
        }

        private static void ClampVector(ref Vector3 value, ref Vector3 min, ref Vector3 max, out Vector3 result)
        {
            float x = value.x;
            x = (x > max.x) ? max.x : x;
            x = (x < min.x) ? min.x : x;

            float y = value.y;
            y = (y > max.y) ? max.y : y;
            y = (y < min.y) ? min.y : y;

            float z = value.z;
            z = (z > max.z) ? max.z : z;
            z = (z < min.z) ? min.z : z;

            result = new Vector3(x, y, z);
        }
    }
}