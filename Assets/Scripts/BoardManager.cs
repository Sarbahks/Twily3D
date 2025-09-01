using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.Json.Serialization;
using UnityEngine.Analytics;
using UnityEditor.Experimental.GraphView;

public class BoardManager : MonoBehaviour
{

    [SerializeField]
    private CanvasGroup cardCanvas;
    [SerializeField]
    private ActualCard cardInGame;

    private static BoardManager instance;

    public static BoardManager Instance
    {
        get
        {

            if (instance == null)
            {
                instance = FindAnyObjectByType<BoardManager>();
            }
            return instance;
        }
    }

    public List<CardData> Cards { get => cards; set => cards = value; }
    public List<AreaStateData> AreasStateData { get => areasStateData; set => areasStateData = value; }
    public bool IsInitialized { get => isInitialized; set => isInitialized = value; }

    private bool isInitialized = false;


    [SerializeField]
    private List<CardData> cards;

    private GameRulesData gameRules;
    public List<CardData> GetBoardData()
    {
        return Cards;
    }




    public CardData GetBoardcaseByIndex(int index)
    {
        return Cards.FirstOrDefault(x => x.Id == index);
    }

    //show the card in big so the player can submit
    internal void ShowCardById(CardData card, bool isPlayerTurn)
    {
        cardCanvas.alpha = 1;
        cardCanvas.interactable = isPlayerTurn;
        cardInGame.Initialize(card ,isPlayerTurn);
    }


    //maybe hide card once validate ??
    public void HideCard()
    {
        cardCanvas.alpha = 0;
        cardCanvas.interactable = false;
    }

    public void UpdateBoardCardsFromServer(GameStateData data)
    {/*
        var serverBoard = (JArray)serverBoardToken;

        for (int i = 0; i < serverBoard.Count; i++)
        {
            var serverCard = serverBoard[i];
            int id = serverCard["id"]?.Value<int>() ?? -1;

            var localCard = Cards.FirstOrDefault(c => c.Id == id);
            if (localCard == null) continue;

            // Met à jour uniquement les champs dynamiques
            localCard.Response = serverCard["response"]?.Value<string>() ?? "";

            var autoEvalStr = serverCard["autoEvaluationResult"]?.Value<string>() ?? "NONE";
            localCard.AutoEvaluationResult = Enum.TryParse(autoEvalStr, out EvaluationResult autoEval)
                ? autoEval
                : EvaluationResult.NONE;

            var proEvalStr = serverCard["proEvaluationResult"]?.Value<string>() ?? "NONE";
            localCard.ProEvaluationResult = Enum.TryParse(proEvalStr, out EvaluationResult proEval)
                ? proEval
                : EvaluationResult.NONE;

            localCard.Unlocked = serverCard["unlocked"]?.Value<bool>() ?? false;
        }*/
    }


    #region boardGameMovement

    public List<AreaBoard> areaBoards;
    public GameObject casePrefab;

    [SerializeField]
    private List<CaseBoard> casesOnBoard;



    [SerializeField]
    private float spacingMultiplier = 2f;
    public void PlaceCasesOnAllBoards()
    {
        if(gameRules == null)
            return;


        foreach (AreaBoard areaBoard in areaBoards)
        {
            // Find matching cases
            List<CardData> matchingCases = Cards
                .Where(c => c.IdArea.Equals(areaBoard.IdArea))
                .ToList();

            if (matchingCases.Count == 0)
                continue;

            PlaceCasesOnBoard(areaBoard, matchingCases);
        }
    }

    private void PlaceCasesOnBoard(AreaBoard areaBoard, List<CardData> matchingCards)
    {
        ClearPreviousCases(areaBoard);

        var areaRule = gameRules.AreaDatas.FirstOrDefault(r => r.AreaId == areaBoard.IdArea);
        if (areaRule == null)
        {
            Debug.LogWarning($"No rule found for Area ID {areaBoard.IdArea}");
            return;
        }

        // Prepare card lists
        var questionCards = GetLimitedCardList(matchingCards, TypeCard.QUESTION, areaRule.MaxCaseQuestion);
        var bonusCards = GetLimitedCardList(Cards, TypeCard.BONUS, areaRule.MaxCaseBonus);
        var profileCards = GetLimitedCardList(Cards, TypeCard.PROFILE, areaRule.MaxCaseProfile);
        var defiCards = GetLimitedCardList(Cards, TypeCard.DEFI, areaRule.MaxCaseDefi);
        var kpiCards = GetLimitedCardList(Cards, TypeCard.KPI, areaRule.MaxCaseKpi);
        var manageCards = GetLimitedCardList(Cards, TypeCard.PROFILMANAGEMENT, areaRule.MaxCaseProfileManagement);

        var allCards = new List<(List<CardData>, TypeCard)>
    {
        (questionCards, TypeCard.QUESTION),
        (bonusCards, TypeCard.BONUS),
        (profileCards, TypeCard.PROFILE),
        (defiCards, TypeCard.DEFI),
        (kpiCards, TypeCard.KPI),
        (manageCards, TypeCard.PROFILMANAGEMENT),
    };

        int totalCases = allCards.Sum(pair => pair.Item1.Count);

        // Generate positions
        List<Vector3> positions = GenerateSafePositions(areaBoard, totalCases);

        int posIndex = 0;

        foreach (var (cardList, typeCard) in allCards)
        {
            foreach (var card in cardList)
            {
                if (posIndex >= positions.Count)
                {
                    Debug.LogWarning("Not enough positions to place all cases.");
                    return;
                }

                Vector3 localPos = positions[posIndex++];
                Vector3 worldPos = areaBoard.transform.position + localPos;

                PlaceCardOnBoard(areaBoard, card, typeCard, worldPos, posIndex);
            }
        }



    }


  
    private List<AreaStateData> areasStateData;
    public void SetupAreaData()
    {
        int areaIds = 4;
        AreasStateData = new List<AreaStateData>();
        for (int i = 1; i<= areaIds; i++)
        {
            var area = GetAreaById(i);
            var newAreaState = new AreaStateData();
            newAreaState.idArea = i;
            newAreaState.casesOnBoard = new List<CaseStateData>();
            AreasStateData.Add(newAreaState);

            foreach (var c in area.CasesOnArea)
            {
                CaseStateData newCaseState = new CaseStateData
                {
                    idCardOn = c.CaseData.Id,
                    isVisited = c.Visited
                };

                newAreaState.casesOnBoard.Add(newCaseState);
            }
        }

 
    }

    private void ClearPreviousCases(AreaBoard areaBoard)
{
    foreach (Transform child in areaBoard.CaseArea.transform)
    {
        Destroy(child.gameObject);
    }

    areaBoard.CasesOnArea.Clear();
}

private List<CardData> GetLimitedCardList(IEnumerable<CardData> source, TypeCard type, int max)
{
    return source
        .Where(c => c.TypeCard == type)
        .OrderBy(_ => UnityEngine.Random.value)
        .Take(max)
        .ToList();
}

private void PlaceCardOnBoard(AreaBoard areaBoard, CardData card, TypeCard typeCard, Vector3 position, int index)
{
    var go = Instantiate(casePrefab, position, Quaternion.identity, areaBoard.CaseArea.transform);

    var script = go.GetComponent<CaseBoard>();
    if (script != null)
    {
        script.CaseData = card;
        script.TextCase.text = $"{areaBoard.IdArea}.{index}";
        script.TypeCase = typeCard;

        script.InitializeCase(typeCard, card, areaBoard.IdArea);

        casesOnBoard.Add(script);
        areaBoard.CasesOnArea.Add(script);
    }
}
    private List<Vector3> GenerateSafePositions(AreaBoard areaBoard, int totalCasesToPlace)
    {
        SphereCollider sphereCollider = areaBoard.ColliderSphere;
        float currentDiskRadius = sphereCollider.radius *
            Mathf.Max(areaBoard.transform.lossyScale.x, areaBoard.transform.lossyScale.z);

        BoxCollider boxCollider = casePrefab.GetComponent<BoxCollider>();
        Vector3 size = boxCollider.size;
        Vector3 prefabScale = casePrefab.transform.lossyScale;

        float caseWidth = size.x * prefabScale.x;
        float caseDepth = size.z * prefabScale.z;
        float cellSize = Mathf.Max(caseWidth, caseDepth) * spacingMultiplier;

        List<Vector3> positions = GenerateHexGrid(cellSize, currentDiskRadius);

        // Regenerate with scaling if not enough
        if (positions.Count < totalCasesToPlace)
        {
            float requiredArea = totalCasesToPlace * (cellSize * cellSize);
            float requiredRadius = Mathf.Sqrt(requiredArea / Mathf.PI);
            float scaleFactor = requiredRadius / currentDiskRadius;

            areaBoard.transform.localScale *= scaleFactor;
            currentDiskRadius *= scaleFactor;

            positions = GenerateHexGrid(cellSize, currentDiskRadius);
        }

        return positions;
    }



    private List<Vector3> GenerateHexGrid(float cellSize, float diskRadius)
    {
        List<Vector3> positions = new List<Vector3>();

        float rowHeight = cellSize * Mathf.Sqrt(3f) / 2f;

        int maxRows = Mathf.CeilToInt((diskRadius * 2) / rowHeight);

        for (int row = -maxRows; row <= maxRows; row++)
        {
            float z = row * rowHeight;

            float rowRadius = diskRadius - Mathf.Abs(z);
            int cellsInRow = Mathf.FloorToInt(rowRadius / cellSize);

            float offsetX = (row % 2 != 0) ? cellSize / 2f : 0f;

            for (int col = -cellsInRow; col <= cellsInRow; col++)
            {
                float x = col * cellSize + offsetX;

                if ((x * x + z * z) <= diskRadius * diskRadius)
                {
                    positions.Add(new Vector3(x, 0, z));
                }
            }
        }

        return positions;
    }




    public int GetCurrentActiveArea()
    {
        if (Cards == null || areaBoards == null) return 0; // safe default

        // Case 1: none of the cards are unlocked anywhere area 0
        if (!Cards.Any(c => c.Unlocked))
            return 0;

        // Case 2: return the first area that still has at least one locked card
        //foreach (var areaId in areaBoards.Select(a => a.IdArea))
        //{

        for(var i = 1; i < 5; i++)
        {
            bool hasLockedInArea = Cards.Any(c => c.IdArea == i && !c.Unlocked);
            if (hasLockedInArea)
                return i;
        }
        
        //}

        // Case 3: everything is unlocked
        return -1;
    }

    public AreaBoard GetAreaById(int idArea)
    {
        return areaBoards.FirstOrDefault(x => x.IdArea == idArea);
    }

    public List<CaseBoard> GetCardsAvaiableByArea(int area)
    {
        var areas = GetAreaById(area);

        var caseboard = casesOnBoard.FindAll(x => x.CaseData.IdArea == areas.IdArea);

        List<CaseBoard> caseDatas = new List<CaseBoard>();

        foreach (var card in caseboard)
        {
            if (card.CaseData.Unlocked)
            {
                caseDatas.Add(card);
            }
        }


        return caseDatas;
    }


    public bool IsAreaCompleted(int areaId)
    {
        return BoardManager.Instance.Cards
            .Where(c => c.IdArea == areaId)
            .All(c => c.Unlocked);
    }

    public CaseBoard GetCaseBoardFromData(CardData selected)
    {
        return casesOnBoard.FirstOrDefault(x => x.CaseData.Id == selected.Id);
    }

    public void SetRules(GameRulesData rules)
    {
        gameRules = rules;
    }

    public void SetCards(List<CardData> cards)
    {

            // Step 1: Separate cards by type
            var questionCards = cards
                .Where(c => c.TypeCard == TypeCard.QUESTION)
                .OrderBy(c => c.IdArea)
                .ThenBy(c => c.Title) // fallback to sort consistently
                .ToList();

            var otherCards = cards
                .Where(c => c.TypeCard != TypeCard.QUESTION)
                .OrderBy(c => c.TypeCard) // sort BONUS before PROFILE, etc.
                .ThenBy(c => c.Title)
                .ToList();

            // Step 2: Combine them
            var combined = new List<CardData>();
            combined.AddRange(questionCards);
            combined.AddRange(otherCards);
        /*
            // Step 3: Assign reproducible IDs
            for (int i = 0; i < combined.Count; i++)
            {
                combined[i].id = i;
            }*/

            // Step 4: Save internally
            this.cards = combined;


        PlaceCasesOnAllBoards();


    }

    public void SetUpActualCard(CardData cardPicked, GameStateData gameData, bool isMyTurn)
    {
        ShowCardById(cardPicked, isMyTurn);
    }

    public void InitializeGameFromData(GameStateData game)
    {
        IsInitialized = true;
        Cards = game.Board;               // List<CardData>
        areasStateData = game.AreaStates; // List<AreaStateData>
        if (areasStateData == null || Cards == null) return;

        foreach (var area in areasStateData)
            PlaceAreaFromGameData(area);
    }

    private void PlaceAreaFromGameData(AreaStateData area)
    {
        List<CardData> cardsOnTheArea = new List<CardData>();

        foreach(var caseBoard in area.casesOnBoard)
        {
            var cardCorrespondingToCase = Cards.Find(x => x.Id == caseBoard.idCardOn);
            if(cardCorrespondingToCase != null)
            {
                cardsOnTheArea.Add(cardCorrespondingToCase);
            }

        }

        PlaceCasesOnBoardFromServer(GetAreaById(area.idArea), cardsOnTheArea);

    }



    private void PlaceCasesOnBoardFromServer(AreaBoard areaBoard, List<CardData> matchingCards)
    {
        ClearPreviousCases(areaBoard);

        SetupVisualArea(areaBoard, matchingCards);

        // Prepare card lists
        var questionCards = matchingCards.Where(x => x.TypeCard == TypeCard.QUESTION).ToList();
        var bonusCards = matchingCards.Where(x => x.TypeCard == TypeCard.BONUS).ToList();
        var profileCards = matchingCards.Where(x => x.TypeCard == TypeCard.PROFILE).ToList(); 
        var defiCards = matchingCards.Where(x => x.TypeCard == TypeCard.DEFI).ToList();
        var kpiCards = matchingCards.Where(x => x.TypeCard == TypeCard.KPI).ToList(); 
        var manageCards = matchingCards.Where(x => x.TypeCard == TypeCard.PROFILMANAGEMENT).ToList(); 
        var allCards = new List<(List<CardData>, TypeCard)>
    {
        (questionCards, TypeCard.QUESTION),
        (bonusCards, TypeCard.BONUS),
        (profileCards, TypeCard.PROFILE),
        (defiCards, TypeCard.DEFI),
        (kpiCards, TypeCard.KPI),
        (manageCards, TypeCard.PROFILMANAGEMENT),
    };

        int totalCases = allCards.Sum(pair => pair.Item1.Count);

        // Generate positions
        List<Vector3> positions = GenerateSafePositions(areaBoard, totalCases);

        int posIndex = 0;

        foreach (var (cardList, typeCard) in allCards)
        {
            foreach (var card in cardList)
            {
                if (posIndex >= positions.Count)
                {
                    Debug.LogWarning("Not enough positions to place all cases.");
                    return;
                }

                Vector3 localPos = positions[posIndex++];
                Vector3 worldPos = areaBoard.transform.position + localPos;

                PlaceCardOnBoard(areaBoard, card, typeCard, worldPos, posIndex);
            }
        }
    }

    private void SetupVisualArea(AreaBoard areaBoard, List<CardData> matchingCards)
    {
        if (areaBoard == null) return;

        // true if any matching card is unlocked
        bool anyUnlocked = matchingCards != null
            && matchingCards.Any(c => c != null && c.Unlocked); 

        areaBoard.IsActive = anyUnlocked;
    }

    public void SyncCardFromServer(GameStateData data)
    {
        //syc cards, sync cases, sync team and other ui
        Cards = data.Board;

        InGameMenuManager.Instance.ActualizeCardsUI(Cards);
    }


    #endregion

}

