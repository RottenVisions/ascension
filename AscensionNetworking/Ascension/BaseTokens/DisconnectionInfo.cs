using UnityEngine;
using System.Collections;
using Ascension.Networking;

public class DisconnectionInfo : IMessageRider
{
    public string reason;
    public int type;

    public void Read(Packet packet)
    {
        reason = packet.ReadString();
        type = packet.ReadInt();
    }

    public void Write(Packet packet)
    {
        packet.WriteString(reason);
        packet.WriteInt(type);
    }
}
