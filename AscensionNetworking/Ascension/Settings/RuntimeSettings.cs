using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Ascension.Networking.Sockets
{
    public class RuntimeSettings : ScriptableObject
    {
        private static RuntimeSettings instance;

        public static RuntimeSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    //Try loading
                    LoadAsset();
                    if (instance == null)
                    {
                        //Create a new if loading fails
                        instance = CreateInstance<RuntimeSettings>();
                    }
                }

                return instance;
            }
        }

        public const string windowTitle = "Settings";
        public const string pathPrefix = "Assets/";
        public const string pathToAsset = "__ASCENSION__/Networking/Resources/User";
        public const string assetName = "RuntimeSettings";
        public const string assetExtension = ".asset";

        public static string FullAssetPathName { get { return pathToAsset + "/" + assetName; } }
        public static string FullAssetPathNameWithExt { get { return pathToAsset + "/" + assetName + assetExtension; } }

        /// <summary>
        /// How long in ms receiver will wait before it will force send acknowledgements back without waiting for any payload
        /// </summary>
        [SerializeField]
        public uint ackDelay = 33;

        [SerializeField]
        public uint allCostTimeout = 20;

        [SerializeField]
        public uint connectTimeout = 2000;

        [SerializeField]
        public uint disconnectTimeout = 2000;

        [SerializeField]
        public ushort fragmentSize = 500;

        [SerializeField]
        public bool isAcksLong = false;

        [SerializeField]
        public ushort maxCombinedReliableMessageCount = 10;

        [SerializeField]
        public ushort maxCombinedReliableMessageSize = 100;

        [SerializeField]
        public byte maxConnectionAttempt = 10;

        [SerializeField]
        public ushort maxSentMessageQueueSize = 128;

        [SerializeField]
        public uint minUpdateTimeout = 10;

        [SerializeField]
        public byte networkDropThreshold = 5;

        [SerializeField]
        public byte overflowDropThreshold = 5;

        [SerializeField]
        public ushort packetSize = 1500;

        [SerializeField]
        public uint pingTimeout = 500;

        [SerializeField]
        public uint reducedPingTimeout = 100;

        [SerializeField]
        public uint resendTimeout = 1200;

        #region Topology

        [SerializeField]
        public int maxDefaultConnections = 64;

        [SerializeField]
        public float messagePoolSizeGrowthFactor = 0.75f;

        [SerializeField]
        public ushort receivedMessagePoolSize = 128;

        [SerializeField]
        public ushort sentMessagePoolSize = 128;

        #endregion

        #region Global Config

        [SerializeField]
        public ReactorModel reactorModel = ReactorModel.SelectReactor;
        /// <summary>
        /// Select() will be blocked on read operation, and no any sends will happened before you will 
        /// receive something:
        ///ret = select( sockets, ..., 0 )
        ///if ret = timeouthappened
        ///DoSendOperation()...
        /// </summary>
        [SerializeField]
        public uint threadAwakeTimeout = 1;

        #endregion

        #region Custom

        [SerializeField]
        public int networkEventMaxSize = 65535; // 0xFFFF = 65535 | Upper limit should be 2147483591

        [SerializeField]
        public int totalNetworkEventCount = 500;

        #endregion

        #region Simulation Stats

        [SerializeField]
        public int framesPerSecond = 60;

        [SerializeField]
        public int clientSendRate = 3;

        [SerializeField]
        public bool allowSendRateThrottling = true;

        [SerializeField]
        public bool disableDejitterBuffer = false;

        [SerializeField]
        public int clientDejitterDelay = 6;

        [SerializeField]
        public int clientDejitterDelayMin = 3;

        [SerializeField]
        public int clientDejitterDelayMax = 9;

        [SerializeField]
        public int serverSendRate = 3;

        [SerializeField]
        public int serverDejitterDelay = 6;

        [SerializeField]
        public int serverDejitterDelayMin = 3;

        [SerializeField]
        public int serverDejitterDelayMax = 9;

        [SerializeField]
        public bool overrideTimeScale = true;

        [SerializeField]
        public bool clientCanInstantiateAll = false;

        [SerializeField]
        public ScopeMode scopeMode = ScopeMode.Automatic;

        /// <summary>
        /// Whether to use automatic or manual mode for accepting incoming client connection requests
        /// </summary>
        [SerializeField]
        public ConnectionAcceptMode serverConnectionAcceptMode = ConnectionAcceptMode.Auto;

        [SerializeField]
        public bool scopeModeHideWarningInGui = true;

        /// <summary>
        /// Whether to use network latency simulation
        /// </summary>
        [SerializeField]
        public bool useNetworkSimulation = true;

        /// <summary>
        /// The packet loss rate to use in latency simulation
        /// </summary>
        [SerializeField]
        public float simulatedLoss;

        /// <summary>
        /// The mean ping in milliseconds to use in latency simulation
        /// </summary>
        [SerializeField]
        public int simulatedPingMean;

        /// <summary>
        /// The deviation to use in ping simulation
        /// </summary>
        [SerializeField]
        public int simulatedPingJitter;

        /// <summary>
        /// Whether to use Perlin Noise or System.Random function to create ping deviations
        /// </summary>
        [SerializeField]
        public RandomizationFunction simulatedRandomFunction = RandomizationFunction.PerlinNoise;

        #endregion

        #region Debug

        [SerializeField]
        public bool useDefaults = true;

        [SerializeField]
        public int debugClientCount = 1;

        [SerializeField]
        public EditorStartMode debugEditorMode = EditorStartMode.Server;

        [SerializeField]
        public string debugStartMapName = "";

        [SerializeField]
        public bool debugPlayAsServer = true;

        [SerializeField]
        public int debugStartPort = 5000;

        [SerializeField]
        public ConfigLogTargets logTargets;

        [SerializeField]
        public bool showDebugInfo;

        [SerializeField]
        public bool logUncaughtExceptions;

        [SerializeField]
        public KeyCode consoleToggleKey;

        [SerializeField]
        public bool consoleVisibleByDefault;

        [SerializeField]
        public bool showAscensionEntityHints;

        #endregion

        #region Extras

        [SerializeField]
        public int commandDejitterDelay;

        [SerializeField]
        public int maxPropertyPriority = 1 << 11;

        /// <summary>
        /// The max number of input commands that can be queued at once
        /// </summary>
        [SerializeField]
        public int commandQueueSize = 60;

        [SerializeField]
        public int commandDelayAllowed;

        /// <summary>
        /// The number of times to redundantly send input commands to the server
        /// </summary>
        [SerializeField]
        public int commandRedundancy = 6;

        [SerializeField]
        public float commandPingMultiplier = 1.25f;


        [SerializeField]
        public int maxEntityPriority = 1 << 16;

        [SerializeField]
        public int packetMaxEventSize = 512;

        #endregion

        #region Compiler
        [SerializeField]
        public int compilationWarnLevel;

        [SerializeField]
        public bool compileAsDll;

        [SerializeField]
        public bool useNetworkDataNamespace;

        #endregion

        #region LLAPI
        [SerializeField]
        public bool lowLevelMessagesIncrementBandwidth;

        [SerializeField]
        public bool simulatePacketLoss;

        [SerializeField]
        public bool simulateLatency;

        [SerializeField]
        public int simulationPacketLossChance;

        [SerializeField]
        public int simulationMaxLatency;
        #endregion
        
        #region Paths
        
        [SerializeField]
        public string assetsPath;
        
        #endregion

        public RuntimeSettings()
        {
            // sendrates of server/client
            //serverSendRate = 3;
            //clientSendRate = 3;

            clientSendRate = serverSendRate;

            // interpolation delay on client is based on server rate
            clientDejitterDelay = serverSendRate * 2;
            clientDejitterDelayMin = clientDejitterDelay - serverSendRate;
            clientDejitterDelayMax = clientDejitterDelay + serverSendRate;

            // interpolation delay on server is based on client rate
            serverDejitterDelay = clientSendRate * 2;
            serverDejitterDelayMin = serverDejitterDelay - clientSendRate;
            serverDejitterDelayMax = serverDejitterDelay + clientSendRate;

            // max clients connected to the server
            maxDefaultConnections = 64;

            // commands config
            commandRedundancy = 6;
            commandPingMultiplier = 1.25f;
            commandDejitterDelay = 3;
            commandDelayAllowed = clientDejitterDelay * 2;
            commandQueueSize = 60;
        }

        public RuntimeSettings Clone()
        {
            return (RuntimeSettings)MemberwiseClone();
        }

        public static void LoadAsset()
        {
#if UNITY_EDITOR
            instance = (RuntimeSettings)AssetDatabase.LoadAssetAtPath(pathPrefix + FullAssetPathNameWithExt, typeof(RuntimeSettings));
#else
            instance = (RuntimeSettings)Resources.Load("User/" + assetName, typeof(RuntimeSettings));
#endif
        }
    }
}
