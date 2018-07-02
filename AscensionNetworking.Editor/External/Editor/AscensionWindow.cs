using System;
using UnityEditor;
using UnityEngine;
using Ascension.Compiler;
using System.Linq;
using System.IO;
using Ascension.Networking.Sockets;
using DEditorGUI = Ascension.Tools.EditorGUI;

public abstract class AscensionWindow : EditorWindow
{
    public static string ProjectPath
    {
        get { return "Assets/__ASCENSION__/Networking/Resources/User/project.bytes"; }
    }

    public static string ProjectTempNewPath
    {
        get { return "Temp/__ASCENSION__/Networking/Resources/User/project_new.bytes"; }
    }

    public static string ProjectTempOldPath
    {
        get { return "Temp/__ASCENSION__/Networking/Resources/User/project_old.bytes"; }
    }

    float repaintTime;

    static bool clear;
    static protected int Repaints;

    static public Project Project;
    static public DateTime ProjectModifyTime;

    static protected AssetDefinition Selected;

    protected bool HasProject
    {
        get { return Project != null; }
    }

    protected void Save()
    {
        if (HasProject)
        {
            AscensionBackgroundSaver.Save(Project);
        }
    }


    protected void Update()
    {
        if ((Repaints > 0) || ((repaintTime + 0.05f) < Time.realtimeSinceStartup))
        {
            Repaint();
            repaintTime = Time.realtimeSinceStartup;
        }
    }

    protected void OnGUI()
    {
        if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
        {
            const EventModifiers MODS = EventModifiers.Control;

            if ((Event.current.modifiers & MODS) == MODS)
            {
                Event.current.Use();

                // compile!
                if (RuntimeSettings.Instance.compileAsDll)
                    AscensionDataAssemblyCompiler.Run();
                else
                    AscensionDataAssemblyCompiler.CodeEmit();
            }
        }

        DEditorGUI.Tooltip = "";

        LoadProject();

        if (Event.current.type == EventType.Repaint)
        {
            Repaints = Mathf.Max(0, Repaints - 1);
        }

        if (Selected != null && Selected.Deleted)
        {
            Selected = null;
        }
    }

    protected void ClearAllFocus()
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        if (Repaints == 0)
        {
            clear = false;
            return;
        }

        if (clear)
        {
            GUI.SetNextControlName("ClearFocusFix");
            GUI.Button(new Rect(0, 0, 0, 0), "", GUIStyle.none);
            GUI.FocusControl("ClearFocusFix");
        }
    }

    protected void BeginClearFocus()
    {
        // we are clearing
        clear = true;

        // repaint a few times
        Repaints = 10;

        // this also helps
        GUIUtility.keyboardControl = 0;
    }

    protected void LoadProject()
    {
        if (File.Exists(ProjectPath) == false)
        {
            Debug.Log("Creating project... " + ProjectPath);

            Project = new Project();
            Save();
        }
        else
        {
            if (Project == null)
            {
                Debug.Log("Loading project... " + ProjectPath);
                Project = File.ReadAllBytes(ProjectPath).ToObject<Project>();
                ProjectModifyTime = File.GetLastWriteTime(ProjectPath);

                if (Project.Merged == false)
                {
                    Debug.Log("Merged Project... " + ProjectPath);

                    Project.Merged = true;
                    Project.RootFolder.Assets = Project.RootFolder.AssetsAll.ToArray();

                    Save();
                }
            }
            else
            {

            }
        }
    }
}