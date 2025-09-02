using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;

using NativeWebSocket;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Linq;
using System.Text.Json.Serialization;
using Unity.VisualScripting;
using SocketIOClient.Transport;
/// <summary>
/// Replaces the previous Socket.IO implementation with NativeWebSocket.
/// Connects to the ws-lobby-server (plain WebSockets) and handles salon creation, listing, joining, leaving, and deletion.
/// </summary>
public class LobbySceneManager : MonoBehaviour
{
    private static LobbySceneManager instance;

    public static LobbySceneManager Instance
    {
        get
        {
                if(instance == null)
            {
                instance = FindAnyObjectByType<LobbySceneManager>();
            }
                return instance;
        }
    }

    [SerializeField]
    private GameObject TeamChoiceObject;

    private string currentTeamId;

    public BigSalonInfo currentBigSalon;
    public GameStateData CurrentGameState { get => currentGameState; set => currentGameState = value; }
    public string CurrentTeamId { get => currentTeamId; set => currentTeamId = value; }
    public string CurrentBigSalonId { get => currentBigSalonId; set => currentBigSalonId = value; }

    [Header("UI References")]
    public TMP_Text statusText;
    public TMP_InputField createSalonInput;

    public TMP_Text pseudoConnecte;
    public TMP_Text roleConnecte;

    public TwilyButton createButton;
    public Transform salonsListContainer;
    public GameObject salonItemPrefab;  // Prefab with SalonItemUI component

    public GameObject salonUI;
    public GameObject gameUI;
    public GameObject welcomeUI;
    public GameObject bigSalonUI;


    private GameStateData currentGameState;


    private WebSocket ws;



    public TwilyButton startGameButton;  // Button to start the game (admin only)

    public ChatPanel chatPanel;

    public BigSalonInfo actualBigSalon;

    [SerializeField]
    public GameObject creationTeamObject;



    public bool IsPlayerInGame()
    {
        if (currentGameState == null)
            return false;

        foreach(var p in currentGameState.Players)
        {
            if(p.userInfo.Id == Authentificator.Instance.Id)
            {
                return true;
            }
        }
        return false;
    }
    private void Awake()
    {
        // Determine if current user is admin, mauybe check for multiple roles

        creationTeamObject.gameObject.SetActive(IsAdmin());

          if (startGameButton != null)
            startGameButton.gameObject.SetActive(IsPlayer());
     
        try
        {
            string pseudo = Authentificator.Instance.Username;// PlayerPrefs.GetString("user_name", "");
            string roles = Authentificator.Instance.Roles;
            pseudoConnecte.text = pseudo;
            roleConnecte.text = roles;
        }
        catch
        {
            Debug.Log("eror auth");
        }



        // rollButton.onClick.AddListener(OnRollDice);

        //big salon part
        if (openBigSalonPanelButton != null)
            openBigSalonPanelButton.OnClick.AddListener(ShowBigSalonPanel);

        if (bsBackToLobbyButton != null)
            bsBackToLobbyButton.OnClick.AddListener(() => {
                bigSalonPanel.SetActive(false);
                // show your normal lobby screen again
                ChangeScreen(GameScreen.LOBBY);
            });

        if (bsRefreshButton != null)
            bsRefreshButton.OnClick.AddListener(RequestBigSalonsList);

    }

    public void SendChat(string text)
    {
      
    }

    #region Roles
    public bool IsObserver()
    {
        string roles = Authentificator.Instance.Roles;//PlayerPrefs.GetString("user_roles", "");



            return roles.Contains("observer", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsAdmin()
    {
        string roles =  Authentificator.Instance.Roles;//PlayerPrefs.GetString("user_roles", "");

        return roles.Contains("administrator", StringComparison.OrdinalIgnoreCase);
    }


    private string[] GetUserRoles()
    {
        string roles =  Authentificator.Instance.Roles;//PlayerPrefs.GetString("user_roles", "");

        if (string.IsNullOrEmpty(roles))
            return new string[0]; // return empty array if nothing saved

        // split by comma, remove empty entries, trim spaces
        return roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToArray();
    }


    public bool IsPlayer()
    {
        string roles = Authentificator.Instance.Roles;//= PlayerPrefs.GetString("user_roles", "");

        return roles.Contains("player", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    public void SetupBigSalonUI(BigSalonInfo snapshot)
    {
        currentBigSalon = snapshot;
        NotificationCenter.Instance.SetupNotificationCenter(snapshot);
        BigSalonLobby.Instance.SetupData(snapshot);
    }

 
    private bool CheckAreaOneDone()
    {
        var areaone = CurrentGameState.AreaStates.Find(x => x.idArea == 1);
        foreach (var c in areaone.casesOnBoard)
        {
            if(!c.isVisited)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckAreaToNotStarted()
    {
        
            var areaone = CurrentGameState.AreaStates.Find(x => x.idArea == 2);
            foreach (var c in areaone.casesOnBoard)
            {
                if (c.isVisited)
                {
                    return false;
                }
            }
            return true;
        
    }


    public void CheckChoseoseFirstTeam()
    {
        if(CheckAreaOneDone() && CheckAreaToNotStarted() && IsPlayerInGame() && CurrentGameState.Step == StepGameType.PLAYCARD)
        {
            rollButton.gameObject.SetActive(false);
            TeamChoiceObject.SetActive(true);
             ProfileChoice.Instance.SetupProfileChoice();
        }
    }


    public void StopSelectionProfile()
    {
        if(CurrentGameState.CurrentPlayerId == Authentificator.Instance.Id)
        {
            rollButton.gameObject.SetActive(true);

        }

        TeamChoiceObject.SetActive(false);

    }


    private void SetActionButtons(bool interact)
    {
        rollButton.gameObject.SetActive(interact);
   //     bonnusButton.interactable = interact;
    }





    #region lobby

    [SerializeField]
    private TMP_InputField nameSalonEntry;
    [SerializeField]
    private TMP_InputField whiteListSalonEntry;
    /// <summary>
    /// Called by the Create button to request a new salon.
    /// </summary>
    public void OnCreateSalon()
    {

        List<string> whiteList = Helpers.Instance.ConvertStringToArray(whiteListSalonEntry.text);
        string creator = Authentificator.Instance.Username;
        if (!string.IsNullOrEmpty(creator) && !whiteList.Contains(creator))
        {
            whiteList.Add(creator);
        }
        string id = Guid.NewGuid().ToString();
        CreateTeam(CurrentBigSalonId, id, nameSalonEntry.text, whiteList);
    }
   

    private async void OnDeleteSalon(string salonId)
    {
        if (!IsAdmin())
        {
            statusText.text = "Not authorized to delete salons.";
            return;
        }
        var packet = new JObject { ["type"] = "deleteSalon", ["salonId"] = salonId };
        await ws.SendText(packet.ToString());
        statusText.text = $"Deleted salon '{salonId}'.";
    }

    
    private void ChangeScreen(GameScreen screen )
    {
        switch (screen)
        {
            case GameScreen.LOBBY:
                salonUI.gameObject.SetActive(false);
                gameUI.gameObject.SetActive(false);
                bigSalonUI.SetActive(true);
                welcomeUI.SetActive(false);


                break;
            case GameScreen.GAME:
                salonUI.gameObject.SetActive(false);
                gameUI.gameObject.SetActive(true);
                bigSalonUI.SetActive(false);
                welcomeUI.SetActive(false);
               
  

                break;
        }
    }

    public enum GameScreen
    {
        LOBBY,
        GAME
    }

   

    #endregion

    #region Game management
    public TMP_Text turnLabel;
    public TMP_Text turnInfoLabel;

    public TMP_Text scoreboardText;
    public TwilyButton rollButton;
 //   public Button bonnusButton;

  

    private string currentPlayerId = "";
 

    public void StartGame()
    {
        OnStartGame();
    }

    public async void OnStartGame()
    {
        if (!IsPlayer() || string.IsNullOrEmpty(currentTeamId)) return;


        StartGameRequest startRequest = new StartGameRequest
        {
            SalonId = actualBigSalon.Id,
            TeamId = CurrentTeamId,
            UserInfo = Authentificator.Instance.GetUser()
        };

        WsClient.Instance.AskStartGame(startRequest);


        turnLabel.text = "Attente des autres joueurs...";
    }




    void UpdateScoreboard(JToken players)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var player in players)
        {
            string name = player["name"].Value<string>();
            int score = player["score"].Value<int>();
            sb.AppendLine(name + ": " + score);
        }
        scoreboardText.text = sb.ToString();
    }

    public bool IsMyTurn()
    {
        return CurrentGameState.CurrentPlayerId == Authentificator.Instance.Id;
    }

    void UpdateTurnUI()
    {
        string myId = PlayerPrefs.GetInt("user_id", -1).ToString();
        bool isObserver = PlayerPrefs.GetString("user_roles").Contains("observer");

        if (CurrentGameState == null)
        {
            Debug.LogWarning("Game state not initialized.");
            SetActionButtons(false);
            turnLabel.text = "Chargement...";
            return;
        }

        // Déterminer si c'est mon tour
        var isMyTurn = IsMyTurn();

        // Vérifie si l'on peut choisir une carte ou non
        bool canPick = isMyTurn;

        if (isMyTurn)
        {
            int pos = CurrentGameState.CurrentPosition;

            // Si une carte est en cours
            if (pos >= 0)
            {
                var currentCard = CurrentGameState.Board.FirstOrDefault(c => c.Id == pos);
                if (currentCard != null)
                {
                    // On considère la carte "terminée" si elle est validée ou répondue (au choix selon ta logique)
                    bool isCompleted = !string.IsNullOrEmpty(currentCard.Response) || currentCard.AutoEvaluationResult != EvaluationResult.NONE;
                    canPick = isCompleted;
                }
                else
                {
                    Debug.LogWarning($"No card found at index {pos} in board.");
                    canPick = false;
                }
            }
        }

        // Appliquer l'état du bouton
        SetActionButtons(canPick);

        // Afficher texte de tour
        turnLabel.text = isObserver
            ? " Vous êtes observateur"
            : isMyTurn
                ? (canPick ? " Votre tour - Choisissez une carte" : " Répondez ou validez la carte en cours")
                : " En attente des autres joueurs...";
    }



    public void PickRandomCardInActiveArea()
    {
        var precedentArea = currentGameState.CurrentArea;
        var game = CurrentGameState;
        if (game == null || game.AreaStates == null) return;
        if(game.CurrentArea <= 0)
        {
            game.CurrentArea = 1;
        }
        // Find the current area
        var currentAreaObj = game.AreaStates.FirstOrDefault(a => a.idArea == game.CurrentArea);
        if (currentAreaObj == null) return;

        // Area is done if all its cases are visited
        bool allVisited = currentAreaObj.casesOnBoard != null &&
                          currentAreaObj.casesOnBoard.All(bc => bc.isVisited);

        if (allVisited)
        {
            // Order areas by id (so next area is predictable)
            var orderedAreas = game.AreaStates.OrderBy(a => a.idArea).ToList();
            int idx = orderedAreas.FindIndex(a => a.idArea == game.CurrentArea);

            if (idx >= 0 && idx + 1 < orderedAreas.Count)
            {
                game.CurrentArea = orderedAreas[idx + 1].idArea;
                Debug.Log($"Area {currentAreaObj.idArea} complete! Now moving to area {game.CurrentArea}");
            }


           /* else
            {
            
                game.Completed = true;
                Debug.Log(" All areas complete  game finished!");
            }*/
        }
        var activeArea = game.CurrentArea;
        if (!CanChangeArea(activeArea))
        {
            //set message that area cant change
            turnInfoLabel.text = "Besoin de validation par un intervenant";
            return;
        }


        if (activeArea == -1)
        {
            Debug.Log("All areas are completed!");
            return;
        }
      
        PickRandomCardInArea(activeArea);
     


    }


    public void PickRandomCardInArea(int areaId)
    {
        SetActionButtons(false);
        // Get area board

        if (areaId == 0)
            areaId = 1;
        var area = BoardManager.Instance.GetAreaById(areaId);


        // Filter to unvisited cases
        var unvisitedCases = AnimationManager.Instance.GetPossibleCases(areaId);

        if (unvisitedCases.Count == 0)
        {
            Debug.LogWarning($"[CARD PICK] No unvisited cases in area {areaId}");
            return;
        }

        // Pick random case
        var randomCase = unvisitedCases[UnityEngine.Random.Range(0, unvisitedCases.Count)];
   

        CardData card = randomCase.CaseData;


        if (card != null)
        {
            AnimationManager.Instance.AnimationSelectionCard(area, randomCase,   card);

           
        }


     

    }

    public void SendCardServerAfterSelection(CardData card)
    {
        WsClient.Instance.ChoseCardOnGame(CurrentBigSalonId, CurrentTeamId, card.Id);

    }





    public async void OnValidateAnswer(CardData card)
    {
        WsClient.Instance.AnswerCard(CurrentBigSalonId, CurrentTeamId, card);
    }




    public async void ChangeCardAdminState(int cardId, EvaluationResult newState)
    {
        WsClient.Instance.ValidateCardAdmin(currentBigSalonId, CurrentTeamId,  cardId, newState);
    }


    private bool CanChangeArea(int actualArea)
    {
        //check all teh cases

        if (actualArea == 0)
            return true;

        var area = BoardManager.Instance.GetAreaById(actualArea);
     


        foreach (var cases in area.CasesOnArea)
        {
            if (!cases.Visited)
                return true;
        }

        foreach (var cases in area.CasesOnArea)
        {
            if(cases.TypeCase == TypeCard.QUESTION && cases.CaseData != null)
            {
                if(cases.CaseData.NeedProEvaluation == true && cases.Visited && currentGameState.Board.FirstOrDefault(x => x.Id == cases.CaseData.Id).Unlocked && cases.CaseData.ProEvaluationResult == EvaluationResult.NONE)
                {
                    statusText.text = "En attente de validation par un intervenant";
                    return false;
                }

            }


        }
      



        return true;
    }


    public async void OnSendSharedMessage(string msgText)
    {
       /* if (string.IsNullOrEmpty(currentSalonId)) return;

        var packet = new JObject
        {
            ["type"] = "setSharedMessage",
            ["salonId"] = currentSalonId,
            ["message"] = msgText ?? ""
        };

        await ws.SendText(packet.ToString());*/
    }

    #endregion


    #region bigsalon



    private string currentBigSalonId = null;



    public async void LeaveBigSalon(string bigSalonId)
    {
        var pkt = new JObject { ["type"] = "leaveBigSalon", ["bigSalonId"] = bigSalonId };
        await ws.SendText(pkt.ToString());
        if (CurrentBigSalonId == bigSalonId) CurrentBigSalonId = null;
    }


    [SerializeField]
    private TMP_InputField nameBS;
        [SerializeField]
    private TMP_InputField descriptionBS;
        [SerializeField]
    private TMP_InputField whiteListBS;


    public void SendFormCreationBigSalon()
    {

        CloseFormBigSalonCreation();
        // 1. Generate a unique random ID (GUID is perfect for this)
        string id = Guid.NewGuid().ToString();

        // 2. Get name and description from UI
        string nameSalon = nameBS.text.Trim();
        string descriSalon = descriptionBS.text.Trim();

        // 3. Convert whitelist input into a List<string>, splitting by new lines
        List<string> whiteList = Helpers.Instance.ConvertStringToArray(whiteListBS.text);



        // 4. Add the creator (from PlayerPrefs) to whitelist if not already there
        string creator = PlayerPrefs.GetString("user_name", "");
        if (!string.IsNullOrEmpty(creator) && !whiteList.Contains(creator))
        {
            whiteList.Add(creator);
        }

        CreateBigSalon(id, nameSalon, descriSalon, whiteList);
    }

    public async void CreateBigSalon(string bigId, string name, string description, List<string> whiteList)
    {

        BigSalonInfo big = new BigSalonInfo
        {
            Id = bigId,
            Name = name,
            Description = description,
            WhiteList = whiteList ?? new List<string>()

        };

       WsClient.Instance.CreateSalon(big);
    }



    public async void RequestBigSalonsList()
    {
        WsClient.Instance.GetSalons();
    }

    public async void CreateTeam(string bigSalonId, string salonId, string name, List<string> whiteList)
    {
        SalonInfo teamToCreate = new SalonInfo
        {
            Id = salonId,
            Name = name,
            WhiteList = whiteList
        };

        WsClient.Instance.CreateTeamOnSalon(bigSalonId, teamToCreate);
    }
   
    // Delete a sub-salon from a Big Salon
    public async void DeleteSalon( string salonId)
    {
        /*string bigSalonId = currentBigSalonId;

        if (string.IsNullOrEmpty(bigSalonId) || string.IsNullOrEmpty(salonId)) return;

        var pkt = new JObject
        {
            ["type"] = "deleteSubSalon",
            ["bigSalonId"] = bigSalonId,
            ["salonId"] = salonId
        };

        await ws.SendText(pkt.ToString());*/
    }


    public async void JoinSalon(string salonId)
    {
        string bigSalonId = CurrentBigSalonId;

     

        if (string.IsNullOrWhiteSpace(bigSalonId) || string.IsNullOrWhiteSpace(salonId))
        {
            statusText.text = "Missing big salon or salon id.";
            return;
        }

        // remember where we are
        CurrentBigSalonId = bigSalonId;
        currentTeamId = salonId;

        var user = Authentificator.Instance.GetUser();

 

        try
        {
            WsClient.Instance.JoinTeamOnSalon(bigSalonId, user, IsPlayer(), salonId);
            // optimistic UI: show the game screen while we wait for server snapshot/events
            statusText.text = $"Joining salon '{salonId}'...";
            ChangeScreen(GameScreen.GAME);   // you said you'll manage starting/restoring the game
            turnLabel.text = "Waiting for game state...";
        }
        catch (Exception ex)
        {
            Debug.LogError($"JoinSalon send failed: {ex.Message}");
            statusText.text = "Failed to join salon.";
        }
    }


    [Header("Big Salon UI")]
    public GameObject bigSalonPanel;
    public TwilyButton bsBackToLobbyButton;
    public TwilyButton bsRefreshButton;

    public TMP_Text bsTitleText;          // TopBar Title
    public TMP_Text bsHeaderText;         // RightCol BigHeaderText

    public Transform bigSalonListContainer;     // LeftCol BigSalonList/Content

    public GameObject bigSalonListItemPrefab;   // BigSalonListItem.prefab


    // optional: a button in your existing lobby UI to open this panel
    public TwilyButton openBigSalonPanelButton;


    private List<BigSalonInfo> actualBigSalonsInfo;

    public void ShowBigSalonPanel()
    {
        ChangeScreen(GameScreen.LOBBY); // optional: hide game UI if needed
        bigSalonPanel.SetActive(true);
        bsTitleText.text = "Big Salons";
        RequestBigSalonsList();
    }


    public void RenderBigSalonList(List<BigSalonInfo> list)
    {
        if (!bigSalonPanel) return;
        Helpers.Instance.ClearContainer(bigSalonListContainer);

        actualBigSalonsInfo = list;

        // Current user pseudo
        string myPseudo = Authentificator.Instance.Username;
        bool isAdmin = IsAdmin() ;

        // Only show salons I am whitelisted for (server should also enforce)
        var visible = isAdmin
            ? list
            : list.Where(b => (b.WhiteList?.Any(w => string.Equals(w, myPseudo, StringComparison.OrdinalIgnoreCase)) ?? false)).ToList();


        ActualizeListBigSalons(visible);
    }

    private void ActualizeListBigSalons(List<BigSalonInfo> visible)
    {
        foreach (Transform child in bigSalonListContainer)
        {
            Destroy(child.gameObject);
        }


        foreach (var bs in visible)
        {
            var go = Instantiate(bigSalonListItemPrefab, bigSalonListContainer);
            var script = go.GetComponent<BigSalonListItem>();
            if(script != null)
            {
                script.Setup(bs);
            }
        }
    }



    public void QuitGame()
    {

        var leaveSalon = new LeaveSalonRequest
        {
            SalonId = CurrentBigSalonId,
            UserInfo = Authentificator.Instance.GetUser()
        };
        WsClient.Instance.LeaveSalon(leaveSalon);
        WsClient.Instance.LeaveTeamOnSalon(CurrentBigSalonId, Authentificator.Instance.GetUser(), IsPlayer(), CurrentTeamId);
    }

    [SerializeField]
    private GameObject bigSalonLobby;


    public void AskJoinBigSalonLobby(string id)
    {
        JoinSalonRequest joinSalonRequest = new JoinSalonRequest
        {
            UserInfo = new UserInfo
            {
                Id = Authentificator.Instance.Id,
                Name = Authentificator.Instance.Username,
                Roles = Authentificator.Instance.RolesArray
            },
            SalonId = id
        };

        WsClient.Instance.JoinSalon(joinSalonRequest);
    }

    public void JoinBigSalonLobby(string bigSalonId)
    {
        if (bigSalonId == null) return;

        CurrentBigSalonId = bigSalonId;

        foreach(var bs in actualBigSalonsInfo)
        {
            if(bs.Id == bigSalonId)
            {
                actualBigSalon = bs;
            }
        }


        // Show the big-salon UI (blank until snapshot arrives)
        bigSalonLobby.SetActive(true);
        SetupBigSalonUI(currentBigSalon);
    }


  

    [SerializeField]
    private GameObject formCreationBigSalon;

    public void OpenFormBigSalonCreation()
    {
        formCreationBigSalon.SetActive(true);
    }

    public void CloseFormBigSalonCreation()
    {
        formCreationBigSalon.SetActive(false);
    }

    #endregion

    #region choice role




    public GameStateData SetUpGameFromServer(GameStateData data)
    {
        var game = data;
        BoardManager.Instance.SetRules(data.GameRules);
        BoardManager.Instance.SetCards(data.Board);

        BoardManager.Instance.SetupAreaData();
        BoardManager.Instance.IsInitialized = true;
        game.AreaStates = BoardManager.Instance.AreasStateData;
        game.Active = true;
        return game;
    
    }

    public GameStateData SyncDataFromServerMinimalist(GameStateData data)
    {
        currentGameState = data;
        BoardManager.Instance.SyncCardFromServer(data);
        return data;
    }

    public GameStateData SyncDataFromServer(GameStateData data)
    {
        currentGameState = data;
        BoardManager.Instance.SyncCardFromServer(data);
        BoardManager.Instance.SetupBoardFromServer(data);
        if (data.CurrentPlayerId == Authentificator.Instance.Id)
        {
            turnLabel.text = "Tirez une carte";
            rollButton.gameObject.SetActive(true);
        }
        else
        {
            var playerToPlay = data.Players.FirstOrDefault(x => x.userInfo.Id == data.CurrentPlayerId).userInfo.Name;
            if(playerToPlay == null || playerToPlay == string.Empty)
            {
                playerToPlay = "Un autre joueur";
            }
            turnLabel.text = playerToPlay + " choisi une carte";
            rollButton.gameObject.SetActive(false);
        }

        return data;

    }

    public void ShowCardPicked()
    {
        //get card current position
        //show the data in the current position
        //enable the response for the player chosen
        BoardManager.Instance.ShowCardById(currentGameState.Board.FirstOrDefault(x => x.Id == currentGameState.CurrentPosition), IsMyTurn());
    }




    #endregion
}


