using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace Ascension.Networking
{

    /// <summary>
    /// The in-game console window
    /// </summary>
    /// <example>
    /// *Example:* Writing a custom message to the console.
    /// 
    /// ```csharp
    /// void Start() {
    ///   NetConsole.Write("[Start:" + this.gameObject.name + "]);
    /// }
    /// ```
    /// </example>
    public class NetConsole : MonoBehaviour
    {
        private struct Line
        {
            public Color color;
            public string text;
        }

        /*public static readonly Color PastelPurpleRGBA = new Color(244, 209, 255);
        public static readonly Color PastelPinkRGBA = new Color(255, 209, 220);
        public static readonly Color PastelGreenRGBA = new Color(220, 255, 209);
        public static readonly Color PastelTealRGBA = new Color(209, 255, 244);
        public static readonly Color PastelRedRGBA = new Color(255, 133, 162);
        public static readonly Color PastelBlueRGBA = new Color(209, 220, 255);
        public static readonly Color PastelLightBlueRGBA = new Color(209, 243, 255);*/

        public static readonly string PastelPurple = "#f4d1ff";
        public static readonly string PastelPink = "#ffd1ff";
        public static readonly string PastelGreen = "#dcffd1";
        public static readonly string PastelTeal = "#d1fff4";
        public static readonly string PastelRed = "#ff85a2";
        public static readonly string PastelBlue = "#d1dcff";
        public static readonly string PastelLightBlue = "#d1f3ff";

        static volatile int changed = 0;
        static readonly object Mutex = new object();
        static readonly RingBuffer<Line> lines = new RingBuffer<Line>(1024);
        static readonly RingBuffer<Line> linesRender = new RingBuffer<Line>(1024);

        [SerializeField]
        float consoleHeight = 0.5f;

        [SerializeField]
        int lineHeight = 11;

        [SerializeField]
        public bool visible = true;

        [SerializeField]
        public KeyCode toggleKey = KeyCode.Tab;

        [SerializeField]
        float backgroundTransparency = 0.75f;

        [SerializeField]
        int padding = 10;

        [SerializeField]
        int fontSize = 10;

        [SerializeField]
        int inset = 10;

        static int LinesCount
        {
            get { return linesRender.Count; }
        }

        static IEnumerable<Line> Lines
        {
            get { return linesRender; }
        }

        /// <summary>
        /// Write one line to the console
        /// </summary>
        /// <param name="line">Text to write</param>
        /// <param name="color">Color of the text</param>
        /// <example>
        /// *Example:* Writing a custom message to the console in color.
        /// 
        /// ```csharp
        /// void OnDeath() {
        ///   NetConsole.Write("[Death:" + this.gameObject.name + "], Color.Red);
        /// }
        /// ```
        /// </example>
        public static void Write(string line, Color color)
        {
            lock (Mutex)
            {
                if (line.Contains("\r") || line.Contains("\n"))
                {
                    foreach (string l in Regex.Split(line, "[\r\n]+"))
                    {
                        WriteReal(l, color);
                    }
                }
                else
                {
                    WriteReal(line, color);
                }
            }

            // tell main thread we wrote stuff
#pragma warning disable 0420
            Interlocked.Increment(ref changed);
#pragma warning restore 0420
        }

        public static void Write(string line, string colorHexCode)
        {
            lock (Mutex)
            {
                if (line.Contains("\r") || line.Contains("\n"))
                {
                    foreach (string l in Regex.Split(line, "[\r\n]+"))
                    {
                        WriteReal(l, Color.white);
                    }
                }
                else
                {
                    line = "<color=" + colorHexCode + ">" + line + "</color>";
                    WriteReal(line, Color.white);
                }
            }

            // tell main thread we wrote stuff
#pragma warning disable 0420
            Interlocked.Increment(ref changed);
#pragma warning restore 0420
        }

        /// <summary>
        /// Write one line to the console
        /// </summary>
        /// <param name="line">Text to write</param>
        /// <example>
        /// *Example:* Writing a custom message to the console.
        /// 
        /// ```csharp
        /// void OnSpawn() {
        ///   NetConsole.Write("[Spawn:" + this.gameObject.name + "]);
        /// }
        /// ```
        /// </example>
        public static void Write(string line)
        {
            Write(line, Color.white);
        }

        public static void WriteReal(string line, Color color)
        {
            // free one slot up
            if (lines.Full) { lines.Dequeue(); }

            // put line 
            lines.Enqueue(new Line { text = line, color = color });
        }

        void Awake()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    fontSize *= 2;
                    lineHeight *= 2;
                    break;
            }
        }

        static public void Clear()
        {
            lock (Mutex)
            {
                lines.Clear();
                linesRender.Clear();
            }
        }

        static public void LinesRefresh()
        {
            // update if we have changed
            if (changed > 0)
            {
                int c = changed;

                do
                {
                    c = changed;

                    lock (Mutex)
                    {
                        lines.CopyTo(linesRender);
                    }

#pragma warning disable 0420
                } while (Interlocked.Add(ref changed, -c) > 0);
#pragma warning restore 0420
            }
        }

        void OnGUI()
        {
            if ((UnityEngine.Event.current.type == EventType.KeyDown) && (UnityEngine.Event.current.keyCode == toggleKey))
            {
                visible = !visible;
            }

            if (visible == false)
            {
                return;
            }

            LinesRefresh();

            // how many lines to render at most
            int lines = Mathf.Max(1, ((int)(Screen.height * consoleHeight)) / lineHeight);

            // background
            DebugInfo.DrawBackground(new Rect(inset, inset, Screen.width - (inset * 2), ((lines - 1) * lineHeight) + (padding * 2)));

            // draw lines
            for (int i = 0; i < lines; ++i)
            {
                int m = Mathf.Min(linesRender.Count, (lines - 1));

                if (i < linesRender.Count)
                {
                    Line l = linesRender[linesRender.Count - m + i];
                    GUIStyle s = DebugInfo.LabelStyleColor(l.color);
                    s.fontSize = fontSize;

                    GUI.Label(GetRect(i), l.text, s);
                }
            }
        }

        Rect GetRect(int line)
        {
            return new Rect(inset + padding, inset + padding + (line * lineHeight), Screen.width - (inset * 2) - (padding * 2), lineHeight);
        }
    }

}
