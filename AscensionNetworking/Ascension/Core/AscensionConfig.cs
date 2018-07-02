using System;

[Serializable]
public class AscensionConfig
{
    public int packetWindow = 256;
    public int packetDatagramSize = 1200;

    public int streamWindow = 1024;
    public int streamDatagramSize = 4096;

    /// <summary>
    /// The default network ping for new connections, default: 0.1f (seconds)
    /// </summary>
    public float defaultNetworkPing = 0.1f;

    /// <summary>
    /// The default aliased ping for new connections, default: 0.15f (seconds)
    /// </summary>
    public float defaultAliasedPing = 0.15f;

    /// <summary>
    /// How many % of the packets we should drop to simulate packet loss, default: 0. Only used in DEBUG builds.
    /// </summary>
    public int simulatedLoss = 0;

    /// <summary>
    /// Max ping we should simulate, default: 0 (ms). Only used in DEBUG builds.
    /// </summary>
    public int simulatedMaxLatency = 0;

    /// <summary>
    /// How many connections we allow, default: 64
    /// </summary>
    public int connectionLimit = 64;

    /// <summary>
    /// If we allow incomming connections, default: true
    /// </summary>
    public bool allowIncommingConnections = true;

    /// <summary>
    /// IF we automatically accept incomming connections if we have slots free, default: true
    /// </summary>
    public bool autoAcceptIncommingConnections = true;

    /// <summary>
    /// If we allow clients which are connecting to a server to implicitly accept the connection
    /// if we get a non-rejected and non-accepted packet from the server, meaning the accept packet
    /// was lost, default: true
    /// </summary>
    public bool allowImplicitAccept = true;

    /// <summary>
    /// Custom noise function for use in packet loss simulation, default: null
    /// </summary>
    public Func<float> noiseFunction = null;


    public AscensionConfig Duplicate()
    {
        return (AscensionConfig)MemberwiseClone();
    }
}
