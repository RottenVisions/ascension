using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{
    /// <summary>
    /// Deault implementation of IPrefabPool which uses GameObject.Instantiate and GameObject.Destroy
    /// </summary>
    public class DefaultPrefabPool : IPrefabPool
    {
        GameObject IPrefabPool.Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation)
        {
            GameObject go;

            go =
                (GameObject) GameObject.Instantiate(((IPrefabPool) this).LoadPrefab(prefabId), position, rotation);
            go.GetComponent<AscensionEntity>().enabled = true;

            return go;
        }

        GameObject IPrefabPool.LoadPrefab(PrefabId prefabId)
        {
            return PrefabDatabase.Find(prefabId);
        }

        void IPrefabPool.Destroy(GameObject gameObject)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
