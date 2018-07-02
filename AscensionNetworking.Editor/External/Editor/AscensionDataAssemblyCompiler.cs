using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Ascension.Compiler;
using Ascension.Networking;
using Ascension.Networking.Sockets;
using Ascension.Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class AscensionDataAssemblyCompiler
{
    private static string AssetDir
    {
        get { return Application.dataPath; }
    }

    private static string SourceDir { get; set; }
    private static string PluginDir { get; set; }

    private static string AscensionDataPath
    {
        get { return EditorUtils.MakePath(AssetDir, "__ASCENSION__", "Networking", "Data"); }
    }

    private static string AscensionScriptsPath
    {
        get { return EditorUtils.MakePath(AssetDir, "__ASCENSION__", "Networking", "Ascension"); }
    }

    private static string UNetScriptsPath
    {
        get { return EditorUtils.MakePath(AssetDir, "__ASCENSION__", "Networking", "UNet"); }
    }

    private static string AscensionUserAssemblyPath
    {
        get { return EditorUtils.MakePath(AssetDir, "__ASCENSION__", "Networking", "Plugins", "Ascension.Data.dll"); }
    }

    private static string AscensionUserAssemblyAsset
    {
        get { return EditorUtils.MakePath("Assets", "__ASCENSION__", "Networking", "Plugins", "Ascension.Data.dll"); }
    }

    private static string AscensionPrecompileAssemblyPath
    {
        get { return EditorUtils.MakePath(AssetDir, "__ASCENSION__", "Networking", "Plugins", "Ascension.dll"); }
    }

    private static string NetworkFile
    {
        get { return EditorUtils.MakePath(AscensionDataPath, "Network.cs"); }
    }

    private static string ProjectFile
    {
        get { return EditorUtils.MakePath(AscensionDataPath, "NetworkData.cs"); }
    }

    private static string AssemblyInfoFile
    {
        get { return EditorUtils.MakePath(AscensionDataPath, "AssemblyInfo.cs"); }
    }

    private static string PrefabsFile
    {
        get { return EditorUtils.MakePath(AscensionDataPath, "Prefabs.cs"); }
    }

    private static string MapsFile
    {
        get { return EditorUtils.MakePath(AscensionDataPath, "Scenes.cs"); }
    }

    private static string NetworkFileTemp
    {
        get { return EditorUtils.MakePath(SourceDir, "Network.cs"); }
    }

    private static string ProjectFileTemp
    {
        get { return EditorUtils.MakePath(SourceDir, "NetworkData.cs"); }
    }

    private static string PrefabsFileTemp
    {
        get { return EditorUtils.MakePath(SourceDir, "Prefabs.cs"); }
    }

    private static string MapsFileTemp
    {
        get { return EditorUtils.MakePath(SourceDir, "Scenes.cs"); }
    }

    private static string AssemblyInfoFileTemp
    {
        get { return EditorUtils.MakePath(SourceDir, "AssemblyInfo.cs"); }
    }

    private static string SourceFileList
    {
        get
        {
            return
                "\"" + NetworkFileTemp + "\" " +
                "\"" + PrefabsFileTemp + "\" " +
                "\"" + MapsFileTemp + "\" " +
                "\"" + ProjectFileTemp + "\" " +
                "\"" + AssemblyInfoFileTemp + "\" ";
        }
    }

    private static string PreAssemblyReferencesList
    {
        get
        {
            List<string> assemblies = new List<string>();
            assemblies.Add(UnityEngineAssemblyPath);
            assemblies.Add(UnityNetworkingAssemblyPath);
            return string.Join(" ", assemblies.Select(x => "-reference:\"" + x + "\"").ToArray());
        }
    }

    private static string PostAssemblyReferencesList
    {
        get
        {
            List<string> assemblies = new List<string>();
            assemblies.Add(UnityEngineAssemblyPath);
            assemblies.Add(AscensionPrecompileAssemblyPath);
            return string.Join(" ", assemblies.Select(x => "-reference:\"" + x + "\"").ToArray());
        }
    }

    private static string AssemblyReferencesList
    {
        get
        {
            List<string> assemblies = new List<string>();
            assemblies.Add(UnityEngineAssemblyPath);
            assemblies.Add(UnityNetworkingAssemblyPath);
            assemblies.Add(AscensionPrecompileAssemblyPath);
            return string.Join(" ", assemblies.Select(x => "-reference:\"" + x + "\"").ToArray());
        }
    }

    private static List<string> AscensionScriptsList
    {
        get
        {
            //Ignore editor files using LINQ
            String[] scriptsArray = Directory.GetFiles(AscensionScriptsPath, "*.cs", SearchOption.AllDirectories).Where(name => !name.EndsWith("Editor.cs", StringComparison.OrdinalIgnoreCase)).ToArray();
            List<string> scripts = new List<string>(scriptsArray);
            return scripts;
        }
    }

    private static string AscensionScripts
    {
        get
        {
            return string.Join(" ", AscensionScriptsList.Select(x => "\"" + x + "\"").ToArray());
        }
    }

    private static List<string> UNetScriptsList
    {
        get
        {
            //Ignore editor files using LINQ
            String[] scriptsArray = Directory.GetFiles(UNetScriptsPath, "*.cs", SearchOption.AllDirectories).Where(name => !name.EndsWith("Editor.cs", StringComparison.OrdinalIgnoreCase)).ToArray();
            List<string> scripts = new List<string>(scriptsArray);
            return scripts;
        }
    }

    private static string UNetScripts
    {
        get
        {
            return string.Join(" ", UNetScriptsList.Select(x => "\"" + x + "\"").ToArray());
        }
    }

    private static string CombinedScripts
    {
        get { return AscensionScripts + " " + UNetScripts; }
    }

    private static bool IsOsx
    {
        get { return !IsWin; }
    }

    private static bool IsWin
    {
        get
        {
            return
                Environment.OSVersion.Platform == PlatformID.Win32NT ||
                Environment.OSVersion.Platform == PlatformID.Win32S ||
                Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                Environment.OSVersion.Platform == PlatformID.WinCE;
        }
    }

    private static bool IsUnity5
    {
        get { return Application.unityVersion.Trim()[0] == '5'; }
    }

    private static string MonoCompiler
    {
        get
        {
            if (IsUnity5)
            {
                return "4.5/mcs.exe";
            }
            return "2.0/gmcs.exe";
        }
    }

    private static string CsharpCompilerPath
    {
        get
        {
            if (IsOsx)
            {
                return EditorUtils.MakePath(EditorApplication.applicationContentsPath,
                    "Frameworks/MonoBleedingEdge/lib/mono/" + MonoCompiler);
            }
            return EditorUtils.MakePath(EditorApplication.applicationContentsPath,
                "MonoBleedingEdge/lib/mono/" + MonoCompiler);
        }
    }

    private static string UnityEngineAssemblyPath
    {
        get
        {
            if (IsOsx)
            {
                return EditorUtils.MakePath(EditorApplication.applicationContentsPath,
                    "Frameworks/Managed/UnityEngine.dll");
            }
            return EditorUtils.MakePath(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll");
        }
    }

    private static string UnityNetworkingAssemblyPath
    {
        get
        {
            if (IsOsx)
            {
                return EditorUtils.MakePath(EditorApplication.applicationContentsPath,
                    "Frameworks/UnityExtensions/Unity/Networking/UnityEngine.Networking.dll");
            }
            return EditorUtils.MakePath(EditorApplication.applicationContentsPath, "UnityExtensions/Unity/Networking/UnityEngine.Networking.dll");
        }
    }

    private static string MonoPath
    {
        get
        {
            if (IsOsx)
            {
                return EditorUtils.MakePath(EditorApplication.applicationContentsPath,
                    "Frameworks/MonoBleedingEdge/bin/mono");
            }
            return EditorUtils.MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/bin/mono.exe");
        }
    }

    public static ManualResetEvent CodeEmit()
    {
        ManualResetEvent evnt = new ManualResetEvent(false);

        try
        {
            // create path if it doesn't exist
            Directory.CreateDirectory(AscensionDataPath);

            // setup compiler options
            AscensionCompilerOperation op = new AscensionCompilerOperation();
            op.projectFilePath = ProjectFile;
            op.project = File.Exists("Assets/__ASCENSION__/Networking/Resources/User/project.bytes")
                ? File.ReadAllBytes("Assets/__ASCENSION__/Networking/Resources/User/project.bytes").ToObject<Project>()
                : new Project();

            // network config
            op.networkFilePath = NetworkFile;
            op.assemblyInfoFilePath = AssemblyInfoFile;

            // maps config
            op.scenesFilePath = MapsFile;
            op.prefabsFilePath = PrefabsFile;

            // run code emitter
            AscensionCompiler.Run(op, false);

            // we are done
            evnt.Set();

            // continue
            EditorSaver.Invoke(() =>
            {
                EmissionDone(op);
            });
        }
        catch (Exception exn)
        {
            evnt.Set();
            Debug.LogException(exn);
        }

        return evnt;
    }

    public static ManualResetEvent Run()
    {
        ManualResetEvent evnt = new ManualResetEvent(false);

        try
        {
            // calculate source dir
            SourceDir = EditorUtils.MakePath(Path.GetDirectoryName(AssetDir), "Temp", "Ascension");

            // ensure temp path exists
            Directory.CreateDirectory(SourceDir);

            // setup compiler options
            AscensionCompilerOperation op = new AscensionCompilerOperation();
            op.projectFilePath = ProjectFileTemp;
            op.project = File.Exists("Assets/__ASCENSION__/Networking/Resources/User/project.bytes")
                ? File.ReadAllBytes("Assets/__ASCENSION__/Networking/Resources/User/project.bytes").ToObject<Project>()
                : new Project();

            // network config
            op.networkFilePath = NetworkFileTemp;
            op.assemblyInfoFilePath = AssemblyInfoFileTemp;

            // maps config
            op.scenesFilePath = MapsFileTemp;
            op.prefabsFilePath = PrefabsFileTemp;

            // run code emitter
            AscensionCompiler.Run(op);
            RunCSharpCompiler(op, evnt);
        }
        catch (Exception exn)
        {
            evnt.Set();
            Debug.LogException(exn);
        }

        return evnt;
    }

    static void RunCSharpCompiler(AscensionCompilerOperation op, ManualResetEvent evnt)
    {
#if DEBUG
        const string CMD_ARGS = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug+ -optimize- -warn:{3} ";
#else
    const string CMD_ARGS = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug- -optimize+ -warn:{3} ";
#endif

        string args = CMD_ARGS;

        if (Core.IsDebugMode)
        {
            args += "-define:DEBUG ";
        }

        if (IsUnity5)
        {
            args += "-sdk:2 ";
        }


        Process p = new Process();
        p.StartInfo.FileName = MonoPath;
        p.StartInfo.Arguments = string.Format(args + SourceFileList, CsharpCompilerPath, AscensionUserAssemblyPath, AssemblyReferencesList, Mathf.Clamp(RuntimeSettings.Instance.compilationWarnLevel, 0, 4));

        p.EnableRaisingEvents = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardOutput = true;

        p.ErrorDataReceived += ErrorDataReceived;
        p.OutputDataReceived += OutputDataReceived;

        p.Exited += (s, ea) => {
            // we are done
            evnt.Set();

            // continue
            EditorSaver.Invoke(() => {
                if (p.ExitCode == 0) { CompilationDone(op); }
            });
        };

        p.Start();
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();
    }



    private static void RunCSharpCompilerOld(AscensionCompilerOperation op, ManualResetEvent evnt)
    {
#if DEBUG
        const string CMD_ARGS =
            "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug+ -optimize- -warn:{3} ";
#else
        const string CMD_ARGS = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug- -optimize+ -warn:{3} ";
#endif

        string args = CMD_ARGS;

        if (Core.IsDebugMode)
        {
            args += "-define:DEBUG ";
        }

        if (IsUnity5)
        {
            args += "-sdk:2 ";
        }

        string preCompArguments = string.Format(args + CombinedScripts, CsharpCompilerPath, AscensionPrecompileAssemblyPath,
    PreAssemblyReferencesList, Mathf.Clamp(RuntimeSettings.Instance.compilationWarnLevel, 0, 4));

        string postCompArguments = string.Format(args + SourceFileList, CsharpCompilerPath, AscensionUserAssemblyPath,
    PostAssemblyReferencesList, Mathf.Clamp(RuntimeSettings.Instance.compilationWarnLevel, 0, 4));

        PreCompile(op, evnt, preCompArguments, postCompArguments);
    }

    private static void PreCompile(AscensionCompilerOperation op, ManualResetEvent evnt, string preArgs, string postArgs)
    {
        Process p = new Process();
        p.StartInfo.FileName = MonoPath;
        p.StartInfo.Arguments = preArgs;
        p.EnableRaisingEvents = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardOutput = true;

        p.ErrorDataReceived += ErrorDataReceived;
        p.OutputDataReceived += OutputDataReceived;

        p.Exited += (s, ea) =>
        {
            if (p.ExitCode == 0)
            {
                PostCompile(op, evnt, postArgs);

                Debug.Log("Pre-compile finished!");
            }
        };

        p.Start();
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();
    }

    private static void PostCompile(AscensionCompilerOperation op, ManualResetEvent evnt, string postArgs)
    {
        Process p = new Process();
        p.StartInfo.FileName = MonoPath;
        p.StartInfo.Arguments = postArgs;

        p.EnableRaisingEvents = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardOutput = true;

        p.ErrorDataReceived += ErrorDataReceived;
        p.OutputDataReceived += OutputDataReceived;

        p.Exited += (s, ea) =>
        {
            // we are done
            evnt.Set();

            // continue
            EditorSaver.Invoke(() =>
            {
                if (p.ExitCode == 0)
                {
                    CompilationDone(op);
                }
            });
        };

        p.Start();
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();
    }

    private static void CompilationDone(AscensionCompilerOperation op)
    {
        AssetDatabase.ImportAsset(AscensionUserAssemblyAsset, ImportAssetOptions.ForceUpdate);

        Debug.Log("AscensionCompiler: Success!");

        EditorPrefs.SetInt("ASCENSION_UNCOMPILED_COUNT", 0);
        EditorPrefs.SetBool(SceneLauncher.COMPILE_SETTING, false);
    }

    private static void EmissionDone(AscensionCompilerOperation op)
    {
        AssetDatabase.ImportAsset(AscensionUserAssemblyAsset, ImportAssetOptions.ForceUpdate);

        Debug.Log("AscensionEmitter: Success!");

        EditorPrefs.SetInt("ASCENSION_UNCOMPILED_COUNT", 0);
        EditorPrefs.SetBool(SceneLauncher.COMPILE_SETTING, false);

        ForceUnityRecompilation();
    }

    private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            Debug.Log(e.Data);
        }
    }

    private static void ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            if (e.Data.Contains(": warning") && !e.Data.Contains(": error"))
            {
                Debug.LogWarning(e.Data);
            }
            else
            {
                Debug.LogError(e.Data);
            }
        }
    }

    private static void ForceUnityRecompilation()
    {
        //Force a recompilation
        AssetDatabase.StartAssetEditing();
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            MonoScript script = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript)) as MonoScript;
            if (script != null)
            {
                AssetDatabase.ImportAsset(assetPath);
                //Break after the first script because Unity only needs 1 script touched to cause a recompilation
                break;
            }
        }
        AssetDatabase.StopAssetEditing();
    }
}