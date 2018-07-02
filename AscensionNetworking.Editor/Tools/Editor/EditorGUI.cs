using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ascension.Compiler;
using AC = Ascension.Compiler;

namespace Ascension.Tools
{
    public static class EditorGUI
    {
        public const int HEADER_HEIGHT = 23;
        public const int GLOBAL_INSET = 1;

        public static readonly Color Blue = new Color(0 / 255f, 162f / 255f, 232f / 255f);
        public static readonly Color LightBlue = new Color(0f / 255f, 232f / 255f, 226f / 255f);
        public static readonly Color Orange = new Color(255f / 255f, 127f / 255f, 39f / 255f);
        public static readonly Color LightGreen = new Color(105f / 255f, 251f / 255f, 9f / 255f);
        public static readonly Color DarkGreen = new Color(34f / 255f, 177f / 255f, 76f / 255f);
        public static readonly Color LightOrange = new Color(255f / 255f, 201f / 255f, 12f / 255f);

        public static AxisSelections EditAxisSelection(AxisSelections value)
        {
            var values = Enum.GetValues(typeof(AxisSelections)).Cast<AxisSelections>().ToArray();
            var names = values.Select(x => x.ToString()).ToArray();
            return values[EditorGUILayout.Popup(Array.IndexOf(values, value), names)];
        }

        public static void EditAxes(FloatCompression[] compression, AxisSelections selection)
        {
            EditorGUILayout.BeginVertical();
            compression[AC.Axis.X] = ExitAxis(compression[AC.Axis.X], "X", (selection & AxisSelections.X) == AxisSelections.X);
            compression[AC.Axis.Y] = ExitAxis(compression[AC.Axis.Y], "Y", (selection & AxisSelections.Y) == AxisSelections.Y);
            compression[AC.Axis.Z] = ExitAxis(compression[AC.Axis.Z], "Z", (selection & AxisSelections.Z) == AxisSelections.Z);
            EditorGUILayout.EndVertical();
        }

        static FloatCompression ExitAxis(FloatCompression compression, string label, bool enabled)
        {
            if (enabled)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(label, GUILayout.Width(15));
                compression = EditFloatCompression(compression);
                EditorGUILayout.EndHorizontal();
            }

            return compression;
        }

        public static void EditSmoothingAlgorithm(AssetDefinition adef, PropertyDefinition pdef)
        {
            EditSmoothingAlgorithm(adef, pdef, true);
        }

        public static void EditSmoothingAlgorithm(AssetDefinition adef, PropertyDefinition pdef, bool allowExtrapolation)
        {
            if (adef is StateDefinition)
            {
                if (allowExtrapolation)
                {
                    WithLabel("Smoothing Algorithm", () =>
                    {
                        pdef.StateAssetSettings.SmoothingAlgorithm = (SmoothingAlgorithms)EditorGUILayout.EnumPopup(pdef.StateAssetSettings.SmoothingAlgorithm);
                    });
                }
                else
                {
                    WithLabel("Smoothing Algorithm", () =>
                    {
                        if (ToggleDropdown(SmoothingAlgorithms.Interpolation.ToString(), SmoothingAlgorithms.None.ToString(), pdef.StateAssetSettings.SmoothingAlgorithm != SmoothingAlgorithms.None))
                        {
                            pdef.StateAssetSettings.SmoothingAlgorithm = SmoothingAlgorithms.Interpolation;
                        }
                        else
                        {
                            pdef.StateAssetSettings.SmoothingAlgorithm = SmoothingAlgorithms.None;
                        }
                    });
                }

                if (pdef.StateAssetSettings.SmoothingAlgorithm == SmoothingAlgorithms.Extrapolation)
                {
                    if (pdef.PropertyType is PropertyTypeTransform)
                    {
                        PropertyTypeTransform transform = (PropertyTypeTransform)pdef.PropertyType;

                        WithLabel("Extrapolation Velocity", () =>
                        {
                            transform.ExtrapolationVelocityMode = (ExtrapolationVelocityModes)EditorGUILayout.EnumPopup(transform.ExtrapolationVelocityMode);
                        });
                    }

                    WithLabel("Extrapolation Settings", () =>
                    {
                        pdef.StateAssetSettings.ExtrapolationMaxFrames = IntFieldOverlay(pdef.StateAssetSettings.ExtrapolationMaxFrames, "Max Frames");
                        pdef.StateAssetSettings.ExtrapolationErrorTolerance = FloatFieldOverlay(pdef.StateAssetSettings.ExtrapolationErrorTolerance, "Error Tolerance");
                    });
                }
            }
        }

        public static void PropertyTypePopup(AssetDefinition asset, PropertyDefinition definition, params GUILayoutOption[] options)
        {
            if (!asset.AllowedPropertyTypes.Contains(definition.PropertyType.GetType()))
            {
                definition.PropertyType = new PropertyTypeFloat();
            }

            definition.PropertyType = PropertyTypePopup(asset.AllowedPropertyTypes, definition.PropertyType, options);
        }

        public static PropertyType PropertyTypePopup(IEnumerable<Type> allTypes, PropertyType current, params GUILayoutOption[] options)
        {
            if (current == null)
            {
                current = (PropertyType)Activator.CreateInstance(allTypes.First());
                current.OnCreated();
            }

            var types = allTypes.OrderBy(x => x.Name.Replace("PropertyType", "")).ToArray();
            var typesNames = types.Select(x => x.Name.Replace("PropertyType", "")).ToArray();
            var selected = Array.IndexOf(types, current.GetType());
            var selectedNew = EditorGUILayout.Popup(selected, typesNames, options);

            if (selected != selectedNew)
            {
                current = (PropertyType)Activator.CreateInstance(types[selectedNew]);
                current.OnCreated();
            }

            return current;
        }

        public static FloatCompression EditFloatCompression(FloatCompression c)
        {
            if (c == null)
            {
                c = FloatCompression.Default();
            }

            c.Enabled = Toggle(c.Enabled);

            UnityEditor.EditorGUI.BeginDisabledGroup(!c.Enabled);
            c.MinValue = Mathf.Min(IntFieldOverlay(c.MinValue, "Min"), c.MaxValue - 1);
            c.MaxValue = Mathf.Max(IntFieldOverlay(c.MaxValue, "Max"), c.MinValue + 1);
            c.Accuracy = Mathf.Max(FloatFieldOverlay(c.Accuracy, "Accuracy"), 0.001f);
            GUILayout.Label("Bits: " + (c.Enabled ? c.BitsRequired : 32), EditorStyles.miniLabel, GUILayout.Width(50));
            UnityEditor.EditorGUI.EndDisabledGroup();

            return c;
        }

        public static int EditPriority(int priority, bool enabled)
        {
            if (enabled)
            {
                priority = Mathf.Clamp(IntFieldOverlay(priority, "Priority", GUILayout.Width(75)), 1, 999);
            }
            else
            {
                Disabled(() => { EditorGUILayout.TextField("N/A", GUILayout.Width(75)); });
            }

            return priority;
        }

        public static Guid AssetPopup(IEnumerable<AssetDefinition> assets, Guid current, IEnumerable<Guid> exclude)
        {
            return AssetPopup("", assets, current, exclude);
        }

        public static Guid AssetPopup(string prefix, IEnumerable<AssetDefinition> assets, Guid current, IEnumerable<Guid> exclude)
        {
            var filtered = assets.Where(x => !exclude.Contains(x.Guid)).ToArray();
            var options = (new string[] { prefix + "NONE" }).Concat(filtered.Select(x => prefix + x.Name)).ToArray();
            var selected = Array.FindIndex(filtered, x => x.Guid == current) + 1;

            selected = EditorGUILayout.Popup(selected, options);

            if (selected == 0)
            {
                return Guid.Empty;
            }

            return filtered[selected - 1].Guid;
        }

        public static void SetWindowTitle(this EditorWindow editor, string title, Texture icon)
        {
            typeof(EditorWindow).GetField("m_CachedTitleContent", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(editor, new GUIContent(title, icon));
        }

        public static bool DeleteDialog()
        {
            return EditorUtility.DisplayDialog("Confirm", "Do you want to delete this item?", "Yes", "No");
        }

        public static void Disabled(Action gui)
        {
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            gui();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }

        public static GUIStyle BoxStyle(int n)
        {
            GUIStyle s = new GUIStyle("flow node " + n);
            s.padding = new RectOffset(4, 4, 4, 4);
            return s;
        }

        public static string Tooltip = "";

        public static Color HighlightColor
        {
            get
            {
                return Color.white;
            }
        }

        public static Color ToUnityColor(this Color4 c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        public static Color4 ToAscensionColor(this Color c)
        {
            return new Color4(c.r, c.g, c.b, c.a);
        }

        public static void SetTooltip(string tooltip)
        {
            Rect r = GUILayoutUtility.GetLastRect();

            if (r.Contains(Event.current.mousePosition))
            {
                Tooltip = tooltip;
            }
        }

        public static void WithLabel(string label, Action gui)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(220));

            gui();

            GUILayout.EndHorizontal();
        }

        public static int IntFieldOverlay(int value, string overlay, params GUILayoutOption[] options)
        {
            GUIStyle f = new GUIStyle("TextField");
            value = EditorGUILayout.IntField(value, f, options);

            GUIStyle l = new GUIStyle(EditorStyles.miniLabel);
            l.alignment = TextAnchor.MiddleRight;
            l.contentOffset = new Vector2(-2, 0);
            l.normal.textColor = Color.gray;

            GUI.Label(GUILayoutUtility.GetLastRect(), overlay, l);
            return value;
        }

        public static float FloatFieldOverlay(float value, string overlay, params GUILayoutOption[] options)
        {
            GUIStyle f = new GUIStyle("TextField");
            value = EditorGUILayout.FloatField(value, f, options);

            GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
            s.alignment = TextAnchor.MiddleRight;
            s.contentOffset = new Vector2(-2, 0);
            s.normal.textColor = Color.gray;

            GUI.Label(GUILayoutUtility.GetLastRect(), overlay, s);
            return value;
        }

        public static string TextFieldOverlay(string value, string overlay, params GUILayoutOption[] options)
        {
            GUIStyle f = new GUIStyle("TextField");
            value = EditorGUILayout.TextField(value, f, options);

            GUIStyle l = new GUIStyle(EditorStyles.miniLabel);
            l.alignment = TextAnchor.MiddleRight;
            l.contentOffset = new Vector2(-2, 0);
            l.normal.textColor = Color.gray;

            GUI.Label(GUILayoutUtility.GetLastRect(), overlay, l);
            return value;
        }

        static GUIStyle PaddingStyle(int left, int right, int top, int bottom)
        {
            GUIStyle s = new GUIStyle(GUIStyle.none);
            s.padding = new RectOffset(left, right, top, bottom);
            return s;
        }

        public static void Icon(string icon, float alpha)
        {
            GUIStyle style;
            style = new GUIStyle(GUIStyle.none);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.margin = new RectOffset(0, 0, 0, 0);
            style.contentOffset = new Vector2(0, 0);

            Color c;

            c = HighlightColor;
            c.a = alpha;

            GUI.color = c;
            GUILayout.Button(LoadIcon(icon), style, GUILayout.Width(16), GUILayout.Height(16));
            GUI.color = Color.white;
        }

        public static void Icon(string icon)
        {
            Icon(icon, 1f);
        }

        public static bool IconButton(string icon, float alpha)
        {
            GUIStyle style;
            style = new GUIStyle(GUIStyle.none);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.margin = new RectOffset(0, 0, 0, 0);
            style.contentOffset = new Vector2(0, 0);

            Color c;

            c = HighlightColor;
            c.a = alpha;

            GUI.color = c;
            bool result = GUILayout.Button(LoadIcon(icon), style, GUILayout.Width(16), GUILayout.Height(16));
            GUI.color = Color.white;

            return result;
        }

        public static bool IconButton(string icon)
        {
            return IconButton(icon, 1f);
        }

        public static bool IconButton(string icon, bool enabled)
        {
            return IconButton(icon, enabled ? 1f : 0.25f) && enabled;
        }

        public static bool Toggle(string on, string off, bool enabled)
        {
            return IconButton(enabled ? on : off);
        }

        public static bool Toggle(string icon, bool enabled)
        {
            if (IconButton(icon, enabled ? 1f : 0.25f))
            {
                return !enabled;
            }

            return enabled;
        }

        static GUIStyle SettingSectionStyle
        {
            get
            {
                GUIStyle s = new GUIStyle(GUIStyle.none);
                s.padding = new RectOffset();
                s.margin = new RectOffset(0, 0, 3, 0);

                return s;
            }
        }

        public static bool ToggleDropdown(string on, string off, bool enabled)
        {
            return EditorGUILayout.Popup(enabled ? 0 : 1, new[] { on, off }) == 0;
        }

        public static bool ToggleButton(string on, string off, bool enabled, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(enabled ? on : off, EditorStyles.miniButton, options))
            {
                return !enabled;
            }

            return enabled;
        }

        public static bool Toggle(bool value)
        {
            GUIStyle toggle = new GUIStyle("Toggle");
            toggle.margin = new RectOffset();
            toggle.padding = new RectOffset();
            return EditorGUILayout.Toggle(value, toggle, GUILayout.Width(15));
        }

        public static void Header(string text)
        {
            EditorGUILayout.BeginHorizontal(PaddingStyle(5, 0, 0, 0));

            GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
            s.margin.top = 0;
            GUILayout.Label(text, s);

            EditorGUILayout.EndHorizontal();
        }

        public static void Header(string text, string icon)
        {
            EditorGUILayout.BeginHorizontal(PaddingStyle(5, 0, 0, 0));

            Icon(icon);

            GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
            s.margin.top = 0;
            GUILayout.Label(text, s);

            EditorGUILayout.EndHorizontal();
        }

        public static GUIStyle InheritanceSeparatorStyle
        {
            get
            {
                GUIStyle s;
                s = new GUIStyle(EditorStyles.label);
                s.padding = new RectOffset(4, 2, 0, 0);
                s.contentOffset = new Vector2(0, 0);
                s.normal.textColor = Color.white;
                return s;
            }
        }

        public static GUIStyle MiniLabelButtonStyle
        {
            get
            {
                GUIStyle s;
                s = new GUIStyle(EditorStyles.miniLabel);
                s.normal.textColor = Color.white;
                s.alignment = TextAnchor.MiddleRight;
                return s;
            }
        }

        public static GUIStyle MiniLabelWithColor(Color c)
        {
            GUIStyle s;
            s = new GUIStyle(EditorStyles.miniLabel);
            s.normal.textColor = c;
            return s;
        }

        public static int ClickCount
        {
            get { return Event.current.clickCount; }
        }

        public static bool IsLeftClick
        {
            get { return Event.current.type == EventType.MouseDown && Event.current.button == 0; }
        }

        public static bool IsRightClick
        {
            get { return Event.current.type == EventType.MouseDown && Event.current.button == 1; }
        }

        public static bool WasKeyPressed(KeyCode key)
        {
            return Event.current.type == EventType.KeyDown && Event.current.keyCode == key;
        }

        public static void UseEvent()
        {
            if (Event.current != null)
            {
                Event.current.Use();
            }
        }

        public static string EditComment(string comment)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("//", InheritanceSeparatorStyle, GUILayout.Width(15));
            comment = EditorGUILayout.TextArea(comment);
            GUILayout.EndHorizontal();

            return comment;
        }

        public static Texture2D LoadIcon(string name)
        {
            return Resources.Load("Icons/" + name, typeof(Texture2D)) as Texture2D;
        }

        public static GUIStyle HeaderBackground
        {
            get
            {
                GUIStyle s;

                s = new GUIStyle();
                s.padding = new RectOffset(3, 3, 3, 0);
                s.border = new RectOffset(3, 3, 3, 3);
                s.margin = new RectOffset();

                if (EditorGUIUtility.isProSkin)
                {
                    s.normal.background = Resources.Load("Backgrounds/dark_slider", typeof(Texture2D)) as Texture2D;
                    s.hover.background = Resources.Load("Backgrounds/dark_slider_hover", typeof(Texture2D)) as Texture2D;
                }
                else
                {
                    s.normal.background = Resources.Load("Backgrounds/dark_slider", typeof(Texture2D)) as Texture2D;
                    s.hover.background = Resources.Load("Backgrounds/dark_slider_hover", typeof(Texture2D)) as Texture2D;
                }

                return s;
            }
        }
    }

}

