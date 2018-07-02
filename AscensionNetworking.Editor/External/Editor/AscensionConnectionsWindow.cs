using System;
using System.Linq;
using System.Collections;
using Ascension.Networking;
using Ascension.Networking.Sockets;
using UnityEditor;
using UnityEngine;
using DEditorGUI = Ascension.Tools.EditorGUI;

[InitializeOnLoad]
public class AscensionConnectionsWindow : AscensionWindow
{
    Vector2 scroll;
    Connection ConnectionDetails;

    void OnEnable()
    {
        titleContent = new GUIContent("Remotes");
        name = "Remotes";
        scroll = Vector2.zero;
    }

    void Header(string icon, string text)
    {
        GUILayout.BeginHorizontal(DEditorGUI.HeaderBackground, GUILayout.Height(DEditorGUI.HEADER_HEIGHT));

        DEditorGUI.IconButton(icon);
        GUILayout.Label(text);

        GUILayout.EndHorizontal();
    }

    new void Update()
    {
        if (Application.isPlaying)
        {
            Repaints = Mathf.Max(Repaints, 1);
        }

        base.Update();
    }

    new void OnGUI()
    {
        base.OnGUI();

        GUILayout.BeginArea(new Rect(DEditorGUI.GLOBAL_INSET, DEditorGUI.GLOBAL_INSET, position.width - (DEditorGUI.GLOBAL_INSET * 2), position.height - (DEditorGUI.GLOBAL_INSET * 2)));

        if (!Application.isPlaying)
        {
            ConnectionDetails = null;
        }

        scroll = GUILayout.BeginScrollView(scroll);

        if (AscensionNetwork.Connections.Count() == 1)
        {
            ConnectionDetails = AscensionNetwork.Connections.First();
        }

        Header("connection", "Connections");
        Connections();

        if (ConnectionDetails != null)
        {
            Header("connection", "Packet details for " + ConnectionDetails.RemoteEndPoint);
            ConnectionDetailsView();
        }

        //Header("mc_server", "LAN Servers");
        //LanServers();

        GUILayout.EndArea();
        GUILayout.EndScrollView();
    }

    /*void LanServers()
    {
        GUIStyle s = new GUIStyle(GUIStyle.none);
        s.padding = new RectOffset(5, 5, 2, 2);
        GUILayout.BeginHorizontal(s);

        var sessions = AscensionNetwork.isRunning ? AscensionNetwork.GetSessions() : new UdpKit.UdpSession[0];

        Each<UdpKit.UdpSession>(sessions, MakeHeader("mc_name", "Name"), c => StatsLabel(c.HostName));
        Each<UdpKit.UdpSession>(sessions, MakeHeader("mc_ipaddress", "End Point"), c => StatsLabel(c.WanEndPoint));

        //Each<UdpKit.UdpSession>(sessions, MakeHeader("mc_bubble", "User Data"), c => StatsLabel(c.Data ?? ""));

        GUILayout.EndHorizontal();
        GUILayout.Space(4);
    }*/

    Action MakeHeader(string icon, string text)
    {
        return () => {
            GUILayout.BeginHorizontal();

            DEditorGUI.IconButton(icon);

            GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
            s.padding = new RectOffset();
            s.margin = new RectOffset(5, 0, 3, 0);

            GUILayout.Label(text, s);

            GUILayout.EndHorizontal();
        };
    }

    bool IsSelected(Connection c)
    {
        return ReferenceEquals(ConnectionDetails, c);
    }

    void StatsButton(Connection c, object text)
    {
        StatsButton(() => ConnectionDetails = c, IsSelected(c), text);
    }

    void StatsButton(Action clicked, bool selected, object text)
    {
        GUIStyle s = new GUIStyle("Label");
        s.padding = new RectOffset();
        s.margin = new RectOffset(0, 0, 0, 2);
        s.normal.textColor = selected ? DEditorGUI.HighlightColor : s.normal.textColor;

        if (GUILayout.Button(text.ToString(), s))
        {
            clicked();
        }
    }

    void StatsLabel(object text)
    {
        GUIStyle s = new GUIStyle("Label");
        s.padding = new RectOffset();
        s.margin = new RectOffset(0, 0, 0, 2);

        if (GUILayout.Button(text.ToString(), s))
        {
        }
    }

    void Connections()
    {
        GUILayout.Space(2);
        GUIStyle s = new GUIStyle(GUIStyle.none);
        s.padding = new RectOffset(5, 5, 2, 2);
        GUILayout.BeginHorizontal(s);

        EachConnection(MakeHeader("ipaddress", "Address"), c => StatsButton(c, c.SockConn.RemoteEndPoint));
        EachConnection(MakeHeader("ping", "Ping (Network)"), c => StatsButton(c, c.SockConn.NetworkPing + " ms"));
        EachConnection(MakeHeader("ping", "Ping (Interval)"), c => StatsButton(c, c.SockConn.NetworkPingInterval + " ms"));
        EachConnection(MakeHeader("download", "Download"), c => StatsButton(c, Math.Round((c.BitsPerSecondIn / 8f) / 1000f, 2) + " kb/s"));
        EachConnection(MakeHeader("upload", "Upload"), c => StatsButton(c, Math.Round((c.BitsPerSecondOut / 8f) / 1000f, 2) + " kb/s"));

        GUILayout.EndHorizontal();
        GUILayout.Space(4);
    }

    void EachConnection(Action header, Action<Connection> call)
    {
        Each(Core.connections, header, call);
    }

    void Each<T>(IEnumerable items, Action header, Action<T> call)
    {
        GUILayout.BeginVertical();
        header();

        foreach (T c in items)
        {
            call(c);
        }

        GUILayout.EndVertical();
    }

    void SelectedConnection(Action header, Action<Connection> call)
    {
        Each(new[] { ConnectionDetails }, header, call);
    }

    void ConnectionDetailsView()
    {
        double statesOut = Math.Round(ConnectionDetails.packetStatsOut.Select(x => x.StateBits).Sum() / 8f / ConnectionDetails.packetStatsOut.Count, 2);
        double eventsOut = Math.Round(ConnectionDetails.packetStatsOut.Select(x => x.EventBits).Sum() / 8f / ConnectionDetails.packetStatsOut.Count, 2);
        double commandsOut = Math.Round(ConnectionDetails.packetStatsOut.Select(x => x.CommandBits).Sum() / 8f / ConnectionDetails.packetStatsOut.Count, 2);

        double statesIn = Math.Round(ConnectionDetails.packetStatsIn.Select(x => x.StateBits).Sum() / 8f / ConnectionDetails.packetStatsIn.Count, 2);
        double eventsIn = Math.Round(ConnectionDetails.packetStatsIn.Select(x => x.EventBits).Sum() / 8f / ConnectionDetails.packetStatsIn.Count, 2);
        double commandsIn = Math.Round(ConnectionDetails.packetStatsIn.Select(x => x.CommandBits).Sum() / 8f / ConnectionDetails.packetStatsIn.Count, 2);

        GUIStyle s = new GUIStyle(GUIStyle.none);
        s.padding = new RectOffset(5, 5, 2, 2);
        GUILayout.BeginHorizontal(s);

        SelectedConnection(MakeHeader("state", "In: States"), c => StatsLabel(statesIn + " bytes"));
        SelectedConnection(MakeHeader("state", "Out: States"), c => StatsLabel(statesOut + " bytes"));

        SelectedConnection(MakeHeader("event", "In: Events"), c => StatsLabel(eventsIn + " bytes"));
        SelectedConnection(MakeHeader("event", "Out: Events"), c => StatsLabel(eventsOut + " bytes"));

        SelectedConnection(MakeHeader("command", "In: Commands"), c => StatsLabel(commandsIn + " bytes"));
        SelectedConnection(MakeHeader("command", "Out: Commands"), c => StatsLabel(commandsOut + " bytes"));

        GUILayout.EndHorizontal();
        GUILayout.Space(4);
    }
}
