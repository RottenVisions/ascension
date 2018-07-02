using UnityEngine;

namespace Ascension.Networking
{
    public interface IPrefabPool
    {
        /// <summary>
        /// Called by Ascension to inspect a prefab before instantiating it. The object
        /// returned from this method can be the prefab itself, it does not have
        /// to be a unique instance.
        /// </summary>
        /// <param name="prefabId">The id of the prefab we are looking for</param>
        /// <returns>A game object representing the prefab or an instance of the prefab</returns>
        GameObject LoadPrefab(PrefabId prefabId);

        /// <summary>
        /// This is called when Ascension wants to create a new instance of an entity prefab.
        /// </summary>
        /// <param name="prefabId">The id of this prefab</param>
        /// <param name="position">The position we want the instance instantiated at</param>
        /// <param name="rotation">The rotation we want the instance to take</param>
        /// <returns>The newly instantiate object, or null if a prefab with <paramref name="prefabId"/> was not found</returns>
        GameObject Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation);

        /// <summary>
        /// This is called when Ascension wants to destroy the instance of an entity prefab.
        /// </summary>
        /// <param name="gameObject">The instance to destroy</param>
        void Destroy(GameObject gameObject);
    }

}