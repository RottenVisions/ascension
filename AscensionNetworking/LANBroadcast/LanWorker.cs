using UnityEngine;
using System.Collections.Generic;

public class LanWorker : MonoBehaviour
{
    private static LanManager manager;
    public static int remotePort = 19374;

    public static bool IsServer { get { return manager != null && manager.IsServer; } }

    public static bool IsClient { get { return manager != null && manager.IsClient; } }

    // Addresses of the computer (Ethernet, WiFi, etc.)
    public static List<string> LocalAddresses 
    { 
        get 
        { 
            if (manager != null) return manager.LocalAddresses;
            return null;
        } 
    }
    public static List<string> LocalSubAddresses {
        get
        {
            if (manager != null) return manager.LocalSubAddresses;
            return null;
        }
    }

    // Addresses found on the network with a server launched
    public static List<string> Addresses
    {
        get
        {
            if (manager != null) return manager.Addresses;
            return null;
        }
    }

    public static void Initialize()
    {
        manager = new LanManager();
    }

    public static void StartServer()
    {
        manager.StartServer(remotePort);
    }

    public static void StartClient()
    {
        manager.StartClient(remotePort);
    }

    public static void CloseServer()
    {
        manager.CloseServer();
    }

    public static void CloseClient()
    {
        manager.CloseClient();
    }

    public static void ScanHost()
    {
        manager.ScanHost();
    }

    public void SendPing()
    {
        StartCoroutine(manager.SendPing(remotePort));
    }
}
