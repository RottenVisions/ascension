using UnityEngine;
using UnityEditor;
using System.IO;

namespace Ascension.Tools
{
    public static class ScriptableObjectUtility
    {
        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(path + typeof (T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        public static void CreateAssetAtPath<T>(string path, string AssetName, T asset) where T : ScriptableObject
        {
            path = "Assets" + "/" + path;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + AssetName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
        }
    }
}
