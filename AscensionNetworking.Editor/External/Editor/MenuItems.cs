using UnityEngine;
using System.Collections;
using System.IO;
using Ascension.Networking.Sockets;
using Ascension.Tools;
using UnityEditor;
using DEditorGUI = Ascension.Tools.EditorGUI;

namespace Ascension.Networking
{
    public static class MenuItems
    {
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<RuntimeSettings>();
        }

        public static void CreateAssetAtPath()
        {
            ScriptableObjectUtility.CreateAssetAtPath(AssetDatabase.GetAssetPath(RuntimeSettings.Instance), RuntimeSettings.assetName, RuntimeSettings.Instance);
        }

        public static void CreateAssetAtPath(string path)
        {
            ScriptableObjectUtility.CreateAssetAtPath(path, RuntimeSettings.assetName, RuntimeSettings.Instance);
        }

        [MenuItem("Ascension/Emit Code", priority = 1)]
        public static void RunCodeEmitter()
        {
            if (RuntimeSettings.Instance.compileAsDll)
                AscensionDataAssemblyCompiler.Run();
            else
                AscensionDataAssemblyCompiler.CodeEmit();
        }

        //[MenuItem("Ascension/Compile Assembly", priority = 1)]
        //public static void RunCompiler()
        //{
        //    AscensionDataAssemblyCompiler.Run();
        //}

        [MenuItem("Ascension/Network Settings")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            RuntimeSettingsEditor window = (RuntimeSettingsEditor)EditorWindow.GetWindow(typeof(RuntimeSettingsEditor));
            window.titleContent = new GUIContent(RuntimeSettings.windowTitle);
            window.Show();

            if (!File.Exists("Assets/" + RuntimeSettings.FullAssetPathNameWithExt))
            {
                CreateAssetAtPath(RuntimeSettings.pathToAsset);
            }
            else
            {
                RuntimeSettings.LoadAsset();
            }
        }

        [MenuItem("Ascension/Update Prefab Database")]
        public static void UpdatePrefabDatabase()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isUpdating)
            {
                Debug.LogError("Can't generate prefab database while the editor is playing, paused, updating assets or compiling");
                return;
            }

            PrefabCompiler.CompilePrefabs();
            AscensionCompiler.UpdatePrefabsDatabase();
        }

        [MenuItem("Ascension/Generate Scene Object Ids")]
        public static void GenerateSceneObjectGuids()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isUpdating)
            {
                Debug.LogError(
                    "Can't generate scene guids while the editor is playing, paused, updating assets or compiling");
                return;
            }

            foreach (AscensionEntity en in GameObject.FindObjectsOfType<AscensionEntity>())
            {
                en.ModifySettings().SceneId = UniqueId.New();
                EditorUtility.SetDirty(en);
                EditorUtility.SetDirty(en.gameObject);
                Debug.Log(string.Format("Assigned new scene id to {0}", en));
            }

            // save scene
            EditorSaver.AskToSaveSceneAt(System.DateTime.Now.AddSeconds(1));
        }

        [MenuItem("Ascension/Remotes", priority = 22)]
        public static void OpenInfoPanel()
        {
            AscensionConnectionsWindow window = EditorWindow.GetWindow<AscensionConnectionsWindow>();
            window.title = "Remotes";
            window.name = "Remotes";
            window.Show();
        }
    }

}
