using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.Collections;

namespace Ascension.Networking.Sockets
{
    [CustomEditor(typeof(NetworkDiscovery), true)]
    [CanEditMultipleObjects]
    public class NetworkDiscoveryEditor : Editor
    {
        bool initialized;
        NetworkDiscovery discovery;

        SerializedProperty broadcastPortProperty;
        SerializedProperty broadcastKeyProperty;
        SerializedProperty broadcastVersionProperty;
        SerializedProperty broadcastSubVersionProperty;
        SerializedProperty broadcastDataProperty;

        SerializedProperty serverPortProperty;
        SerializedProperty messageProperty;
        SerializedProperty extraProperty;
        SerializedProperty broadcastTimeout;

        void Init()
        {
            if (initialized)
                return;

            initialized = true;
            discovery = target as NetworkDiscovery;

            broadcastPortProperty = serializedObject.FindProperty("broadcastingPort");
            broadcastKeyProperty = serializedObject.FindProperty("broadcastKey");
            broadcastVersionProperty = serializedObject.FindProperty("broadcastVersion");
            broadcastSubVersionProperty = serializedObject.FindProperty("broadcastSubversion");
            broadcastDataProperty = serializedObject.FindProperty("broadcastData");
            broadcastTimeout = serializedObject.FindProperty("broadcastTimeout");

            serverPortProperty = serializedObject.FindProperty("serverPort");
            messageProperty = serializedObject.FindProperty("message");
            extraProperty = serializedObject.FindProperty("extra");
        }

        public override void OnInspectorGUI()
        {
            Init();
            serializedObject.Update();
            DrawControls();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawControls()
        {
            if (discovery == null)
                return;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(broadcastPortProperty);

            EditorGUILayout.PropertyField(broadcastKeyProperty);
            EditorGUILayout.PropertyField(broadcastVersionProperty);
            EditorGUILayout.PropertyField(broadcastSubVersionProperty);
            
            EditorGUILayout.PropertyField(broadcastTimeout);

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(serverPortProperty);
            EditorGUILayout.PropertyField(messageProperty);
            EditorGUILayout.PropertyField(extraProperty);

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(broadcastDataProperty);

            EditorGUILayout.Separator();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            discovery.dontDestroyClientOnLoad = EditorGUILayout.Toggle("Prevent Client Destruction? ", discovery.dontDestroyClientOnLoad);

            if (Application.isPlaying)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Broadcast Host ID", discovery.broadcastHostId.ToString());
                EditorGUILayout.LabelField("Running? ", discovery.broadcasting.ToString());
                EditorGUILayout.LabelField("Is Server?", discovery.broadcastingAsServer.ToString());
                EditorGUILayout.LabelField("Is Client?", (!discovery.broadcastingAsServer).ToString());
            }            
        }
    }
}
