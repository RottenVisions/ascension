using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ascension.Networking.Sockets;
using Ascension.Tools;
using UnityEditor;
using UnityEngine;

namespace Ascension.Networking
{

    public partial class PrefabCompiler
    { 
        private struct NetworkPrefab
        {
            public int id;
            public string name;
            public GameObject go;
        }

        private static IEnumerable<NetworkPrefab> FindPrefabs()
        {
            var id = 1;
            var files = Directory.GetFiles(@"Assets", "*.prefab", SearchOption.AllDirectories);
            var settings = RuntimeSettings.Instance;

            for (int i = 0; i < files.Length; ++i)
            {
                AscensionEntity entity = AssetDatabase.LoadAssetAtPath(files[i], typeof (AscensionEntity)) as AscensionEntity;

                if (entity)
                {
                    entity.prefabId = id;
                    entity.sceneGuid = null;

                    if (settings.clientCanInstantiateAll)
                    {
                        entity.allowInstantiateOnClient = true;
                    }

                    EditorUtility.SetDirty(entity.gameObject);
                    EditorUtility.SetDirty(entity);

                    yield return
                        new NetworkPrefab
                        {
                            go = entity.gameObject,
                            id = id,
                            name = entity.gameObject.name.CSharpIdentifier()
                        };

                    id += 1;
                }

                EditorUtility.DisplayProgressBar("Updating Ascension Prefab Database", "Scanning for prefabs ...",
                    Mathf.Clamp01((float) i / (float) files.Length));
            }
        }

        public static void UpdatePrefabsDatabase()
        {
            try
            {
                // get all prefabs
                IEnumerable<NetworkPrefab> prefabs = FindPrefabs();

                // create new array
                PrefabDatabase.Instance.Prefabs = new GameObject[prefabs.Count() + 1];

                // update array
                foreach (NetworkPrefab prefab in prefabs)
                {
                    if (PrefabDatabase.Instance.Prefabs[prefab.id])
                    {
                        throw new AscensionException("Duplicate Prefab ID {0}", prefab.id);
                    }

                    // assign prefab
                    PrefabDatabase.Instance.Prefabs[prefab.id] = prefab.go;

                    // log this to the user
                    Debug.Log(String.Format("Assigned {0} to '{1}'", new PrefabId(prefab.id),
                        AssetDatabase.GetAssetPath(prefab.go)));
                }

                // save it!
                EditorUtility.SetDirty(PrefabDatabase.Instance);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void CompilePrefabs()
        {
            FirstSave();

            if (PrefabDatabase.Instance.DatabaseMode == PrefabDatabaseMode.AutomaticScan)
            {
                UpdatePrefabsDatabase();
            }

            for (int i = 1; i < PrefabDatabase.Instance.Prefabs.Length; ++i)
            {
                if (PrefabDatabase.Instance.Prefabs[i])
                {
                    var go = PrefabDatabase.Instance.Prefabs[i];
                    var entity = go.GetComponent<AscensionEntity>();

                    if (entity && entity.SceneGuid != UniqueId.None)
                    {
                        entity.sceneGuid = "";

                        EditorUtility.SetDirty(go);
                        EditorUtility.SetDirty(entity);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the asset exists or not, if it doesn't create it
        /// </summary>
        public static void FirstSave()
        {
            if (!File.Exists("Assets/" + PrefabDatabase.FullAssetPathNameWithExt))
            {
                ScriptableObjectUtility.CreateAssetAtPath(PrefabDatabase.pathToAsset, PrefabDatabase.assetName, PrefabDatabase.Instance);
            }
        }
    }
}

