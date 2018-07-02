using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ascension.Networking;
using Ascension.Networking.Sockets;
using UnityEditor;
using UnityEngine;

partial class AscensionCompiler
{
    private static IEnumerable<AscensionPrefab> FindPrefabs()
    {
        int id = 1;
        string[] files = Directory.GetFiles(@"Assets", "*.prefab", SearchOption.AllDirectories);
        RuntimeSettings settings = RuntimeSettings.Instance;

        for (int i = 0; i < files.Length; ++i)
        {
            AscensionEntity entity =
                AssetDatabase.LoadAssetAtPath(files[i], typeof (AscensionEntity)) as AscensionEntity;

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
                    new AscensionPrefab
                    {
                        go = entity.gameObject,
                        id = id,
                        name = entity.gameObject.name.CSharpIdentifier()
                    };

                id += 1;
            }

            EditorUtility.DisplayProgressBar("Updating Ascension Prefab Database", "Scanning for prefabs ...",
                Mathf.Clamp01(i / (float) files.Length));
        }
    }

    public static void UpdatePrefabsDatabase()
    {
        try
        {
            // get all prefabs
            IEnumerable<AscensionPrefab> prefabs = FindPrefabs();

            // create new array
            PrefabDatabase.Instance.Prefabs = new GameObject[prefabs.Count() + 1];

            // update array
            foreach (AscensionPrefab prefab in prefabs)
            {
                if (PrefabDatabase.Instance.Prefabs[prefab.id])
                {
                    throw new AscensionException("Duplicate Prefab ID {0}", prefab.id);
                }

                // assign prefab
                PrefabDatabase.Instance.Prefabs[prefab.id] = prefab.go;

                // log this to the user
                Debug.Log(string.Format("Assigned {0} to '{1}'", new PrefabId(prefab.id),
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

    private static void CompilePrefabs(AscensionCompilerOperation op)
    {
        if (PrefabDatabase.Instance.DatabaseMode == PrefabDatabaseMode.AutomaticScan)
        {
            UpdatePrefabsDatabase();
        }

        for (int i = 1; i < PrefabDatabase.Instance.Prefabs.Length; ++i)
        {
            if (PrefabDatabase.Instance.Prefabs[i])
            {
                GameObject go = PrefabDatabase.Instance.Prefabs[i];
                AscensionEntity entity = go.GetComponent<AscensionEntity>();

                if (entity && entity.SceneGuid != UniqueId.None)
                {
                    entity.sceneGuid = "";

                    EditorUtility.SetDirty(go);
                    EditorUtility.SetDirty(entity);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        using (AscensionSourceFile file = new AscensionSourceFile(op.prefabsFilePath))
        {
            file.EmitScope("public static class AscensionPrefabs", () =>
            {
                for (int i = 1; i < PrefabDatabase.Instance.Prefabs.Length; ++i)
                {
                    GameObject prefab = PrefabDatabase.Instance.Prefabs[i];

                    if (prefab)
                    {
                        file.EmitLine(
                            "public static readonly Ascension.Networking.PrefabId {0} = new Ascension.Networking.PrefabId({1});",
                            prefab.name.CSharpIdentifier(), prefab.GetComponent<AscensionEntity>().prefabId);
                    }
                }
            });
        }
    }

    private struct AscensionPrefab
    {
        public GameObject go;
        public int id;
        public string name;
    }
}