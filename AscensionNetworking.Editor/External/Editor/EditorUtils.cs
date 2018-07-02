using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Ascension.Networking
{
    public static class EditorUtils
    {
        public static bool IsEditorPlaying
        {
            get
            {
                return EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode ||
                       EditorApplication.isPaused;
            }
        }

        public static bool HasPro
        {
            get { return InternalEditorUtility.HasPro(); }
        }

        public static string CSharpIdentifier(this string value)
        {
            if (Char.IsDigit(value[0]))
            {
                value = "_" + value;
            }

            return Regex.Replace(value, "[^a-zA-Z0-9_]+", "_");
        }

        public static string MakePath(params string[] parts)
        {
            return
                String.Join(Path.DirectorySeparatorChar.ToString(),
                    parts.Select(x => x.TrimEnd('/', '\\').TrimStart('\\')).ToArray())
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);
        }

        public static string MakeAssetPath(params string[] parts)
        {
            string path;

            path = MakePath(parts);
            path = path.Replace(MakePath(Application.dataPath), "");
            path = MakePath("Assets", path);

            return path;
        }

        public static string Join<T>(this IEnumerable<T> items, string seperator)
        {
            return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
        }

        public static byte[] GetResourceBytes(string path)
        {
            return Assembly.GetExecutingAssembly().GetResourceBytes(path);
        }

        public static byte[] GetResourceBytes(this Assembly asm, string path)
        {
            using (Stream stream = asm.GetManifestResourceStream(path))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static void InstallAsset(string file, Func<byte[]> data)
        {
            if (!File.Exists(file))
            {
                InstallAsset(file, data());
            }
        }

        public static void InstallAsset(string file, byte[] data)
        {
            string path = Path.GetDirectoryName(file);

            try
            {
                Directory.CreateDirectory(path);
            }
            catch
            {
            }

            File.WriteAllBytes(file, data);
            AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceUpdate);
        }
    }
}