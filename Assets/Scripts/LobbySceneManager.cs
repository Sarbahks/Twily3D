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

    private string currentTeamId;

    public BigSalonInfo currentBigSalon;
    public GameStateData CurrentGameState { get => currentGameState; set => currentGameState = value; }
    public string CurrentTeamId { get => currentTeamId; set => currentTeamId = value; }

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

    private bool isAdmin;

    public TwilyButton startGameButton;  // Button to start the game (admin only)

    public ChatPanel chatPanel;

    public BigSalonInfo actualBigSalon;


    public static readonly string _salonCreation = "createSalon";
    public static readonly string _salonDeletion = "deleteSalon";
    public static readonly string _salonJoin = "joinSalon";
    public static readonly string _salonLeave = "leaveSalon";

    public static readonly string _answerSubmission = "submitAnswer";


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
        string roles = Authentificator.Instance.Roles;//PlayerPrefs.GetString("user_roles", "");
        isAdmin = roles.Contains("administrator", StringComparison.OrdinalIgnoreCase);
        createButton.gameObject.SetActive(isAdmin);

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(isAdmin);

        string pseudo = Authentificator.Instance.Username;// PlayerPrefs.GetString("user_name", "");
    
        pseudoConnecte.text = pseudo;
        roleConnecte.text = roles;



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

    public bool IsMyTurn(GameStateData stateData)
    {
        string myId = PlayerPrefs.GetInt("user_id", -1).ToString();
        bool isObserver = PlayerPrefs.GetString("user_roles").Contains("observer");
        return (myId == CurrentGameState.CurrentPlayerId.ToString() && !isObserver);
    }


    private void HandleCardPicked(CardData cardPicked, GameStateData gameData, bool isMyTurn)
    {
        if (!isMyTurn)
        {
            AnimationManager.Instance.MovePawnToTheCaseAsClient(cardPicked);
        }


       BoardManager.Instance.SetUpActualCard(cardPicked, gameData, isMyTurn);
    }

    private void InitializeGamePayload(GameRulesData rules, List<CardData> cards)
    {
        BoardManager.Instance.SetRules(rules);
        BoardManager.Instance.SetCards(cards);


        Debug.Log("Rules have been setup");
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
        CreateTeam(currentBigSalonId, id, nameSalonEntry.text, whiteList);
    }
   

    private async void OnDeleteSalon(string salonId)
    {
        if (!isAdmin)
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
    private bool isMyTurn = false;

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



    void HandleGameUpdate(GameStateData gameState)
    {
        CurrentGameState = gameState;


        if (CurrentGameState == null)
        {
            Debug.LogError("Failed to parse GameStateData");
            return;
        }

        currentPlayerId = CurrentGameState.CurrentPlayerId.ToString();

        // UI updates
        UpdateScoreboard(JToken.FromObject(CurrentGameState.Players)); // ou adapte UpdateScoreboard pour prendre List<PlayerData>!
        UpdateTurnUI();

        BoardManager.Instance.UpdateBoardCardsFromServer(gameState);

        

        if (CurrentGameState.Completed)
        {
            turnLabel.text = " Le jeu est terminé !";
            SetActionButtons(false);
            return;
        }


        InGameMenuManager.Instance.TextShared.text = CurrentGameState.SharedMessage;

        UpdateAreaActive();
        
    }


    private void UpdateAreaActive()
    {
        //check active area
        //check if the cases  are validated
        //if complete, go to the las one
        //currentGameState.completed

        var areaId = BoardManager.Instance.GetCurrentActiveArea();

        var area = BoardManager.Instance.GetAreaById(areaId);

        if (area == null)
            return;
        area.SetActive();
        bool areaComplete = true;


 

        foreach(var caseBoard in area.CasesOnArea)
        {
            if (!caseBoard.Visited)
            {
                areaComplete = false ;
            }
        }

        if (areaComplete)
        {
            //go to next area
            area.Clear = true;


        }

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
        isMyTurn = (myId == CurrentGameState.CurrentPlayerId.ToString() && !isObserver);

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
        int activeArea = BoardManager.Instance.GetCurrentActiveArea();


        if (!CanChangeArea(activeArea))
        {
            return;
        }


        if (activeArea == -1)
        {
            Debug.Log("All areas are completed!");
            return;
        }

        SetActionButtons(false);

        PickRandomCardInArea(activeArea);
    }


    public void PickRandomCardInArea(int areaId)
    {
        if (!isMyTurn || string.IsNullOrEmpty(currentTeamId))
            return;

        // Get area board

        if (areaId == 0)
            areaId = 1;
        var area = BoardManager.Instance.GetAreaById(areaId);
        var listCase = area.CasesOnArea;

        // Filter to unvisited cases
        var unvisitedCases = listCase.Where(c => !c.Visited).ToList();

        if (unvisitedCases.Count == 0)
        {
            Debug.LogWarning($"[CARD PICK] No unvisited cases in area {areaId}");
            return;
        }

        // Pick random case
        var randomCase = unvisitedCases[UnityEngine.Random.Range(0, unvisitedCases.Count)];
   

        CardData card = null;

        if (randomCase.TypeCase == TypeCard.QUESTION && randomCase.CaseData != null)
        {
            // Use the attached card
            card = randomCase.CaseData;
        }
        else
        {
            // Need to pick a card manually
            var desiredType = randomCase.TypeCase;

            var possibleCards = BoardManager.Instance.Cards
                .Where(cd => cd.TypeCard == desiredType && !cd.Unlocked)
                .ToList();

            if (possibleCards.Count == 0)
            {
                Debug.LogWarning($"[CARD PICK] No available {desiredType} cards in area {areaId}");
                return;
            }

            // Pick one randomly
            card = possibleCards[UnityEngine.Random.Range(0, possibleCards.Count)];
            card.Unlocked = true;

            // Optionally assign it to the case so we can access it later if needed
            randomCase.CaseData = card;
        }

        if (card != null)
        {
            AnimationManager.Instance.AnimationSelectionCard(area, randomCase,   card);

           
        }
    }

    public void SendCardServerAfterSelection(CardData card)
    {
        /*var packet = new JObject
            {
                ["type"] = "pickCard",
                ["salonId"] = currentSalonId,
                ["cardIndex"] = card.id
        };

        _ = ws.SendText(packet.ToString());*/

    }





    public async void OnValidateAnswer(string answerInput, int id)
    {
        /*if (!isMyTurn || string.IsNullOrEmpty(currentSalonId)) return;

 // helper method to find the board index
        string answer = answerInput.Trim();

        var packet = new JObject
        {
            ["type"] = _answerSubmission,
            ["salonId"] = currentSalonId,
            ["index"] = id,
            ["response"] = answer
        };

        await ws.SendText(packet.ToString());*/
     //   validateButton.interactable = false; // Prevent double click
    }




    public async void ChangeCardAdminState(int cardId, EvaluationResult newState)
    {/*
        if (string.IsNullOrEmpty(currentSalonId)) { Debug.LogWarning("No salon selected."); return; }
        if (!IsAdmin()) { Debug.LogWarning("Only admins can change card state."); return; }

        var packet = new JObject
        {
            ["type"] = "changeCardAdminState",
            ["salonId"] = currentSalonId,
            ["cardId"] = cardId,
            ["proEvaluationResult"] = newState.ToString() // "NONE","WAITING","BAD","MID","GOOD"
        };

        await ws.SendText(packet.ToString());
        Debug.Log($"[ADMIN] Requested proEvaluationResult change: card={cardId}, value={newState}");*/
    }


    private bool CanChangeArea(int actualArea)
    {
        //check all teh cases

        if (actualArea == 0)
            return true;

        var area = BoardManager.Instance.GetAreaById(actualArea);
        bool canChangeArea = true;


        foreach(var cases in area.CasesOnArea)
        {
            if(cases.TypeCase == TypeCard.QUESTION && cases.CaseData != null)
            {
                if(cases.CaseData.NeedProEvaluation == true && cases.CaseData.ProEvaluationResult != EvaluationResult.GOOD)
                {
                    statusText.text = "En attente de validation par un intervenant";
                    return false;
                }

            }
        }
        //check question on the cards, check if questions need to be valisated by an admin



        return canChangeArea;
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
        if (currentBigSalonId == bigSalonId) currentBigSalonId = null;
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
        string bigSalonId = currentBigSalonId;

     

        if (string.IsNullOrWhiteSpace(bigSalonId) || string.IsNullOrWhiteSpace(salonId))
        {
            statusText.text = "Missing big salon or salon id.";
            return;
        }

        // remember where we are
        currentBigSalonId = bigSalonId;
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
            SalonId = currentBigSalonId,
            UserInfo = Authentificator.Instance.GetUser()
        };
        WsClient.Instance.LeaveSalon(leaveSalon);
        WsClient.Instance.LeaveTeamOnSalon(currentBigSalonId, Authentificator.Instance.GetUser(), IsPlayer(), CurrentTeamId);
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

        currentBigSalonId = bigSalonId;

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

    public GameStateData SyncDataFromServer(GameStateData data)
    {
        currentGameState = data;
        BoardManager.Instance.SyncCardFromServer(data);


        return data;

    }



    #endregion
}


