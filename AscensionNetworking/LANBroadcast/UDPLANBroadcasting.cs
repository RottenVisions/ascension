using System;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class UDPLANBroadcasting : MonoBehaviour
{
    private static UDPLANBroadcasting instance;

    public string instanceName;
    public int serverPort;
    public int remotePort = 19784;

    private UdpClient sender;
    private UdpClient receiver;

    private string receivedInstanceName;
    private string receivedIP;
    private int receivedPort;

    private int localPort = 0;

    public static UDPLANBroadcasting Instance
    {
        get { return instance ?? (instance = new UDPLANBroadcasting()); }
    }

    public string ReceivedInstanceName { get { return receivedInstanceName; } }

    public string ReceivedIP { get { return receivedIP; } }

    public int ReceivedPort { get { return receivedPort; } }

    void Start()
    {
        sender = new UdpClient(localPort, AddressFamily.InterNetwork);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, remotePort);
        sender.Connect(groupEP);

        InvokeRepeating("BroadcastData", 0, 5f);
    }

    void BroadcastData()
    {
        string customMessage = string.Format("{0}*{1}*{2}", instanceName, LocalIPAddress(), serverPort);

        if (customMessage != "")
        {
            sender.Send(Encoding.ASCII.GetBytes(customMessage), customMessage.Length);
        }
    }

    public void StartReceivingIP()
    {
        try
        {
            if (receiver == null)
            {
                receiver = new UdpClient(remotePort);
                receiver.BeginReceive(new AsyncCallback(ReceiveData), null);
            }
        }
        catch (SocketException e)
        {
            Debug.Log(e.Message);
        }
    }

    void ReceiveData(IAsyncResult result)
    {
        IPEndPoint receiveIPGroup = new IPEndPoint(IPAddress.Any, remotePort);

        byte[] received;

        if (receiver != null)
        {
            received = receiver.EndReceive(result, ref receiveIPGroup);
        }
        else
        {
            return;
        }
        receiver.BeginReceive(new AsyncCallback(ReceiveData), null);
        string receivedString = Encoding.ASCII.GetString(received);

        if (string.IsNullOrEmpty(receivedString)) return;

        Debug.Log(receivedString);
        ParseReceivedData(receivedString);
        Debug.Log(string.Format("{0}, {1}, {2}", receivedInstanceName, receivedIP, receivedPort));
    }

    void ParseReceivedData(string data)
    {
        string[] parsedData = data.Split('*');
        //Catch an invalid data strip
        if (parsedData.Length > 3) return;
        //Parse data then delete any white space characters
        receivedInstanceName = parsedData[0];
        receivedIP = parsedData[1];
        receivedPort = int.Parse(parsedData[2]);
    }

    public static string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }
}
