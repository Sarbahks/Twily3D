using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WsClient : MonoBehaviour
{
    private static WsClient instance;
    public static WsClient Instance
    {
        get
        {
            
                if(instance == null)
                {
                    instance = FindFirstObjectByType<WsClient>();
                }

                return instance;
            
        }
    }


    [Serializable]
    public class Envelope<T> { public string type; public T data; }




    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private WebSocket ws;

    private async void Start()
    {
        // In Editor/local HTTP: ws://
        // In WebGL over HTTPS: use wss://your-host/ws
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = "wss://localhost:7239/ws"; // if your page is served over https
#else
        string url = "ws://localhost:5124/ws";
#endif
        ws = new WebSocket(url);

        ws.OnOpen += () =>
        {
            Debug.Log("WS open, sending connectedToServer...");
            var payload = new ServerUserData
            {
                UserInfo = new UserInfo
                {
                    Id = Authentificator.Instance.Id,
                    Name = Authentificator.Instance.Username,
                    Roles = Authentificator.Instance.RolesArray
                },
                Token = Authentificator.Instance.JwtToken
            };
            var envelope = new Envelope<ServerUserData> { type = "connectedToServer", data = payload };
            var json = JsonConvert.SerializeObject(envelope);
            _ = ws.SendText(json);
        };

        ws.OnMessage += bytes =>
        {
            var text = Encoding.UTF8.GetString(bytes);
            Debug.Log("WS recv: " + text);

            try
            {
                var jobj = JObject.Parse(text);
                var type = jobj["type"]?.ToString() ?? "";

                if (type == "connectedAck")
                {
                    var data = jobj["data"]?.ToObject<ServerUserData>();
                    Debug.Log($"connectedAck OK. id={data?.UserInfo?.Id}, name={data?.UserInfo?.Name}");
                }
                if(type == "bigSalonsList")
                {
          
                    var salons = jobj["data"]?["salons"]?.ToObject<List<BigSalonInfo>>();


                    LobbySceneManager.Instance.RenderBigSalonList(salons);
                    Debug.Log("Recieve data big salons");
                }
                if (type == "salonJoined")
                {
                    var resp = jobj["data"]?.ToObject<JoinSalonResponse>();
                    if (resp != null)
                    {
                        if (resp.Joined)
                        {
                            Debug.Log($"Joined salon {resp.SalonId}");
                      
                            //actualize salons data
                            GetSalons();



                            LobbySceneManager.Instance.JoinBigSalonLobby(resp.SalonId);
                        }
                        else
                        {
                            Debug.LogWarning($"Could not join salon {resp.SalonId}");
                            // maybe show message to user
                        }
                    }
                }
                if (type == "salonLeaved")
                {
                    var resp = jobj["data"]?.ToObject<LeaveSalonResponse>();
                    if (resp != null && resp.Leaved)
                    {
                        Debug.Log($"Left salon {resp.SalonId}");
                        // return to lobby screen etc.
                    }
                }

                if (type == "actualizeLobby")
                {
                    var salon = jobj["data"]?["salon"]?.ToObject<BigSalonInfo>();
                    if (salon != null)
                    {
                        LobbySceneManager.Instance.SetupBigSalonUI(salon);
                        NotificationCenter.Instance.SetupNotificationCenter(salon);
                    }
                }
                if (type == "teamJoined")
                {
                    bool joined = (bool)(jobj["data"]?["joined"] ?? false);
                    string salonId = (string)jobj["data"]?["salonId"];
                    string teamId = (string)jobj["data"]?["teamId"];

                    if (joined)
                    {
                        Debug.Log($"Joined team {teamId} in salon {salonId}");
                        // UI: optionally show success; the "actualizeLobby" broadcast will bring the fresh state
                        GetLobby(salonId);//change that for something with a return, and then, do a gameupdate if game != null or is started
                    }
                    else
                    {
                        Debug.LogWarning($"Join team failed for {teamId} in salon {salonId}");
                    }
                }

                if (type == "teamLeaved")
                {
                    bool leaved = (bool)(jobj["data"]?["leaved"] ?? false);
                    string salonId = (string)jobj["data"]?["salonId"];
                    string teamId = (string)jobj["data"]?["teamId"];

                    if (leaved)
                    {
                        Debug.Log($"Left team {teamId} in salon {salonId}");
                        // UI: wait for "actualizeLobby" to refresh
                    }
                }
                if (type == "initializeGame")
                {
                    var data = jobj["data"]?.ToObject<InitializeGamePayload>();
                    // build board, rules, etc. on client as needed...
                   var game =  LobbySceneManager.Instance.SetUpGameFromServer(data.Game);
                    // then send back:
                    var envelope = new Envelope<GameBoardInitializedPayload>
                    {
                        type = "gameBoardInitialized",
                        data = new GameBoardInitializedPayload
                        {
                            SalonId = data.SalonId,
                            TeamId = data.TeamId,
                            Game = game  // or your modified/filled GameStateData
                        }
                    };
                    var json = JsonConvert.SerializeObject(envelope);
                    _ = ws.SendText(json);
                }
                if (type == "gameInitialized")
                {
                    var payload = jobj["data"]?.ToObject<GameBoardInitializedPayload>();
                    // payload.Game is authoritative; move to gameplay scene, enable UI, etc.
                    Debug.Log("Start game and choose profile");

                    LobbySceneManager.Instance.SetUpGameFromServer(payload.Game);

                    BoardManager.Instance.InitializeGameFromData(payload.Game);
                    LobbySceneManager.Instance.SyncDataFromServer(payload.Game);
                    GameScript.Instance.StartTheGame();
                   
                   
            
                }
                if (type == "profileChosen")
                {
                    var payload = jobj["data"]?.ToObject<GameStateData>();
                    if (payload == null)
                    {
                        Debug.LogWarning("profileChosen received with missing payload.Game");
                        return;
                    }

                    // Apply authoritative GameState
                    LobbySceneManager.Instance.SyncDataFromServer(payload);

                    // If step advanced, move UI accordingly
                    if (payload.Step == StepGameType.ROLECHOSEN)
                    {
                        GameScript.Instance.ProfileAllChosen();
                    }

                 
                }

                else if (type == "error")
                {
                    Debug.LogWarning("Server error: " + jobj["data"]?.ToString());
                }
            }
            catch { /* non JSON or parse error */ }
        };

        ws.OnError += e => Debug.LogError("WS error: " + e);
        ws.OnClose += code => Debug.Log("WS closed: " + code);

        // keep socket alive if editor focus changes
        Application.runInBackground = true;

        await ws.Connect();

        // optional heartbeat to verify flow
        //InvokeRepeating(nameof(SendPing), 2f, 5f);
    }

    void Update()
    {
        // Required outside WebGL to deliver OnMessage callbacks
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
    }

    private void SendPing()
    {
        if (ws != null && ws.State == WebSocketState.Open)
            _ = ws.SendText("{\"type\":\"ping\"}");
    }

    public void CreateSalon(BigSalonInfo info)
    {
        
        var envelope = new Envelope<BigSalonInfo> { type = "createBigSalon", data = info };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }

    public void GetSalons()
    {
        var envelope = new Envelope<string> { type = "getBigSalons", data = string.Empty };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }


    public void JoinSalon(JoinSalonRequest joinSalonRequest) 
    {
        var envelope = new Envelope<JoinSalonRequest> { type = "joinSalonRequest", data = joinSalonRequest };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }

    public void LeaveSalon(LeaveSalonRequest leaveSalonRequest)
    {
        var envelope = new Envelope<LeaveSalonRequest> { type = "leaveSalonRequest", data = leaveSalonRequest };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }

    public void CreateTeamOnSalon(string idSalon, SalonInfo team)
    {
        var envelope = new Envelope<CreateTeamRequest> { type = "createTeam", data = new CreateTeamRequest { IdSalon = idSalon, SalonInfo = team} };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }


    public void JoinTeamOnSalon(string idSalon, UserInfo user, bool isPlayer, string teamId)
    {


        var envelope = new Envelope<JoinTeamRequest> { type = "joinTeam", data = new JoinTeamRequest { SalonId = idSalon,TeamId = teamId , UserInfo = user , IsPlayer = isPlayer } }; 
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }

    public void LeaveTeamOnSalon(string idSalon, UserInfo user, bool isPlayer, string teamId)
    {

        var envelope = new Envelope<LeaveTeamRequest> { type = "leaveTeam", data = new LeaveTeamRequest { SalonId = idSalon, TeamId = teamId ,UserInfo = user, IsPlayer = isPlayer } };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }


    public void GetLobby(string salonId)
    {
        var envelope = new Envelope<string> { type = "getLobbyInfo", data = salonId };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }


    public void AskStartGame(StartGameRequest startRequest)
    {
        var envelope = new Envelope<StartGameRequest> { type = "askStartGame", data = startRequest };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }


    public void LeaveGame(LeaveGameRequest leaveRequest)
    {
        var envelope = new Envelope<LeaveGameRequest> { type = "leaveGame", data = leaveRequest };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }


    public void SelectRole(string idBigSalon, string idTeam, RoleGameType roleGameType)
    {
        //ask,type player taht it is and send it to the server


        ChoseRoleGameRequest req = new ChoseRoleGameRequest
        {
            UserInfo = Authentificator.Instance.GetUser(),
            SalonId = idBigSalon,
            TeamId = idTeam,
            RoleWanted = roleGameType
        };
        var envelope = new Envelope<ChoseRoleGameRequest> { type = "choseRole", data = req };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }



    private void OnDestroy()
    {
        LobbySceneManager.Instance.QuitGame();
    }

    private async void OnApplicationQuit()
    {
        try
        {
            // Tell server you are leaving the salon first
            if (LobbySceneManager.Instance != null)
            {
                LobbySceneManager.Instance.QuitGame(); // this should send leaveSalonRequest
            }

            // Give it a tiny frame to flush (NativeWebSocket queues sends)
            await System.Threading.Tasks.Task.Delay(100);

            // Close socket cleanly
            if (ws != null && ws.State == WebSocketState.Open)
            {
                await ws.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("OnApplicationQuit cleanup error: " + ex);
        }
    }

}
