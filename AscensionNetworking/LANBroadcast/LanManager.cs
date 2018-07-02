using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class LanManager
{
    // Addresses of the computer (Ethernet, WiFi, etc.)
    public List<string> LocalAddresses { get; private set; }
    public List<string> LocalSubAddresses { get; private set; }

    // Addresses found on the network with a server launched
    public List<string> Addresses { get; private set; }

    public bool IsSearching { get; private set; }
    public float PercentSearching { get; private set; }

    public bool IsServer { get { return socketServer != null; } }

    public bool IsClient { get { return socketClient != null; } }

    private Socket socketServer;
    private Socket socketClient;

    private EndPoint remoteEndPoint;

    public LanManager()
    {
        Addresses = new List<string>();
        LocalAddresses = new List<string>();
        LocalSubAddresses = new List<string>();
    }

    public void StartServer(int port)
    {
        if (socketServer == null && socketClient == null)
        {
            try
            {
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (socketServer == null)
                {
                    Debug.LogWarning("SocketServer creation failed");
                    return;
                }

                // Check if we received pings
                socketServer.Bind(new IPEndPoint(IPAddress.Any, port));

                remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
                                               ref remoteEndPoint, new AsyncCallback(ReceiveServer), null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    public void CloseServer()
    {
        if (socketServer != null)
        {
            socketServer.Close();
            socketServer = null;
        }
    }

    public void StartClient(int port)
    {
        if (socketServer == null && socketClient == null)
        {
            try
            {
                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (socketClient == null)
                {
                    Debug.LogWarning("SocketClient creation failed");
                    return;
                }

                // Check if we received response from a remote (server)
                socketClient.Bind(new IPEndPoint(IPAddress.Any, port));

                socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);

                remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                socketClient.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
                                         ref remoteEndPoint, new AsyncCallback(ReceiveClient), null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    public void CloseClient()
    {
        if (socketClient != null)
        {
            socketClient.Close();
            socketClient = null;
        }
    }

    public IEnumerator SendPing(int port)
    {
        Addresses.Clear();

        if (socketClient != null)
        {
            int maxSend = 4;
            float countMax = (maxSend * LocalSubAddresses.Count) - 1;

            float index = 0;

            IsSearching = true;

            // Send several pings just to be sure (a ping can be lost!)
            for (int i = 0; i < maxSend; i++)
            {

                // For each address that this device has
                foreach (string subAddress in LocalSubAddresses)
                {
                    IPEndPoint destinationEndPoint = new IPEndPoint(IPAddress.Parse(subAddress + ".255"), port);
                    byte[] str = Encoding.ASCII.GetBytes("ping");

                    socketClient.SendTo(str, destinationEndPoint);

                    PercentSearching = index / countMax;

                    index++;

                    yield return new WaitForSeconds(0.1f);
                }
            }
            IsSearching = false;
        }
    }

    private void ReceiveServer(IAsyncResult ar)
    {
        if (socketServer != null)
        {
            try
            {
                int size = socketServer.EndReceiveFrom(ar, ref remoteEndPoint);
                byte[] str = Encoding.ASCII.GetBytes("pong");

                // Send a pong to the remote (client)
                socketServer.SendTo(str, remoteEndPoint);

                socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
                                               ref remoteEndPoint, new AsyncCallback(ReceiveServer), null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    private void ReceiveClient(IAsyncResult ar)
    {
        if (socketClient != null)
        {
            try
            {
                int size = socketClient.EndReceiveFrom(ar, ref remoteEndPoint);
                string address = remoteEndPoint.ToString().Split(':')[0];

                // This is not ourself and we do not already have this address
                if (!LocalAddresses.Contains(address) && !Addresses.Contains(address))
                {
                    Addresses.Add(address);
                }

                socketClient.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
                                               ref remoteEndPoint, new AsyncCallback(ReceiveClient), null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    public void ScanHost()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                string address = ip.ToString();
                string subAddress = address.Remove(address.LastIndexOf('.'));

                //if (!LocalAddresses.Contains(address))
                //{
                    LocalAddresses.Add(address);
                //}

                if (!LocalSubAddresses.Contains(subAddress))
                {
                    LocalSubAddresses.Add(subAddress);
                }
            }
        }
    }
}