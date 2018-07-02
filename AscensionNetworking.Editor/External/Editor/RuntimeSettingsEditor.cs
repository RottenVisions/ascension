using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Ascension.Networking;
using Ascension.Networking.Sockets;
using UnityEngine.Networking;
using DEditorGUI = Ascension.Tools.EditorGUI;

namespace Ascension.Tools
{
    public class RuntimeSettingsEditor : EditorWindow
    {
        public static readonly Color Blue = new Color(0 / 255f, 162f / 255f, 232f / 255f);
        public static readonly Color LightBlue = new Color(0f / 255f, 232f / 255f, 226f / 255f);
        public static readonly Color Orange = new Color(255f / 255f, 127f / 255f, 39f / 255f);
        public static readonly Color LightGreen = new Color(105f / 255f, 251f / 255f, 9f / 255f);
        public static readonly Color DarkGreen = new Color(34f / 255f, 177f / 255f, 76f / 255f);
        public static readonly Color LightOrange = new Color(255f / 255f, 201f / 255f, 12f / 255f);

        private Vector2 scrollPos = Vector2.zero;

        private void Header(string text, string icon)
        {
            DEditorGUI.Header(text, icon);
        }

        //Gives a style to the header
        void HeaderWithStyle(string name, string icon)
        {
            GUILayout.BeginHorizontal(DEditorGUI.HeaderBackground, GUILayout.Height(DEditorGUI.HEADER_HEIGHT));
            Header(name, icon);
            GUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            HeaderWithStyle("Network", "network");
            Replication();

            HeaderWithStyle("Simulation", "simulation");
            Simulation();

            //HeaderWithStyle("Configuration", "configuration");
            //Settings();

            //HeaderWithStyle("Topology", "topology");
            //Topology();

            //HeaderWithStyle("Global Configuration", "global-configuration");
            //GlobalConfig();

            //HeaderWithStyle("Custom", "custom");
            //Custom();

            HeaderWithStyle("Defaults", "defaults");
            Defaults();

            HeaderWithStyle("Extras", "extras");
            Extras();

            HeaderWithStyle("Compiler", "compiler");
            Compiler();

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                Save();
            }
        }

        private void Save()
        {
            EditorUtility.SetDirty(RuntimeSettings.Instance);
            AssetDatabase.SaveAssets();
        }

        private void Settings()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Acknowledgement Delay", () => {
                settings.ackDelay = (uint)DEditorGUI.IntFieldOverlay((int)settings.ackDelay, "ms");
                DEditorGUI.SetTooltip("How long in ms receiver will wait before it will force send acknowledgements back without waiting for any payload");
            });

            DEditorGUI.WithLabel("Are Acknowledgements Long? ", () => {
                settings.isAcksLong = DEditorGUI.ToggleButton("Yes", "No", settings.isAcksLong);
            });

            DEditorGUI.WithLabel("All Cost Timeout", () =>
            {
                settings.allCostTimeout = (uint)DEditorGUI.IntFieldOverlay((int)settings.allCostTimeout, "ms");
                DEditorGUI.SetTooltip("How long in ms receiver will wait before it will force send acknowledgements back without waiting for any payload");
            });

            DEditorGUI.WithLabel("Connect Timeout", () => {
                settings.connectTimeout = (uint)DEditorGUI.IntFieldOverlay((int)settings.connectTimeout, "ms");
                DEditorGUI.SetTooltip("Who can send this as an global event?");
            });

            DEditorGUI.WithLabel("Disconnect Timeout", () => {
                settings.disconnectTimeout = (uint)DEditorGUI.IntFieldOverlay((int)settings.disconnectTimeout, "ms");
            });

            DEditorGUI.WithLabel("Fragment Size", () => {
                settings.fragmentSize = (ushort)DEditorGUI.IntFieldOverlay(settings.fragmentSize, "bytes");
            });

            DEditorGUI.WithLabel("Max Combined Reliable Message Count", () => {
                settings.maxCombinedReliableMessageCount = (ushort)DEditorGUI.IntFieldOverlay(settings.maxCombinedReliableMessageCount, "");
            });

            DEditorGUI.WithLabel("Max Combined Reliable Message Size", () => {
                settings.maxCombinedReliableMessageSize = (ushort)DEditorGUI.IntFieldOverlay(settings.maxCombinedReliableMessageSize, "");
            });

            DEditorGUI.WithLabel("Max Connection Attempts", () => {
                settings.maxConnectionAttempt = (byte)DEditorGUI.IntFieldOverlay(settings.maxConnectionAttempt, "");
            });

            DEditorGUI.WithLabel("Max Sent Message Queue Size", () => {
                settings.maxSentMessageQueueSize = (ushort)DEditorGUI.IntFieldOverlay(settings.maxSentMessageQueueSize, "");
            });

            DEditorGUI.WithLabel("Min Update Timeout", () => {
                settings.minUpdateTimeout = (uint)DEditorGUI.IntFieldOverlay((int)settings.minUpdateTimeout, "ms");
            });

            DEditorGUI.WithLabel("Network Drop Threshold", () => {
                settings.networkDropThreshold = (byte)DEditorGUI.IntFieldOverlay(settings.networkDropThreshold, "%");
            });

            DEditorGUI.WithLabel("Overflow Drop Threshold", () => {
                settings.overflowDropThreshold = (byte)DEditorGUI.IntFieldOverlay(settings.overflowDropThreshold, "%");
            });

            DEditorGUI.WithLabel("Packet Size", () => {
                settings.packetSize = (ushort)DEditorGUI.IntFieldOverlay(settings.packetSize, "bytes");
            });

            DEditorGUI.WithLabel("Ping Timeout", () => {
                settings.pingTimeout = (uint)DEditorGUI.IntFieldOverlay((int)settings.pingTimeout, "ms");
            });

            DEditorGUI.WithLabel("Reduced Ping Timeout", () => {
                settings.reducedPingTimeout = (uint)DEditorGUI.IntFieldOverlay((int)settings.reducedPingTimeout, "ms");
            });

            DEditorGUI.WithLabel("Resend Timeout", () => {
                settings.resendTimeout = (uint)DEditorGUI.IntFieldOverlay((int)settings.resendTimeout, "ms");
            });
        }

        void Custom()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Max Network Event Data Size", () =>
            {
                settings.networkEventMaxSize = DEditorGUI.IntFieldOverlay(settings.networkEventMaxSize, "bytes");
            });

            DEditorGUI.WithLabel("Total Network Event Count", () =>
            {
                settings.totalNetworkEventCount = DEditorGUI.IntFieldOverlay(settings.totalNetworkEventCount, "");
            });
        }

        void Topology()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Message Pool Size Growth Factor", () => {
                settings.messagePoolSizeGrowthFactor = DEditorGUI.FloatFieldOverlay(settings.messagePoolSizeGrowthFactor, "");
            });

            DEditorGUI.WithLabel("Received Message Pool Size", () => {
                settings.receivedMessagePoolSize = (ushort)DEditorGUI.IntFieldOverlay(settings.receivedMessagePoolSize, "bytes");
            });

            DEditorGUI.WithLabel("Sent Message Pool Size", () => {
                settings.sentMessagePoolSize = (ushort)DEditorGUI.IntFieldOverlay(settings.sentMessagePoolSize, "");
            });
        }

        void GlobalConfig()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Reactor Model Type? ", () =>
            {
                settings.reactorModel = (ReactorModel)EditorGUILayout.EnumPopup(settings.reactorModel);
            });

            DEditorGUI.WithLabel("Thread Awake Timeout", () => {
                settings.threadAwakeTimeout = (uint)DEditorGUI.FloatFieldOverlay(settings.threadAwakeTimeout, "??");
            });
        }

        void Replication()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Simulation Rate", () => {
                settings.framesPerSecond = DEditorGUI.IntFieldOverlay(settings.framesPerSecond, "FixedUpdate Calls / Second");
            });

            DEditorGUI.WithLabel("Network Rate", () => {
                settings.serverSendRate = Mathf.Clamp(settings.serverSendRate, 1, settings.framesPerSecond);

                var ssr = settings.serverSendRate;
                var fps = settings.framesPerSecond;

                string legend = "";

                if (fps == ((fps / ssr) * ssr))
                {
                    legend = (fps / ssr).ToString();
                }
                else
                {
                    legend = System.Math.Round((float)fps / (float)ssr, 2).ToString();
                }

                settings.serverSendRate = Mathf.Clamp(DEditorGUI.IntFieldOverlay(settings.serverSendRate, string.Format("{0} Packets / Second", legend)), 1, fps);
            });

            DEditorGUI.WithLabel("Sendrate Throttling? ", () =>
            {
                settings.allowSendRateThrottling = DEditorGUI.ToggleButton("On", "Off", settings.allowSendRateThrottling);
            });

            DEditorGUI.WithLabel("Disable Dejitter Buffer", () => {
                settings.disableDejitterBuffer = EditorGUILayout.Toggle(settings.disableDejitterBuffer);
            });

            UnityEditor.EditorGUI.BeginDisabledGroup(settings.disableDejitterBuffer);

            DEditorGUI.WithLabel("Dejitter Delay", () => {
                settings.serverDejitterDelayMin = Mathf.Max(0, DEditorGUI.IntFieldOverlay(settings.serverDejitterDelayMin, "Min"));
                settings.serverDejitterDelay = Mathf.Max(1, DEditorGUI.IntFieldOverlay(settings.serverDejitterDelay, "Frames"));
                settings.serverDejitterDelayMax = Mathf.Max(settings.serverDejitterDelay + 1, DEditorGUI.IntFieldOverlay(settings.serverDejitterDelayMax, "Max"));
            });

            UnityEditor.EditorGUI.EndDisabledGroup();

            DEditorGUI.WithLabel("Scoping Mode", () => {
                ScopeMode previous = settings.scopeMode;
                settings.scopeMode = (ScopeMode)EditorGUILayout.EnumPopup(settings.scopeMode);

                if (previous != settings.scopeMode)
                {
                    settings.scopeModeHideWarningInGui = false;
                    Save();
                }
            });

            DEditorGUI.WithLabel("Instantiate Mode", () => {
                settings.clientCanInstantiateAll = DEditorGUI.ToggleDropdown("Client Can Instantiate Everything", "Individual Setting On Each Prefab", settings.clientCanInstantiateAll);
            });

            if ((settings.scopeMode == ScopeMode.Manual) && (settings.scopeModeHideWarningInGui == false))
            {
                EditorGUILayout.HelpBox("When manual scoping is enabled you are required to call AscensionEntity.SetScope for each connection that should receive a replicated copy of the entity.", UnityEditor.MessageType.Warning);

                if (GUILayout.Button("I understand, hide this warning", EditorStyles.miniButton))
                {
                    settings.scopeModeHideWarningInGui = true;
                    Save();
                }
            }

            DEditorGUI.WithLabel("Override Timescale? ", () => {
                settings.overrideTimeScale = DEditorGUI.ToggleButton("On", "Off", settings.overrideTimeScale);
            });

            DEditorGUI.WithLabel("Max Connections", () => {
                settings.maxDefaultConnections = DEditorGUI.IntFieldOverlay(settings.maxDefaultConnections, "");
            });

            //Set the client and server settings the same
            settings.clientSendRate = settings.serverSendRate;
            settings.clientDejitterDelay = settings.serverDejitterDelay;
            settings.clientDejitterDelayMin = settings.serverDejitterDelayMin;
            settings.clientDejitterDelayMax = settings.serverDejitterDelayMax;
        }

        void Defaults()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Use Default Settings? ", () => {
                settings.useDefaults = DEditorGUI.ToggleButton("On", "Off", settings.useDefaults);
            });

            DEditorGUI.WithLabel("Log Targets", () => {
                settings.logTargets = (ConfigLogTargets)EditorGUILayout.EnumMaskField(settings.logTargets);
            });

            DEditorGUI.WithLabel("Show Debug Info", () => {
                settings.showDebugInfo = EditorGUILayout.Toggle(settings.showDebugInfo);
            });

            DEditorGUI.WithLabel("Log Unity To Console", () => {
                settings.logUncaughtExceptions = EditorGUILayout.Toggle(settings.logUncaughtExceptions);
            });

            var consoleEnabled = (settings.logTargets & ConfigLogTargets.Console) == ConfigLogTargets.Console;
            UnityEditor.EditorGUI.BeginDisabledGroup(consoleEnabled == false);

            EditorGUILayout.BeginVertical();

            DEditorGUI.WithLabel("Toggle Key", () => {
                settings.consoleToggleKey = (KeyCode)EditorGUILayout.EnumPopup(settings.consoleToggleKey);
            });

            DEditorGUI.WithLabel("Visible By Default", () => {
                settings.consoleVisibleByDefault = EditorGUILayout.Toggle(settings.consoleVisibleByDefault);
            });

            UnityEditor.EditorGUI.EndDisabledGroup();

            DEditorGUI.WithLabel("Accept Mode", () => {
                settings.serverConnectionAcceptMode = (ConnectionAcceptMode)EditorGUILayout.EnumPopup(settings.serverConnectionAcceptMode);
            });

            DEditorGUI.WithLabel("Show Ascension Entity Hints? ", () => {
                settings.showAscensionEntityHints = DEditorGUI.ToggleButton("On", "Off", settings.showAscensionEntityHints);
            });

            DEditorGUI.WithLabel("Remotes: Count LLAPI Messages?", () => {
                settings.lowLevelMessagesIncrementBandwidth = DEditorGUI.ToggleButton("Yes", "No", settings.lowLevelMessagesIncrementBandwidth);
            });

            EditorGUILayout.EndVertical();
        }

        void Simulation()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;
            EditorGUILayout.BeginVertical();

            if (Core.IsDebugMode == false)
            {
                EditorGUILayout.HelpBox("Ascension is in release mode, these settings have no effect", UnityEditor.MessageType.Warning);
            }

            UnityEditor.EditorGUI.BeginDisabledGroup(Core.IsDebugMode == false);

            DEditorGUI.WithLabel("Enabled", () =>
            {
                settings.useNetworkSimulation = EditorGUILayout.Toggle(settings.useNetworkSimulation);
            });

            UnityEditor.EditorGUI.EndDisabledGroup();
            UnityEditor.EditorGUI.BeginDisabledGroup(settings.useNetworkSimulation == false || Core.IsDebugMode == false);

            DEditorGUI.WithLabel("Packet Loss", () =>
            {
                settings.simulatePacketLoss = EditorGUILayout.Toggle(settings.simulatePacketLoss);
            });

            DEditorGUI.WithLabel("Latency", () =>
            {
                settings.simulateLatency = EditorGUILayout.Toggle(settings.simulateLatency);
            });

            DEditorGUI.WithLabel("Packet Loss", () =>
            {
                int loss;

                loss = Mathf.Clamp(Mathf.RoundToInt(settings.simulatedLoss * 100), 0, 100);
                loss = DEditorGUI.IntFieldOverlay(loss, "Percent");

                settings.simulatedLoss = Mathf.Clamp01(loss / 100f);
            });

            DEditorGUI.WithLabel("Max Latency", () =>
            {
                settings.simulationMaxLatency = DEditorGUI.IntFieldOverlay(settings.simulationMaxLatency, "100 ms");
            });

            UnityEditor.EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        void Extras()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Max Packet Event Size", () =>
            {
                settings.packetMaxEventSize = DEditorGUI.IntFieldOverlay(settings.packetMaxEventSize, "");
            });

            DEditorGUI.WithLabel("Max Entity Priority", () =>
            {
                settings.maxEntityPriority = DEditorGUI.IntFieldOverlay(settings.maxEntityPriority, "");
            });

            DEditorGUI.WithLabel("Max Property Priority", () =>
            {
                settings.maxPropertyPriority = DEditorGUI.IntFieldOverlay(settings.maxPropertyPriority, "");
            });

            DEditorGUI.WithLabel("Command Queue Size", () =>
            {
                settings.commandQueueSize = DEditorGUI.IntFieldOverlay(settings.commandQueueSize, "");
            });

            DEditorGUI.WithLabel("Command Dejitter Delay", () =>
            {
                settings.commandDejitterDelay = DEditorGUI.IntFieldOverlay(settings.commandDejitterDelay, "");
            });

            DEditorGUI.WithLabel("Command Redundancy", () =>
            {
                settings.commandRedundancy = DEditorGUI.IntFieldOverlay(settings.commandRedundancy, "");
            });

            DEditorGUI.WithLabel("Command Delay Allowed", () =>
            {
                settings.commandDelayAllowed = DEditorGUI.IntFieldOverlay(settings.commandDelayAllowed, "");
            });

            DEditorGUI.WithLabel("Command Ping Multiplier", () =>
            {
                settings.commandPingMultiplier = DEditorGUI.FloatFieldOverlay(settings.commandPingMultiplier, "");
            });
        }

        void Compiler()
        {
            RuntimeSettings settings = RuntimeSettings.Instance;

            DEditorGUI.WithLabel("Warning Level", () =>
            {
                settings.compilationWarnLevel = EditorGUILayout.IntField(settings.compilationWarnLevel);
                settings.compilationWarnLevel = Mathf.Clamp(settings.compilationWarnLevel, 0, 4);
            });

            DEditorGUI.WithLabel("Prefab Mode", () =>
            {
                PrefabDatabase.Instance.DatabaseMode = (PrefabDatabaseMode)EditorGUILayout.EnumPopup(PrefabDatabase.Instance.DatabaseMode);
            });

            DEditorGUI.WithLabel("Compile DLL Assembly? ", () => {
                settings.compileAsDll = DEditorGUI.ToggleButton("On", "Off", settings.compileAsDll);
            });
            DEditorGUI.WithLabel("Use Network Data Namespace? ", () => {
                settings.useNetworkDataNamespace = DEditorGUI.ToggleButton("On", "Off", settings.useNetworkDataNamespace);
            });
        }
    }
}
