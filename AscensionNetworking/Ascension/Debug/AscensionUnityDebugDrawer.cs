using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ascension.Networking
{
    public class AscensionUnityDebugDrawer : AscensionNetworkInternal.IDebugDrawer
    {
        bool isEditor;

        void AscensionNetworkInternal.IDebugDrawer.IsEditor(bool isEditor)
        {
            this.isEditor = isEditor;
        }

        void AscensionNetworkInternal.IDebugDrawer.SelectGameObject(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (!isEditor)
            {
                UnityEditor.Selection.activeGameObject = gameObject;
            }
#endif
        }

        void AscensionNetworkInternal.IDebugDrawer.Indent(int level)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                UnityEditor.EditorGUI.indentLevel = level;
                return;
            }
#endif
        }

        void AscensionNetworkInternal.IDebugDrawer.Label(string text)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                GUILayout.Label(text);
                return;
            }
#endif

            DebugInfo.Label(text);
        }

        void AscensionNetworkInternal.IDebugDrawer.LabelBold(string text)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                GUILayout.Label(text, EditorStyles.boldLabel);
                return;
            }
#endif

            DebugInfo.LabelBold(text);
        }

        void AscensionNetworkInternal.IDebugDrawer.LabelField(string text, object value)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                UnityEditor.EditorGUILayout.LabelField(text, value.ToString());
                return;
            }
#endif

            DebugInfo.LabelField(text, value);
        }

        void AscensionNetworkInternal.IDebugDrawer.Separator()
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                UnityEditor.EditorGUILayout.Separator();
                return;
            }
#endif

            GUILayout.Space(2);
        }

    }
}