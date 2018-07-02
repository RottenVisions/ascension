using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Ascension.Networking.Sockets;

namespace Ascension.Networking
{
    public static class AscensionNetworkInternal
    {
        public static bool UsingUnityPro;

        public static IDebugDrawer DebugDrawer;

        public static Action EnvironmentSetup;
        public static Action EnvironmentReset;
        public static Func<int, string> GetSceneName;
        public static Func<string, int> GetSceneIndex;
        public static Func<List<NetTuple<GlobalBehaviorAttribute, Type>>> GetGlobalBehaviourTypes;

        public static void Initialize(NetworkModes mode, IPEndPoint endPoint, string autoloadScene, RuntimeSettings config)
        {
            Core.Initialize(mode, endPoint, config, autoloadScene);
        }

        public static void Shutdown()
        {
            Core.Shutdown();
        }

        public interface IDebugDrawer
        {
            void IsEditor(bool isEditor);
            void Indent(int adjust);
            void Label(string text);
            void LabelBold(string text);
            void LabelField(string text, object value);
            void Separator();
            void SelectGameObject(GameObject gameObject);
        }
    }

    public static class AscensionCoreInternal
    {
        public static readonly List<AscensionEntity> ChangedEditorEntities = new List<AscensionEntity>();
    }
}
