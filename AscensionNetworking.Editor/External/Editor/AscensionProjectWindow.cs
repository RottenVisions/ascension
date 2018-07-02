using Ascension.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ascension.Networking;
using Ascension.Networking.Sockets;
using UnityEditor;
using UnityEngine;
using DEditorGUI = Ascension.Tools.EditorGUI;
using Event = UnityEngine.Event;

public class AscensionProjectWindow : AscensionWindow
{
    [MenuItem("Ascension/Assets", priority = -100)]
    public static void Open()
    {
        AscensionProjectWindow w;

        w = EditorWindow.GetWindow<AscensionProjectWindow>();
        w.titleContent = new GUIContent("Assets");
        w.name = "Assets";
        w.minSize = new Vector2(150, 200);
        w.Show();
    }

    Vector2 scroll;
    string addGroup = null;
    AssetDefinition addGroupTo = null;

    [SerializeField]
    string selectedAssetGuid;

    static string version = "5.0.0";

    bool HasGroupSelected
    {
        get { return !string.IsNullOrEmpty(Project.ActiveGroup) && Project.ActiveGroup != "Everything"; }
    }

    void NewAsset(AssetDefinition def)
    {
        def.Guid = Guid.NewGuid();
        def.Name = "New" + def.GetType().Name.Replace("Definition", "");

        if (HasGroupSelected)
        {
            def.Groups.Add(Project.ActiveGroup);
        }

        // add to parent
        ArrayUtility.Add(ref Project.RootFolder.Assets, def);

        // select it
        Select(def, true);

        // save project
        Save();
    }

    new void Update()
    {
        base.Update();

        if (HasProject)
        {
            if (string.IsNullOrEmpty(selectedAssetGuid) == false && Selected == null)
            {
                try
                {
                    Select(Project.RootFolder.Assets.First(x => x.Guid == new Guid(selectedAssetGuid)), false);
                }
                catch
                {
                    selectedAssetGuid = null;
                }
            }
        }
    }

    new void OnGUI()
    {
        base.OnGUI();

        GUILayout.BeginArea(new Rect(0, 0, position.width, position.height - 22));
        scroll = GUILayout.BeginScrollView(scroll, false, false);

        EditorGUILayout.BeginHorizontal();

        var addingGroup = addGroup != null && addGroupTo != null;

        if (addingGroup)
        {
            GUI.SetNextControlName("AscensionProjectWindow_AddGroup");
            addGroup = GUILayout.TextField(addGroup);
            GUI.FocusControl("AscensionProjectWindow_AddGroup");

            switch (Event.current.keyCode.ToString())
            {
                case "Return":
                    addGroup = addGroup.Trim();

                    if (addGroup.Length > 0)
                    {
                        addGroupTo.Groups.Add(addGroup);
                    }

                    addGroup = null;
                    addGroupTo = null;
                    break;

                case "Escape":
                    addGroup = null;
                    addGroupTo = null;
                    break;
            }
        }
        else
        {
            EditorGUI.BeginDisabledGroup(Project.Groups.Count() == 0);

            var list = new[] { "Everything" }.Concat(Project.Groups).ToArray();
            var listCounted = new[] { "Everything (" + Project.RootFolder.Assets.Length + ")" }.Concat(Project.Groups.Select(x => x + " (" + Project.RootFolder.Assets.Count(a => a.Groups.Contains(x)) + ")")).ToArray();

            var index = Mathf.Max(0, Array.IndexOf(list, Project.ActiveGroup));
            var selected = EditorGUILayout.Popup(index, listCounted);

            if (Project.ActiveGroup != list[selected])
            {
                Project.ActiveGroup = list[selected];
                Save();
            }

            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndHorizontal();

        if (HasProject)
        {
            Header("States", "states");
            DisplayAssetList(Project.States.Cast<AssetDefinition>());

            Header("Objects", "objects");
            DisplayAssetList(Project.Structs.Cast<AssetDefinition>());

            Header("Commands", "commands");
            DisplayAssetList(Project.Commands.Cast<AssetDefinition>());

            Header("Events", "events");
            DisplayAssetList(Project.Events.Cast<AssetDefinition>());

            if (DEditorGUI.IsRightClick)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New State"), false, () => NewAsset(new StateDefinition()));
                menu.AddItem(new GUIContent("New Object"), false, () => NewAsset(new ObjectDefinition()));
                menu.AddItem(new GUIContent("New Event"), false, () => NewAsset(new EventDefinition()));
                menu.AddItem(new GUIContent("New Command"), false, () => NewAsset(new CommandDefinition()));
                menu.ShowAsContext();
            }
        }

        if (GUI.changed)
        {
            Save();
        }

        ClearAllFocus();

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(4, position.height - 20, position.width - 8, 16));
        Footer();
        GUILayout.EndArea();
    }

    void Header(string text, string icon)
    {
        DEditorGUI.Header(text, icon);
    }

    void Footer()
    {
        GUILayout.BeginHorizontal();

        //var version = Assembly.GetExecutingAssembly().GetName().Version;
        
        GUILayout.Label(string.Format("{0} ({1})", version, Core.IsDebugMode ? "DEBUG" : "RELEASE"), EditorStyles.miniLabel);

        GUILayout.FlexibleSpace();

        PrefabDatabase db = PrefabDatabase.Instance;

        if (db.DatabaseMode == PrefabDatabaseMode.ManualScan)
        {
            if (DEditorGUI.IconButton("refresh"))
            {
                AscensionCompiler.UpdatePrefabsDatabase();
                Debug.Log("Upading prefab database...");
            }

            GUILayout.Space(8);
        }

        if (DEditorGUI.IconButton("code-emit"))
        {
            if(RuntimeSettings.Instance.compileAsDll)
                AscensionDataAssemblyCompiler.Run();
            else
                AscensionDataAssemblyCompiler.CodeEmit();

            Debug.Log("Compiling project... " + ProjectPath);
        }

        GUILayout.Space(8);

        if (DEditorGUI.IconButton("save-project"))
        {
            Save();
            Debug.Log("Saving project... " + ProjectPath);
        }

        GUILayout.EndHorizontal();
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

    void DisplayAssetList(IEnumerable<AssetDefinition> assets)
    {
        bool deleteMode = (Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control;

        foreach (var a in assets.OrderBy(x => x.Name))
        {

            if (a.Groups.Contains(""))
            {
                a.Groups = new HashSet<string>(a.Groups.Where(x => x != ""));

                // save
                Save();
            }

            // check
            if (HasGroupSelected && !a.Groups.Contains(Project.ActiveGroup))
            {
                continue;
            }

            GUILayout.BeginHorizontal();

            GUIStyle style;
            style = new GUIStyle(EditorStyles.miniButtonLeft);
            style.alignment = TextAnchor.MiddleLeft;

            if (IsSelected(a))
            {
                style.normal.textColor = DEditorGUI.HighlightColor;
            }

            if (GUILayout.Button(new GUIContent(a.Name), style))
            {
                Select(a, true);
            }

            if (GUILayout.Button(" ", EditorStyles.miniButtonRight, GUILayout.Width(20)))
            {
                if (deleteMode)
                {
                    if (EditorUtility.DisplayDialog("Delete Asset", string.Format("Do you want to delete {0} ({1})?", a.Name, a.GetType().Name.Replace("Definition", "")), "Yes", "No"))
                    {
                        a.Deleted = true;

                        if (IsSelected(a))
                        {
                            Select(null, false);
                        }

                        Save();
                    }
                }
                else
                {
                    OpenFilterMenu(a);
                }
            }

            if (deleteMode)
            {
                OverlayIcon("delete", +1);
            }
            else
            {
                OverlayIcon("group-small", 0);
            }

            GUILayout.EndHorizontal();
        }


        for (int i = 0; i < Project.RootFolder.Assets.Length; ++i)
        {
            if (Project.RootFolder.Assets[i].Deleted)
            {
                // remove deleted assets
                ArrayUtility.RemoveAt(ref Project.RootFolder.Assets, i);

                // decrement index
                i -= 1;

                // save project
                Save();
            }
        }
    }

    void OpenFilterMenu(AssetDefinition asset)
    {
        GenericMenu menu = new GenericMenu();

        foreach (string group in Project.Groups)
        {
            menu.AddItem(new GUIContent(group), asset.Groups.Contains(group), userData =>
            {
                NetTuple<AssetDefinition, string> pair = (NetTuple<AssetDefinition, string>)userData;

                if (pair.item0.Groups.Contains(pair.item1))
                {
                    pair.item0.Groups.Remove(pair.item1);
                }
                else
                {
                    pair.item0.Groups.Add(pair.item1);
                }

                Save();
            }, new NetTuple<AssetDefinition, string>(asset, group));
        }

        menu.AddItem(new GUIContent(">> New Group"), false, userData =>
        {
            addGroup = "New Group";
            addGroupTo = (AssetDefinition)userData;
        }, asset);

        menu.ShowAsContext();
    }

    bool IsSelected(object obj)
    {
        return ReferenceEquals(obj, Selected);
    }

    void Select(AssetDefinition asset, bool focusEditor)
    {
        if (asset == null)
        {
            selectedAssetGuid = null;
        }
        else
        {
            selectedAssetGuid = asset.Guid.ToString();
        }

        Repaints = 10;
        Selected = asset;
        BeginClearFocus();

        DEditorGUI.UseEvent();

        if (focusEditor)
        {
            AscensionEditorWindow.Open();
        }
    }
}
