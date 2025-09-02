using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class AnimationManager : MonoBehaviour
{
    private static AnimationManager instance;

    public static AnimationManager Instance
    {
        get
        { if(instance == null)
            {
                instance = FindAnyObjectByType<AnimationManager>();
            }
            
            
            return instance; }
    }

    private JObject lastGameState;



    public void LogTurnResume(JObject currentGame)
    {
        if (currentGame == null) return;

        // Handle wrapped message with "data" field
        if (currentGame.ContainsKey("data"))
            currentGame = (JObject)currentGame["data"];

        var players = currentGame["players"];
        var board = currentGame["board"];

        if (players == null || board == null)
        {
            Debug.LogWarning("No players or board in game state.");
            return;
        }

        Debug.Log("=== FULL GAME STATE RESUME ===");

        foreach (var player in players)
        {
            string name = player["name"].Value<string>();
            int score = player["score"].Value<int>();
            int position = player["position"].Value<int>();

            string caseDesc = "-";
            if (position >= 0 && position < board.Count())
            {
                var boardCase = board[position];
                string question = boardCase["question"].Value<string>();
                string response = boardCase["response"].Value<string>();
                bool validated = boardCase["validate"].Value<bool>();
                caseDesc = $"Question: {question}\n   Response: {response} (Validated: {validated})";
            }

            Debug.Log($"Player: {name}\n Score: {score}\n Position: {position}\n {caseDesc}");
        }

        if (lastGameState != null)
        {
            Debug.Log("=== LAST TURN DIFF ===");
            foreach (var player in players)
            {
                var previous = lastGameState["players"]?.FirstOrDefault(p => p["id"].Value<string>() == player["id"].Value<string>());
                if (previous == null) continue;

                int oldPos = previous["position"].Value<int>();
                int newPos = player["position"].Value<int>();
                int oldScore = previous["score"].Value<int>();
                int newScore = player["score"].Value<int>();

                if (oldPos != newPos || oldScore != newScore)
                {
                    Debug.Log($"{player["name"]}: Moved from {oldPos} to {newPos}, Score: {oldScore} -> {newScore}");
                }
            }
        }

        lastGameState = (JObject)currentGame.DeepClone();
    }

    [SerializeField]
    private bool gameStarted = false;

    [SerializeField]
    private GameObject pawn;

    [SerializeField]
    private GameObject randomBoxe;

    [SerializeField]
    private TextMeshProUGUI randomBoxeText;

    [SerializeField]
    private GameObject questionObject;
    [SerializeField]
    private GameObject bonusObject;
    [SerializeField]
    private GameObject defiObject;
    [SerializeField]
    private GameObject kpiObject;
    [SerializeField]
    private GameObject profileObject;
    [SerializeField]
    private GameObject profileManagementObject;

    [SerializeField]
    private int randomIteration;

    [SerializeField]
    private bool test;
    private void Update()
    {
        if (test)
        {
            test = false;
            GetPossibleCases(1);
            GetPossibleCases(2);
            GetPossibleCases(3);
            GetPossibleCases(4);
            GetPossibleCases(5);
        }
    }

    public List<CaseBoard> GetPossibleCases(int areaId)
    {
        List<CaseBoard> caseBoards = new List<CaseBoard>();
        var area = LobbySceneManager.Instance.CurrentGameState.AreaStates.FirstOrDefault(x => x.idArea == areaId);

        foreach(var cb in area.casesOnBoard)
        {
            var correspondingCard = LobbySceneManager.Instance.CurrentGameState.Board.FirstOrDefault(x => x.Id == cb.idCardOn);
            if (correspondingCard != null)
            {
                if(correspondingCard.Unlocked)
                {
                    BoardManager.Instance.GetBoardCaseByCardId(correspondingCard.Id).Unlock();
                }
                else
                {
                   var toadd = BoardManager.Instance.GetBoardCaseByCardId(correspondingCard.Id);
                    if(toadd != null)
                    {
                        caseBoards.Add(toadd);
                    }
                }
            }
        }

        return caseBoards;
    }

    public async void AnimationSelectionCard(AreaBoard area, CaseBoard finalCase, CardData card)
    {
        var casesArea = GetPossibleCases(area.IdArea);

        if (casesArea.Count == 0)
        {
            Debug.LogWarning("[ANIMATION] No cases available to animate.");
            LobbySceneManager.Instance.SendCardServerAfterSelection(card);
            return;
        }

        int actualIteration = 0;
        CaseBoard caseSelected = null;

        randomBoxe.SetActive(true);

        while (actualIteration < randomIteration)
        {
            caseSelected = casesArea[UnityEngine.Random.Range(0, casesArea.Count)];

            ShowCaseVisual(caseSelected, $"Tirage {actualIteration + 1}/{randomIteration}");

            await Task.Delay(50);
            actualIteration++;
        }

        // Final flash
        ShowCaseVisual(finalCase, "Carte sélectionnée !");
        await Task.Delay(1000);

        randomBoxe.SetActive(false);
    



        LobbySceneManager.Instance.SendCardServerAfterSelection(card);


    }

    private void ShowCaseVisual(CaseBoard caseBoard, string label)
    {
        DesactivateAllVisualsWheel();

        switch (caseBoard.TypeCase)
        {
            case TypeCard.QUESTION:
                randomBoxeText.text = caseBoard.TextCase.text;
                questionObject.SetActive(true);
                break;
            case TypeCard.BONUS:
                bonusObject.SetActive(true);
                break;
            case TypeCard.PROFILE:
                profileObject.SetActive(true);
                break;
            case TypeCard.DEFI:
                defiObject.SetActive(true);
                break;
            case TypeCard.KPI:
                kpiObject.SetActive(true);
                break;
            case TypeCard.PROFILMANAGEMENT:
                profileManagementObject.SetActive(true);
                break;
        }
     
       // randomBoxeText.text = label;
    }




    private void DesactivateAllVisualsWheel()
    {
        questionObject.SetActive(false);
         bonusObject.SetActive(false);
         defiObject.SetActive(false);
         kpiObject.SetActive(false);
        profileObject.SetActive(false);
        profileManagementObject.SetActive(false);
    }


  




    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private CameraTwily cameraController;



    public void SpawnPawn()
    {
        gameStarted = true;

        var area = BoardManager.Instance.GetAreaById(1);
        if (area == null)
        {
            Debug.LogError("Area with ID 1 not found.");
            return;
        }

        if (area.Entry == null)
        {
            Debug.LogError("Area.Entry is not assigned.");
            return;
        }

        if (pawn == null)
        {
            Debug.LogError("Pawn is not assigned.");
            return;
        }

        pawn.transform.position = area.Entry.position;
        pawn.SetActive(true);
    }

    
    public void MovePawnToTheCaseId(CardData card)
    {

        CameraTwily.Instance.SetFocusPlayer(true);

        var caseboard = BoardManager.Instance.GetCaseBoardFromData(card);

 //       randomBoxe.SetActive(false);

        if (pawn == null || caseboard == null)
            return;

        MoveFromAnAreaToAnOther(caseboard);
        CameraTwily.Instance.SetFocusPlayer(false);

        
    }




    #region pawMovement pard

    [Header("References")]
    [SerializeField] private Material lineMaterial;

    [Header("Settings")]
    [SerializeField] private float shrinkScale = 0.3f;
    [SerializeField] private float shrinkDuration = 0.5f;
    [SerializeField] private float lineDrawDelay = 0.02f;

    private LineRenderer travelLine;





    private Coroutine _moveCo;

    // your existing API (caller stays the same)
    public void MoveFromAnAreaToAnOther(CaseBoard caseBoard)
    {
        if (caseBoard == null || pawn == null) return;

        if (_moveCo != null) StopCoroutine(_moveCo);
        _moveCo = StartCoroutine(MoveFlow(caseBoard));
    }

    // exit(current) -> entry(target) -> case (straight lines, XZ only)
    private IEnumerator MoveFlow(CaseBoard caseBoard)
    {/*
        int areaIdToGo = caseBoard.CaseData.IdArea;
        int actualArea = BoardManager.Instance.GetCurrentActiveArea();
        if (actualArea == 0) actualArea = 1;

        // different area? go to Exit then Entry
        if (areaIdToGo != actualArea)
        {
            var from = BoardManager.Instance.GetAreaById(actualArea);
            var to = BoardManager.Instance.GetAreaById(areaIdToGo);

            if (from != null && from.Exit != null)
                yield return StartCoroutine(MoveToPosition(from.Exit.position, moveSpeed));

            if (to != null && to.Entry != null)
                yield return StartCoroutine(MoveToPosition(to.Entry.position, moveSpeed));
        }
        */
        // finally, go to the case
        yield return StartCoroutine(MoveToPosition(caseBoard.transform.position, moveSpeed));


      

        Debug.Log("Arrived to the case");
        _moveCo = null;
    }

    // straight-line move for the pawn, locking Y to pawn's current height
    private IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        // keep current pawn Y
        float y = pawn.transform.position.y;
        target = new Vector3(target.x, y, target.z);

        // if already there, skip
        if ((pawn.transform.position - target).sqrMagnitude <= 0.01f)
            yield break;

        while ((pawn.transform.position - target).sqrMagnitude > 0.01f)
        {
            pawn.transform.position = Vector3.MoveTowards(pawn.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        pawn.transform.position = target; // snap
    }





    private IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(pawn.transform.position, target) > 0.05f)
        {
            pawn.transform.position = Vector3.MoveTowards(pawn.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        pawn.transform.position = target;
    }

    private IEnumerator ScalePawn(Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = pawn.transform.localScale;

        while (elapsed < duration)
        {
            pawn.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        pawn.transform.localScale = targetScale;
    }

    private Vector3[] GetNavMeshPath(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            return path.corners;
        }
        return null;
    }
    #endregion

}
