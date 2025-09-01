using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq; // for JToken parsing
using TMPro;
using SocketIOClient;
using SocketIOClient.Transport;                 // if you’re using TextMeshPro

[Serializable]
public class PlayerInfo
{
    public int id;
    public string name;
    public List<string> roles;
}

public class LobbyClient : MonoBehaviour
{
    [Header("UI References (assign in Inspector)")]
    public TMP_InputField salonIdInput;
    public TMP_Text rosterText;
    public TMP_Text statusText;

    private SocketIOUnity socket;
    private const string SERVER_URL = "http://localhost:3000";

    async void Start()
    {
        statusText.text = "Connecting to lobby server…";

        // 1) Build the URI & options, including the JWT as a query param
        var uri = new Uri(SERVER_URL);
        var opts = new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                { "token", PlayerPrefs.GetString("jwt_token", "") }
            },
            EIO = EngineIO.V4,         // Socket.IO protocol version
            Transport = TransportProtocol.WebSocket
        };

        // 2) Create your SocketIOUnity instance
        socket = new SocketIOUnity(uri, opts);

        // 3) Hook up events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log(" Connected to lobby server");
            statusText.text = "Connected! Enter a salon ID to join.";
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log(" Disconnected from lobby server");
            statusText.text = "Disconnected. Reconnecting…";
            _ = socket.ConnectAsync();
        };

        socket.On("rosterUpdate", response =>
        {
            // response is a JToken (Newtonsoft) containing an array of player objects
            var arr = response.GetValue<JArray>();
            var players = arr.ToObject<List<PlayerInfo>>();
            UpdateRosterUI(players);
        });

        // 4) Actually connect
        await socket.ConnectAsync();
    }

    public async void JoinSalon()
    {
        var salonId = salonIdInput.text.Trim();
        if (string.IsNullOrEmpty(salonId))
        {
            statusText.text = "Please enter a Salon ID.";
            return;
        }
        // Emit the salon ID as a JSON payload
        await socket.EmitAsync("joinSalon", JToken.FromObject(salonId));
        statusText.text = $"Joined Salon: {salonId}";
    }

    public async void LeaveSalon()
    {
        var salonId = salonIdInput.text.Trim();
        if (string.IsNullOrEmpty(salonId))
        {
            statusText.text = "Please enter a Salon ID.";
            return;
        }
        await socket.EmitAsync("leaveSalon", JToken.FromObject(salonId));
        statusText.text = $"Left Salon: {salonId}";
        rosterText.text = "";
    }

    private void UpdateRosterUI(List<PlayerInfo> players)
    {
        rosterText.text = "";
        foreach (var p in players)
            rosterText.text += $"{p.name} ({p.roles[0]})\n";
    }
}
