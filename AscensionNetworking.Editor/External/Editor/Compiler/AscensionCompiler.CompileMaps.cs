using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ascension.Networking;
using UnityEditor;

partial class AscensionCompiler
{
    public static void CompileMaps(AscensionCompilerOperation op)
    {
        List<Scene> scenes = new List<Scene>();

        for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                string name = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);

                scenes.Add(new Scene
                {
                    Name = name,
                    Identifier = EditorUtils.CSharpIdentifier(name)
                });
            }
        }

        foreach (IGrouping<string, Scene> group in scenes.GroupBy(x => x.Identifier))
        {
            if (group.Count() > 1)
            {
                throw new AscensionException("You have several scenes named '{0}' in the build settings.", group.Key);
            }
        }

        using (AscensionSourceFile file = new AscensionSourceFile(op.scenesFilePath))
        {
            file.EmitLine("using System.Collections.Generic;");
            file.EmitScope("public static class AscensionScenes", () =>
            {
                file.EmitLine(
                    "static public readonly Dictionary<string, int> nameLookup = new Dictionary<string, int>();");
                file.EmitLine(
                    "static public readonly Dictionary<int, string> indexLookup = new Dictionary<int, string>();");

                file.EmitScope("public static void AddScene(short prefix, short id, string name)", () =>
                {
                    file.EmitLine("int index = (prefix << 16) | (int)id;");
                    file.EmitLine("nameLookup.Add(name, index);");
                    file.EmitLine("indexLookup.Add(index, name);");
                });

                file.EmitLine("static public IEnumerable<string> AllScenes { get { return nameLookup.Keys; } }");

                file.EmitScope("static AscensionScenes()", () =>
                {
                    for (int n = 0; n < scenes.Count; ++n)
                    {
                        file.EmitLine("AddScene(0, {1}, \"{0}\");", scenes[n].Name, n);
                    }
                });

                for (int n = 0; n < scenes.Count; ++n)
                {
                    file.EmitLine("public const string {0} = \"{1}\";", scenes[n].Identifier, scenes[n].Name);
                }
            });

            file.EmitScope("namespace Ascension.Networking.Data", () =>
            {
                file.EmitScope("public static class AscensionScenesInternal", () =>
                {
                    file.EmitLine(
                        "static public int GetSceneIndex(string name) { return AscensionScenes.nameLookup[name]; }");
                    file.EmitLine(
                        "static public string GetSceneName(int index) { return AscensionScenes.indexLookup[index]; }");
                });
            });
        }
    }

    private struct Scene
    {
        public string Identifier;
        public string Name;
    }

    private class SceneComparer : IEqualityComparer<EditorBuildSettingsScene>
    {
        public bool Equals(EditorBuildSettingsScene x, EditorBuildSettingsScene y)
        {
            return x.path.CompareTo(y.path) == 0;
        }

        public int GetHashCode(EditorBuildSettingsScene obj)
        {
            return obj.path.GetHashCode();
        }
    }
}