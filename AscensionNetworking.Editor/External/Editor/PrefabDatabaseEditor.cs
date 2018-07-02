using System.Collections.Generic;
using System.IO;
using Ascension.Networking;
using UnityEditor;
using UnityEngine;
using DEditorGUI = Ascension.Tools.EditorGUI;

[CustomEditor(typeof(PrefabDatabase))]
public class AscensionPrefabDatabaseEditor : Editor
{

    [MenuItem("Ascension/Prefabs", priority = 23)]
    static void OpenPrefabDatabaseEditor()
    {
        Selection.activeObject = PrefabDatabase.Instance;
    }

    void OverlayIcon(string icon, int xOffset)
    {
        Rect r = GUILayoutUtility.GetLastRect();
        r.xMin = (r.xMax - 19) + xOffset;
        r.xMax = (r.xMax - 3) + xOffset;
        r.yMin = r.yMin;
        r.yMax = r.yMax + 1;

        GUI.color = DEditorGUI.HighlightColor;
        GUI.DrawTexture(r, DEditorGUI.LoadIcon(icon));
        GUI.color = Color.white;
    }

    void Save()
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }

    public override void OnInspectorGUI()
    {

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        GUILayout.Space(2);
        GUI.DrawTexture(GUILayoutUtility.GetRect(128, 128, 64, 64, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)), Resources.Load("AscensionLogo") as Texture2D);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        PrefabDatabase db = (PrefabDatabase)target;

        if (db.DatabaseMode == PrefabDatabaseMode.Manual)
        {
            EditorGUILayout.BeginVertical();

            for (int i = 1; i < db.Prefabs.Length; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(" ", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    // remove prefab
                    ArrayUtility.RemoveAt(ref db.Prefabs, i);

                    // save
                    Save();

                    // decrement index
                    --i;

                    continue;
                }

                OverlayIcon("minus", +1);

                db.Prefabs[i] = (GameObject)EditorGUILayout.ObjectField(db.Prefabs[i], typeof(GameObject), false);

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(6);

            if (GUILayout.Button("Add Prefab Slot", EditorStyles.miniButton))
            {
                System.Array.Resize(ref db.Prefabs, db.Prefabs.Length + 1);
                Save();
            }

            HashSet<int> set = new HashSet<int>();

            for (int i = 1; i < db.Prefabs.Length; ++i)
            {
                if (db.Prefabs[i])
                {
                    if (set.Contains(db.Prefabs[i].GetInstanceID()))
                    {
                        // tell the user we did this
                        Debug.LogError(string.Format("Removed Duplicate Prefab: {0}", db.Prefabs[i].name));

                        // clear it out
                        db.Prefabs[i] = null;

                        // save
                        Save();
                    }
                    else
                    {
                        set.Add(db.Prefabs[i].GetInstanceID());
                    }
                }
            }

            if (GUI.changed)
            {
                Save();
            }

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.BeginVertical();

            for (int i = 1; i < db.Prefabs.Length; ++i)
            {
                if (db.Prefabs[i])
                {
                    GUIStyle style;

                    style = new GUIStyle(EditorStyles.miniButton);
                    style.alignment = TextAnchor.MiddleLeft;

                    string id = db.Prefabs[i].GetComponent<AscensionEntity>().prefabId.ToString();
                    string path = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(db.Prefabs[i]));

                    if (GUILayout.Button(id + " " + path, style))
                    {
                        Selection.activeGameObject = db.Prefabs[i];
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

    }
}
