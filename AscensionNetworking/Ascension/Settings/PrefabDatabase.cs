using System.Collections.Generic;
using Ascension.Networking.Sockets;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Ascension.Networking
{
    public enum PrefabDatabaseMode
    {
        AutomaticScan = 0,
        ManualScan = 1,
        Manual = 2
    }

    public class PrefabDatabase : ScriptableObject
    {
        static PrefabDatabase instance;
        static Dictionary<PrefabId, GameObject> lookup;

        public const string pathPrefix = "Assets/";
        public const string pathToAsset = "__ASCENSION__/Networking/Resources/User";
        public const string assetName = "PrefabDatabase";
        public const string assetExtension = ".asset";

        public static string FullAssetPathName { get { return pathToAsset + "/" + assetName; } }
        public static string FullAssetPathNameWithExt { get { return pathToAsset + "/" + assetName + assetExtension; } }

        public static PrefabDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    LoadAsset();

                    if (instance == null)
                    {
                        NetLog.Error("Could not find resource 'PrefabDatabase'");
                        instance = CreateInstance<PrefabDatabase>();
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        public PrefabDatabaseMode DatabaseMode = PrefabDatabaseMode.AutomaticScan;

        [SerializeField]
        public GameObject[] Prefabs = new GameObject[0];

        public static void BuildCache()
        {
            LoadInstance();
            UpdateLookup();
        }

        static void UpdateLookup()
        {
            lookup = new Dictionary<PrefabId, GameObject>();

            for (int i = 1; i < Instance.Prefabs.Length; ++i)
            {
                if (Instance.Prefabs[i])
                {
                    var prefabId = Instance.Prefabs[i].GetComponent<AscensionEntity>().PrefabId;

                    if (lookup.ContainsKey(prefabId))
                    {
                        throw new AscensionException("Duplicate {0} for {1} and {2}", prefabId, Instance.Prefabs[i].GetComponent<Entity>(), lookup[prefabId].GetComponent<Entity>());
                    }

                    lookup.Add(Instance.Prefabs[i].GetComponent<AscensionEntity>().PrefabId, Instance.Prefabs[i]);
                }
            }
        }

        static void LoadInstance()
        {
            instance = (PrefabDatabase)Resources.Load("User/" + assetName, typeof(PrefabDatabase));
        }

        public static GameObject Find(PrefabId id)
        {
            if (lookup == null || instance == null)
            {
                LoadInstance();
                UpdateLookup();
            }

            GameObject prefab;

            if (lookup.TryGetValue(id, out prefab))
            {
                return prefab;
            }
            else
            {
                NetLog.Error("Could not find game object for {0}", id);
                return null;
            }
        }

        public static bool Contains(AscensionEntity entity)
        {
            if (Instance.Prefabs == null)
                return false;

            if (!entity)
                return false;

            if (entity.prefabId >= Instance.Prefabs.Length)
                return false;

            if (entity.prefabId < 0)
                return false;

            return Instance.Prefabs[entity.prefabId] == entity.gameObject;
        }

        private static void Save()
        {
            if (instance == null)
                return;

#if UNITY_EDITOR
            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
#endif
        }

        public static void LoadAsset()
        {
#if UNITY_EDITOR
            instance = (PrefabDatabase)AssetDatabase.LoadAssetAtPath(pathPrefix + FullAssetPathNameWithExt, typeof(PrefabDatabase));
#else
            instance = Resources.Load<PrefabDatabase>("User/" + assetName);
#endif
        }
    }
}
