using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Ascension.Networking.Sockets
{
    public static class Configuration
    {
        static ConnectionConfig config = new ConnectionConfig();

        static GlobalConfig globalConfig = new GlobalConfig();

        static HostTopology hostTopology;

        static int channelCount;

        public static ConnectionConfig GetConfig()
        {
            return config;
        }

        public static GlobalConfig GetGlobalConfig()
        {
            return globalConfig;
        }

        public static HostTopology GetHostTopology()
        {
            return hostTopology;
        }

        #region Configuration
        public static ConnectionConfig SetupConfig()
        {
            config.AckDelay = RuntimeSettings.Instance.ackDelay;
            config.IsAcksLong = RuntimeSettings.Instance.isAcksLong;
            config.AllCostTimeout = RuntimeSettings.Instance.allCostTimeout;
            config.ConnectTimeout = RuntimeSettings.Instance.connectTimeout;
            config.DisconnectTimeout = RuntimeSettings.Instance.disconnectTimeout;
            config.FragmentSize = RuntimeSettings.Instance.fragmentSize;
            config.MaxCombinedReliableMessageCount = RuntimeSettings.Instance.maxCombinedReliableMessageCount;
            config.MaxCombinedReliableMessageSize = RuntimeSettings.Instance.maxCombinedReliableMessageSize;
            config.MaxConnectionAttempt = RuntimeSettings.Instance.maxConnectionAttempt;
            config.MaxSentMessageQueueSize = RuntimeSettings.Instance.maxSentMessageQueueSize;
            config.MinUpdateTimeout = RuntimeSettings.Instance.minUpdateTimeout;
            config.NetworkDropThreshold = RuntimeSettings.Instance.networkDropThreshold;
            config.OverflowDropThreshold = RuntimeSettings.Instance.overflowDropThreshold;
            config.PacketSize = RuntimeSettings.Instance.packetSize;
            config.PingTimeout = RuntimeSettings.Instance.pingTimeout;
            config.ReducedPingTimeout = RuntimeSettings.Instance.reducedPingTimeout;
            config.ResendTimeout = RuntimeSettings.Instance.resendTimeout;

            //Setup Channels
            config = ChannelSetup(config);

            return config;
        }

        public static GlobalConfig SetupGlobalConfig()
        {
            // global config
            GlobalConfig gconfig = new GlobalConfig();
            gconfig.ReactorModel = RuntimeSettings.Instance.reactorModel;
            //GlobalConfig.ThreadAwakeTimeout is a minimum timeout when system will try to grab messages 
            //ready to send, combine them to packet and send out.
            //http://forum.unity3d.com/threads/networktransport-receivefromhost.321627/
            gconfig.ThreadAwakeTimeout = RuntimeSettings.Instance.threadAwakeTimeout;
            //This must match the packet size given in the general config
            gconfig.MaxPacketSize = RuntimeSettings.Instance.packetSize;

            //Cache
            globalConfig = gconfig;

            return gconfig;
        }

        public static HostTopology SetupTopology(ConnectionConfig connectionConfig)
        {
            HostTopology hostTopo = new HostTopology((RuntimeSettings.Instance.useDefaults ? ChannelSetup(new ConnectionConfig()) : connectionConfig), RuntimeSettings.Instance.maxDefaultConnections);
            hostTopo.MessagePoolSizeGrowthFactor = RuntimeSettings.Instance.messagePoolSizeGrowthFactor;
            hostTopo.ReceivedMessagePoolSize = RuntimeSettings.Instance.receivedMessagePoolSize;
            hostTopo.SentMessagePoolSize = RuntimeSettings.Instance.sentMessagePoolSize;

            //Cache
            hostTopology = hostTopo;

            return hostTopology;
        }

        public static ConnectionConfig ChannelSetup(ConnectionConfig config)
        {
            //Check if we have added all the channels already
            //If so, do not add these channels again as it will cause a CRC mismatch
            if (config.ChannelCount > 0)
                return config;

            //Add Channels
            config.AddChannel(QosType.Reliable);
            config.AddChannel(QosType.Unreliable);
            config.AddChannel(QosType.ReliableSequenced);
            config.AddChannel(QosType.UnreliableSequenced);
            config.AddChannel(QosType.ReliableFragmented);
            config.AddChannel(QosType.UnreliableFragmented);
            config.AddChannel(QosType.ReliableStateUpdate);
            config.AddChannel(QosType.StateUpdate);
            config.AddChannel(QosType.AllCostDelivery);

            channelCount = config.ChannelCount;

            return config;
        }
        #endregion
    }

}

