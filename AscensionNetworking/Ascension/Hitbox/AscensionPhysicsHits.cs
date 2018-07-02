using System;
using System.Collections.Generic;
using Ascension.Networking;

namespace Ascension.Networking.Physics
{
    /// <summary>
    ///     Describes a hit to a AscensionHitbox on a AscensionHitboxBody
    /// </summary>
    /// <example>
    ///     *Example:* Logging the details of a AscensionPhysicsHit object.
    ///     ```csharp
    ///     void FireWeaponOwner(PlayerCommand cmd, AscensionEntity entity) {
    ///     if(entity.isOwner) {
    ///     using(AscensionPhysicsHits hits = AscensionNetwork.RaycastAll(new Ray(entity.transform.position,
    ///     cmd.Input.targetPos),
    ///     cmd.ServerFrame))0 {
    ///     if(hit.count > 0) {
    ///     AscensionPhysicsHit hit = hits.GetHit(0);
    ///     Debug.Log(string.Format("[HIT] Target={0}, Distance={1}, HitArea={2}", hit.body.gameObject.name, hit.distance,
    ///     hit.hitbox.hitboxType);
    ///     }
    ///     }
    ///     }
    ///     }
    ///     ```
    /// </example>
    public struct AscensionPhysicsHit
    {
        /// <summary>
        ///     The body which was hit
        /// </summary>
        public AscensionHitboxBody body;

        /// <summary>
        ///     The distance away from the origin of the ray
        /// </summary>
        public float distance;

        /// <summary>
        ///     Which hitbox was hit
        /// </summary>
        public AscensionHitbox hitbox;
    }

    /// <summary>
    ///     Container for a group of AscensionPhysicsHits
    /// </summary>
    /// <example>
    ///     *Example:* Using ```AscensionNetwork.RaycastAll()``` to detect hit events and processing the AscensionPhysicsHits
    ///     object that is returned.
    ///     ```csharp
    ///     void FireWeaponOwner(PlayerCommand cmd, AscensionEntity entity) {
    ///     if(entity.isOwner) {
    ///     using(AscensionPhysicsHits hits = AscensionNetwork.RaycastAll(new Ray(entity.transform.position,
    ///     cmd.Input.targetPos),
    ///     cmd.ServerFrame)) {
    ///     var hit = hits.GetHit(0);
    ///     var targetEntity = hit.body.GetComponent&ltAscensionEntity&gt();
    ///     if(targetEntity.StateIs&ltILivingEntity&gt()) {
    ///     targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage;
    ///     }
    ///     }
    ///     }
    ///     }
    ///     ```
    /// </example>
    public class AscensionPhysicsHits : NetObject, IDisposable
    {
        internal static readonly NetObjectPool<AscensionPhysicsHits> Pool = new NetObjectPool<AscensionPhysicsHits>();
        internal List<AscensionPhysicsHit> hits = new List<AscensionPhysicsHit>();

        /// <summary>
        ///     How many hits we have in the collection
        /// </summary>
        /// <example>
        ///     *Example:* Using the hit count to iterate through all hits
        ///     ```csharp
        ///     void OnOwner(PlayerCommand cmd, AscensionEntity entity) {
        ///     if(entity.isOwner) {
        ///     using(AscensionPhysicsHits hits = AscensionNetwork.RaycastAll(new Ray(entity.transform.position,
        ///     cmd.Input.targetPos),
        ///     cmd.ServerFrame)) {
        ///     for(int i = 0; i
        ///     < hits.count; ++ i) {
        ///         var hit= hits.GetHit( i);
        ///         var targetEntity= hit.body.GetComponent& ltAscensionEntity& gt();
        ///         if( targetEntity.StateIs& ltILivingEntity& gt()) {
        ///         targetEntity.GetState& ltILivingEntity& gt(). Modify(). HP -= activeWeapon.damage;
        ///     }
        ///     }
        ///     }
        ///     ```
        /// </example>
        public int Count
        {
            get { return hits.Count; }
        }

        /// <summary>
        ///     Array indexing of the hits in this object
        /// </summary>
        /// <param name="index">Index position</param>
        /// <returns>The AscensionPhysicsHit at the given index</returns>
        /// <example>
        ///     *Example:* Using the array indexing to get the first object hit by a weapon firing raycast.
        ///     ```csharp
        ///     void FireWeaponOwner(PlayerCommand cmd, AscensionEntity entity) {
        ///     if(entity.isOwner) {
        ///     using(AscensionPhysicsHits hits = AscensionNetwork.RaycastAll(new Ray(entity.transform.position,
        ///     cmd.Input.targetPos),
        ///     cmd.ServerFrame))0 {
        ///     if(hit.count > 0) {
        ///     var hit = hits[0];
        ///     var targetEntity = hit.body.GetComponent&ltAscensionEntity&gt();
        ///     if(targetEntity.StateIs&ltILivingEntity&gt()) {
        ///     targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage;
        ///     }
        ///     }
        ///     }
        ///     }
        ///     }
        ///     ```
        /// </example>
        public AscensionPhysicsHit this[int index]
        {
            get { return hits[index]; }
        }

        /// <summary>
        ///     Implementing the IDisposable interface to allow "using" syntax.
        /// </summary>
        /// <example>
        ///     *Example:* Implementing the Disponse() method allows AscensionPhysicsHits to be in a "using" block.
        ///     ```csharp
        ///     void DoRaycast(Ray ray) {
        ///     using(AscensionPhysicsHits hits = AscensionNetwork.RaycastAll(ray)) {
        ///     // the hits variable will be automatically disposed at the end of this block
        ///     }
        ///     }
        ///     ```
        /// </example>
        public void Dispose()
        {
            hits.Clear();
            Pool.Release(this);
        }

        /// <summary>
        ///     Get the hit at a specific index
        /// </summary>
        /// <param name="index">Index position</param>
        /// <returns>The AscensionPhysicsHit at the given index</returns>
        /// <example>
        ///     *Example:* Using the GetHit method to find the first object hit by a weapon firing raycast.
        ///     ```csharp
        ///     void FireWeaponOwner(PlayerCommand cmd, AscensionEntity entity) {
        ///     if(entity.isOwner) {
        ///     using(AscensionPhysicsHits hits = AscensionNetwork.RaycastAll(new Ray(entity.transform.position,
        ///     cmd.Input.targetPos),
        ///     cmd.ServerFrame))0 {
        ///     if(hit.count > 0) {
        ///     var hit = hits.GetHit(0);
        ///     var targetEntity = hit.body.GetComponent&ltAscensionEntity&gt();
        ///     if(targetEntity.StateIs&ltILivingEntity&gt()) {
        ///     targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage;
        ///     }
        ///     }
        ///     }
        ///     }
        ///     }
        ///     ```
        /// </example>
        public AscensionPhysicsHit GetHit(int index)
        {
            return hits[index];
        }

        internal void AddHit(AscensionHitboxBody body, AscensionHitbox hitbox, float distance)
        {
            hits.Add(new AscensionPhysicsHit {body = body, hitbox = hitbox, distance = distance});
        }
    }
}