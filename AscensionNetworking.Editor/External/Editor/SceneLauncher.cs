using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Ascension.Networking.Sockets;
using UnityEngine;
using UnityEditor;
using Process = System.Diagnostics.Process;
using DEditorGUI = Ascension.Tools.EditorGUI;
using System.Runtime.InteropServices;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Ascension.Networking
{

    public class SceneLauncher : EditorWindow
    {
        public const string COMPILE_SETTING = "ASCENSION_COMPILE";

        private static SceneLauncher sceneLauncher;

        private const string NETWORK_LIBRARY_NAME = "Ascension";
        
        private const string SCENES_DIRECTORY = "Scenes";

        private const int STAGE_NONE = 0;
        private const int STAGE_COMPILE_PLAYER = 1;
        private const int STAGE_START_PLAYERS = 2;
        private const int STAGE_START_EDITOR = 3;
        private const int STAGE_PLAYING = 4;

        private const string DEBUGSTART_STAGE = NETWORK_LIBRARY_NAME + "_DEBUGSTART_STAGE";
        private const string DEBUGSTART_RESTORESCENE = NETWORK_LIBRARY_NAME + "_DEBUGSTART_RESTORESCENE";

        private float lastRepaint;

        private bool isOSX
        {
            get { return Application.platform == RuntimePlatform.OSXEditor; }
        }

        private bool IsWindows
        {
            get { return Application.platform == RuntimePlatform.WindowsEditor; }
        }

        private string DebugScene
        {
            get { return "Assets/__ASCENSION__/" + SCENES_DIRECTORY + "/" + NETWORK_LIBRARY_NAME + "DebugScene.unity"; }
        }

        private string DebugSceneNonPro
        {
            get { return "Assets/__ASCENSION__/" + SCENES_DIRECTORY + "/" + NETWORK_LIBRARY_NAME + "NonProScene.unity"; }
        }

        public string[] scenes
        {
            get { return (new[] {DebugScene}).Concat(EditorBuildSettings.scenes.Select(x => x.path)).ToArray(); }
        }

        private string PlayerPath
        {
            get
            {
                if (isOSX)
                {
                    return NETWORK_LIBRARY_NAME + "_DebugStart_Build/" + NETWORK_LIBRARY_NAME + "_DebugStart_Build";
                }

                return NETWORK_LIBRARY_NAME + "_DebugStart_Build\\" + NETWORK_LIBRARY_NAME + "_DebugStart_Build.exe";
            }
        }

        private string PlayerPathExecutable
        {
            get
            {
                if (isOSX)
                {
                    string[] paths = new string[]
                    {
                        PlayerPath + ".app/Contents/MacOS/" + PlayerSettings.productName,
                        PlayerPath + "/Contents/MacOS/" + PlayerSettings.productName,
                        PlayerPath + "/Contents/MacOS/" + NETWORK_LIBRARY_NAME + "_DebugStart_Build",
                        PlayerPath + ".app/Contents/MacOS/" + NETWORK_LIBRARY_NAME + "_DebugStart_Build"
                    };

                    for (int i = 0; i < paths.Length; ++i)
                    {
                        if (File.Exists(paths[i])) return paths[i];
                    }

                    Debug.Log("Could not find executable at any of the following paths: " +
                        Utils.Join(paths, ", "));
                }

                return PlayerPath;
            }
        }

        private BuildTarget BuildTarget
        {
            get
            {
                if (isOSX)
                {
                    return BuildTarget.StandaloneOSXIntel;
                }

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
                {
                    return BuildTarget.StandaloneWindows64;
                }
                else
                {
                    return BuildTarget.StandaloneWindows;
                }
            }
        }

        private BuildOptions BuildOptions
        {
            get { return BuildOptions.None; }
        }


        #region Methods

        [MenuItem("Ascension/Scenes", priority = 20)]
        public static void OpenScenes()
        {
            sceneLauncher = GetWindow<SceneLauncher>();
            sceneLauncher.titleContent = new GUIContent("Scenes");
            sceneLauncher.name = "Scenes";
            sceneLauncher.Show();
        }

        private void BuildPlayer()
        {
            try
            {
                if (RuntimeSettings.Instance.debugClientCount == 0 &&
                    RuntimeSettings.Instance.debugEditorMode == EditorStartMode.Server)
                {
                    EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_START_PLAYERS);
                    return;
                }
                string path =
                    EditorUtils.MakePath(
                        Path.GetDirectoryName(Application.dataPath),
                        isOSX ? PlayerPath : Path.GetDirectoryName(PlayerPath)
                        );

                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception exn)
                {
                    Debug.LogException(exn);
                }

                string result;

                result = BuildPipeline.BuildPlayer(scenes, PlayerPath, BuildTarget, BuildOptions);
                result = (result ?? "").Trim();

                if (result.Length == 0)
                {
                    EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_START_PLAYERS);
                }
                else
                {
                    EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
                }
            }
            catch
            {
                EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
                throw;
            }
        }

        private void PositionWindowsOnOSX()
        {
            if (isOSX && (RuntimeSettings.Instance.debugEditorMode == EditorStartMode.None))
            {
                Process p = new Process();
                p.StartInfo.FileName = "osascript";
                p.StartInfo.Arguments =

                    @"-e 'tell application """ + UnityEditor.PlayerSettings.productName + @"""
	activate
end tell'";

                p.Start();
            }
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(System.String className, System.String windowName);

        public static void SetPosition(int x, int y, string processName, int resX = 0, int resY = 0)
        {
            SetWindowPos(FindWindow(null, processName), 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
        }
#endif

        private void PositionWindowOnWindows(string processName, int x, int y)
        {
            SetPosition(x, y, processName);
        }

        private void StartPlayers()
        {
            try
            {
                int clientCount = RuntimeSettings.Instance.debugClientCount;

                // starting server player
                if (RuntimeSettings.Instance.debugEditorMode == EditorStartMode.Client ||
                    RuntimeSettings.Instance.debugEditorMode == EditorStartMode.None)
                {
                    if (RuntimeSettings.Instance.debugEditorMode == EditorStartMode.Client)
                    {
                        clientCount -= 1;
                    }

                    Process p = new Process();
                    p.StartInfo.FileName = PlayerPathExecutable;
                    p.StartInfo.Arguments = "--" + NETWORK_LIBRARY_NAME.ToLower() + "-debugstart-server";
                    p.Start();
                }

                // start client players
                for (int i = 0; i < clientCount; ++i)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = PlayerPathExecutable;
                    p.StartInfo.Arguments = "--" + NETWORK_LIBRARY_NAME.ToLower() + "-debugstart-client --" + NETWORK_LIBRARY_NAME.ToLower() + "-window-index-" + i;
                    p.Start();
                }

                PositionWindowsOnOSX();


            }
            finally
            {
                EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_START_EDITOR);
            }
        }

        private void StopPlayers()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                try
                {
                    foreach (Process p in Process.GetProcesses())
                    {
                        try
                        {
                            if (p.ProcessName == NETWORK_LIBRARY_NAME + "_DebugStart_Build")
                            {
                                p.Kill();
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                try
                {
                    foreach (Process p in Process.GetProcesses())
                    {
                        try
                        {
                            if (p.ProcessName == PlayerSettings.productName)
                            {
                                p.Kill();
                            }

                            if (p.ProcessName == NETWORK_LIBRARY_NAME + "_DebugStart_Build")
                            {
                                p.Kill();
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
        }

        void LoadAndStartScene()
        {
            EditorPrefs.SetString(DEBUGSTART_RESTORESCENE, EditorApplication.currentScene);
            EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_PLAYING);

            if (EditorApplication.OpenScene(DebugScene))
            {
                EditorApplication.isPlaying = true;
            }
        }

        void StartEditor()
        {
            try
            {
                if (EditorUtils.HasPro == false)
                {
                    LoadAndStartScene();
                }
                else
                {
                    switch (RuntimeSettings.Instance.debugEditorMode)
                    {
                        case EditorStartMode.Client:
                        case EditorStartMode.Server:
                            LoadAndStartScene();
                            break;

                        case EditorStartMode.None:
                            EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
                            break;
                    }
                }
            }
            catch
            {
                EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
                throw;
            }
        }

        void StopEditor()
        {
            if (EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                // reload scene
                if (EditorPrefs.HasKey(DEBUGSTART_RESTORESCENE))
                {
                    //if (EditorPrefs.GetString(DEBUGSTART_RESTORESCENE) != "")
                    if (DEBUGSTART_RESTORESCENE != "ASCENSION_DEBUGSTART_RESTORESCENE")
                        EditorSceneManager.OpenScene(EditorPrefs.GetString(DEBUGSTART_RESTORESCENE));
                    EditorPrefs.DeleteKey(DEBUGSTART_RESTORESCENE);
                }

                // kill players
                StopPlayers();

                // reset stage state
                EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
            }
        }

        void OnEnable()
        {
            name = (titleContent = new GUIContent("Scenes")).ToString();
            lastRepaint = 0f;
        }

        void Update()
        {
            if (lastRepaint + 0.1f < Time.realtimeSinceStartup)
            {
                lastRepaint = Time.realtimeSinceStartup;
                Repaint();
            }

            switch (EditorPrefs.GetInt(DEBUGSTART_STAGE))
            {
                case STAGE_COMPILE_PLAYER:
                    BuildPlayer();
                    break;

                case STAGE_START_PLAYERS:
                    StartPlayers();
                    break;

                case STAGE_START_EDITOR:
                    StartEditor();
                    break;

                case STAGE_PLAYING:
                    StopEditor();
                    break;
            }
        }

        void SetStage(int stage)
        {
            EditorPrefs.SetInt(DEBUGSTART_STAGE, stage);
        }

        void Settings_ServerPort()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            GUILayout.BeginHorizontal();

            settings.debugStartPort = EditorGUILayout.IntField("Server Port", settings.debugStartPort);

            if (GUILayout.Button("Refresh", EditorStyles.miniButton))
            {
                Socket sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sc.Bind(new IPEndPoint(IPAddress.Any, 0));

                IPEndPoint ipEndPoint
                    = sc.LocalEndPoint as IPEndPoint;
                if (ipEndPoint != null)
                    settings.debugStartPort = ipEndPoint.Port;

                try
                {
                    sc.Shutdown(SocketShutdown.Both);
                    sc.Close();
                }
                catch { }

                EditorUtility.SetDirty(settings);
            }

            GUILayout.EndHorizontal();

        }

        void Settings()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;
            GUILayout.BeginVertical();
            Settings_ServerPort();

            if (EditorUtils.HasPro)
            {
                settings.debugEditorMode = (EditorStartMode)EditorGUILayout.EnumPopup("Editor Mode", settings.debugEditorMode);
                settings.debugClientCount = EditorGUILayout.IntField("Clients", settings.debugClientCount);
            }

            GUILayout.EndVertical();
        }

        Vector2 sceneScrollPosition;

        void Scenes()
        {
            sceneScrollPosition = GUILayout.BeginScrollView(sceneScrollPosition);

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    var sceneName = Path.GetFileNameWithoutExtension(scene.path);

                    GUILayout.Space(2);
                    GUILayout.BeginHorizontal(DEditorGUI.HeaderBackground, GUILayout.Height(DEditorGUI.HEADER_HEIGHT));

                    var isCurrent = SceneManager.GetActiveScene().path == scene.path;
                    GUIStyle label = new GUIStyle("Label");
                    label.normal.textColor = isCurrent ? DEditorGUI.HighlightColor : label.normal.textColor;
                    GUILayout.Label(sceneName);

                    // Scene Edit Button

                    EditorGUI.BeginDisabledGroup(isCurrent);

                    if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                        EditorSceneManager.OpenScene(scene.path);
                    }

                    EditorGUI.EndDisabledGroup();

                    // Scene Start Button

                    if (GUILayout.Button("Play As Server", EditorStyles.miniButton, GUILayout.Width(100)))
                    {
                        RuntimeSettings settings = RuntimeSettings.Instance;

                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            settings.debugStartMapName = sceneName;
                            settings.debugPlayAsServer = true;
                            settings.debugEditorMode = EditorStartMode.Server;

                            // save asset
                            EditorUtility.SetDirty(settings);
                            AssetDatabase.SaveAssets();

                            // set stage
                            SetStage(STAGE_COMPILE_PLAYER);
                        }
                    }

                    if (EditorUtils.HasPro)
                    {
                        if (GUILayout.Button("Debug Start", EditorStyles.miniButton, GUILayout.Width(100)))
                        {
                            RuntimeSettings settings = RuntimeSettings.Instance;

                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                settings.debugStartMapName = sceneName;
                                settings.debugPlayAsServer = false;

                                // save asset
                                EditorUtility.SetDirty(settings);
                                AssetDatabase.SaveAssets();

                                // set stage
                                SetStage(STAGE_COMPILE_PLAYER);
                            }
                        }
                    }

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(DEditorGUI.GLOBAL_INSET, DEditorGUI.GLOBAL_INSET, position.width - (DEditorGUI.GLOBAL_INSET * 2), position.height - (DEditorGUI.GLOBAL_INSET * 2)));
            GUILayout.Space(4);

            DEditorGUI.Header("Debug Start Settings", "debug");
            Settings();

            DEditorGUI.Header("Scenes", "scenes");
            Scenes();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(RuntimeSettings.Instance);
                AssetDatabase.SaveAssets();
            }

            GUILayout.EndArea();
        }
        #endregion
    }
}
