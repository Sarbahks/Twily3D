using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
                            GetLobby(resp.SalonId);



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
                       // GetLobby(salonId);//change that for something with a return, and then, do a gameupdate if game != null or is started
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
                    LobbySceneManager.Instance.SyncDataFromServer(data.Game);
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
                    LobbySceneManager.Instance.SyncDataFromServer(payload.Game);
                    BoardManager.Instance.InitializeGameFromData(payload.Game);
                    LobbySceneManager.Instance.SyncDataFromServer(payload.Game);
                    GameScript.Instance.StartTheGame();

                    GetLobby(LobbySceneManager.Instance.CurrentTeamId);
                   
            
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

                    GameScript.Instance.SetProfileChoice(payload);

                    // If step advanced, move UI accordingly
                    if (payload.Step == StepGameType.ROLECHOSEN)
                    {
                        GameScript.Instance.ProfileAllChosen();
                    }

                 
                }
                if (type == "cardChosen")
                {
                    var payload = jobj["data"]?.ToObject<ChoseCardResponse>();
                    if (payload == null)
                    {
                        Debug.LogWarning("cardChosen missing payload");
                        return;
                    }

                    // Update local state: mark the card as unlocked
                    LobbySceneManager.Instance.CurrentGameState = payload.Game; 
                    var game = LobbySceneManager.Instance.CurrentGameState;
                    game.CurrentPosition = payload.IdCard;
                    if (game != null && game.Board != null)
                    {
                        var card = game.Board.FirstOrDefault(c => c.Id == payload.IdCard);
                        if (card != null)
                        {
                            card.Unlocked = true;
                            LobbySceneManager.Instance.SyncDataFromServer(payload.Game);
                            //  BoardManager.Instance.SetupBoardFromServer(payload.Game);
                        }

                      
                            AnimationManager.Instance.MovePawnToTheCaseId(card);

                        
                    }

                 
                    LobbySceneManager.Instance.ShowCardPicked();
                    //actualize card, if the car and unlock response for the player that got it
                    //BoardManager.Instance.RefreshUnlockedVisual(payload.IdCard); // your own UI hook
                }
                if (type == "cardAnswered")
                {
                    var payload = jobj["data"]?.ToObject<AnswerCardResponse>();
                    if (payload?.Game == null)
                    {
                        Debug.LogWarning("cardAnswered missing payload.Game");
                        return;
                    }

                    // Apply authoritative GameState
                    LobbySceneManager.Instance.SyncDataFromServer(payload.Game);
                  //  BoardManager.Instance.InitializeGameFromData(payload.Game);
                    //GameScript.Instance.SetCurrentGameState(payload.Game);

                    // Optional: UI hooks
                    // BoardManager.Instance.RefreshAfterAnswer();
                    // TurnManager.Instance.OnTurnAdvanced(payload.Game.CurrentPlayerId)
                    //
                    
                    if(payload == null)
                    {
                        var card = payload.Game.Board.FirstOrDefault(x=> x.Id ==  payload.Game.CurrentPosition);
                        if(card != null)
                        {
                            if(card.NeedProEvaluation && payload.Game.CurrentPlayerId == Authentificator.Instance.Id)
                            {
                                var newnotif = new NotificationTwily
                                {
                                    idNotification = new GUI().ToString(),
                                    notificationInfo = Authentificator.Instance.Username,
                                    typeNotification = TypeNotification.VALIDATION,
                                    idSalonNotif = LobbySceneManager.Instance.CurrentBigSalonId,
                                    idTeamNotif = LobbySceneManager.Instance.CurrentTeamId != null ? LobbySceneManager.Instance.CurrentTeamId : string.Empty,
                                    idUserNotif = Authentificator.Instance.Id,
                                    notificationTime = DateTime.Now
                                };
                            }
                        }
                    }

                    //check if i need too select a team, if im a player i need to chose my team depending of the manager
                    LobbySceneManager.Instance.CheckChoseoseFirstTeam();
                }
                if (type == "playerProfileCardsChosen")
                {
                    var payload = jobj["data"]?.ToObject<ChoseProfileResponse>();
                    if (payload?.Game == null) return;

                    LobbySceneManager.Instance.SyncDataFromServer(payload.Game);
               
                    if(payload.Game.Step == StepGameType.SELECTTEAM)
                    {
                        LobbySceneManager.Instance.StopSelectionProfile();
                    }


                    // If you want to move UI forward once profiles are set, handle here.
                }
                if (type == "notificationSent" || type == "notificationDeleted")
                {
                    var big = jobj["data"]?.ToObject<BigSalonInfo>();
                    if (big != null)
                    {
                        // Refresh local salon model/UI using BigSalonInfo.Notifications
                        //  LobbySceneManager.Instance?.OnBigSalonUpdated(big);
                        NotificationCenter.Instance.SetupNotificationCenter(big);
                    }
                }

                if (type == "cardValidatedAdmin")
                {
                    var payload = jobj["data"]?.ToObject<ValidateCardAdminResponse>();
                    if (payload?.Game == null) return;

                    LobbySceneManager.Instance.SyncDataFromServerMinimalist(payload.Game);


   
                }
                if (type == "budgetSubmitted")
                {
                    var payload = jobj["data"]?.ToObject<BudgetSubmittedResponse>();
                    if (payload?.Game == null) return;

                    LobbySceneManager.Instance.SyncDataFromServerMinimalist(payload.Game);
                    InGameMenuManager.Instance.DashBoard.SyncDashboardData();

                }

                if (type == "crisisSubmitted")
                {
                    var payload = jobj["data"]?.ToObject<CrisisSubmittedResponse>();
                    if (payload?.Game == null) return;

                    LobbySceneManager.Instance.SyncDataFromServerMinimalist(payload.Game);
                    InGameMenuManager.Instance.DashBoard.SyncDashboardData();
                }


                else if (type == "error")
                {
                    Debug.LogWarning("Server error: " + jobj["data"]?.ToString());
                }
            }
            catch(Exception e) {
                Debug.LogException(e);
            }
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

    public void ChoseCardOnGame(string idSalon, string teamId, int idCard)
    {
        var chosenCardRequest = new ChoseCardRequest
        {
            IdCard = idCard,
            IdSalon = idSalon,
            IdTeam = teamId
        };

        var envelope = new Envelope<ChoseCardRequest> { type = "choseCard", data = chosenCardRequest  };
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

    public void AnswerCard(string idBigSalon, string idTeam, CardData cardData)
    {
        AnswerCardRequest req = new AnswerCardRequest
        {
          IdSalon = idBigSalon,
          IdTeam = idTeam,
          CardAnsewerd = cardData,
          IdPlayerAnswered = Authentificator.Instance.Id
        };
        var envelope = new Envelope<AnswerCardRequest> { type = "answerCard", data = req };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }


    public void SelectTeamPlayer(List<CardData> chosenCards)
    {
        var req = new ChoseProfileRequest
        {
            CardsChosen = chosenCards,
            IdSalon = LobbySceneManager.Instance.CurrentBigSalonId,
            IdTeam = LobbySceneManager.Instance.CurrentTeamId,
            UserInfo = Authentificator.Instance.GetUser()
        };

        var envelope = new Envelope<ChoseProfileRequest> { type = "choseProfile", data = req };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }

    public void SendNotification(string salonId, NotificationTwily notifToSend)
    {
        var req = new SendNotificationRequest
        {
            IdSalon = salonId,
            Notification = notifToSend
        };
        var envelope = new Envelope<SendNotificationRequest> { type = "sendNotif", data = req };
        var json = JsonConvert.SerializeObject(envelope);
        _ = ws.SendText(json);
    }

    public void DeleteDotification(string salonId, string idNotifToDelete)
    {
        var req = new DeleteNotificationRequest
        {
            IdSalon = salonId,
            IdNotification = idNotifToDelete
        };
        // NOTE: was "sendNotif" by mistake—use "deleteNotif"
        var envelope = new Envelope<DeleteNotificationRequest> { type = "deleteNotif", data = req };
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


    public void ValidateCardAdmin(string currentBigSalonId, string currentTeamId, int cardId, EvaluationResult newState)
    {
        var req = new ValidateCardAdminRequest
        {
            IdSalon = currentBigSalonId,
            IdTeam = currentTeamId,
            CardId = cardId,
            NewState = newState
        };

        var env = new Envelope<ValidateCardAdminRequest> { type = "validateCardAdmin", data = req };
        var json = JsonConvert.SerializeObject(env);
        _ = ws.SendText(json);
    }


    public void SubmitBudgetToServer()
    {
        var budget = InGameMenuManager.Instance.DashBoard.GetBudgetValueFromBoard(); // your function
        var req = new SubmitBudgetRequest
        {
            IdSalon = LobbySceneManager.Instance.CurrentBigSalonId,
            IdTeam = LobbySceneManager.Instance.CurrentTeamId,
            Budget = budget,
            UserId = Authentificator.Instance.Id
        };

        var env = new Envelope<SubmitBudgetRequest> { type = "submitBudget", data = req };
        _ = ws.SendText(JsonConvert.SerializeObject(env));
    }

    public void SubmitCrisisToServer()
    {
        var crisis = InGameMenuManager.Instance.DashBoard.GetCrisisValue(); // your function
        var req = new SubmitCrisisRequest
        {
            IdSalon = LobbySceneManager.Instance.CurrentBigSalonId,
            IdTeam = LobbySceneManager.Instance.CurrentTeamId,
            Crisis = crisis,
            UserId = Authentificator.Instance.Id
        };

        var env = new Envelope<SubmitCrisisRequest> { type = "submitCrisis", data = req };
        _ = ws.SendText(JsonConvert.SerializeObject(env));
    }


}
