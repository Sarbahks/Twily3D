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
    public async void AnimationSelectionCard(AreaBoard area, CaseBoard finalCase, CardData card)
    {
        var casesArea = area.CasesOnArea.Where(x => !x.Visited).ToList();

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
        MovePawnToTheCase(finalCase);

        finalCase.Visited = true;

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


    public bool CaseMatchCard(CardData card, CaseBoard caseBoard) 
    {
        if (caseBoard.TypeCase == TypeCard.QUESTION)
        {
            if (card.Id == caseBoard.CaseData?.Id)
            {
                return true;
            }
            return false;
        }
        else if (caseBoard.TypeCase == card.TypeCard)
        {
            return true;
        }
        return false;  
    }


    /*
         int questionCasesNumber = casesarea.Where(x => x.TypeCase == TypeCard.QUESTION).ToList().Count;
        int bonusCasesNumber = casesarea.Where(x => x.TypeCase == TypeCard.QUESTION).ToList().Count;
        int defiCasesNumber = casesarea.Where(x => x.TypeCase == TypeCard.QUESTION).ToList().Count;
        int kpiCasesNumber = casesarea.Where(x => x.TypeCase == TypeCard.QUESTION).ToList().Count;
        int profileCasesNumber = casesarea.Where(x => x.TypeCase == TypeCard.QUESTION).ToList().Count;
        int profileManagementCasesNumber = casesarea.Where(x => x.TypeCase == TypeCard.QUESTION).ToList().Count;
*/



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

    
    public void MovePawnToTheCase(CaseBoard caseBoard)
    {

        CameraTwily.Instance.SetFocusPlayer(true);


 //       randomBoxe.SetActive(false);

        if (pawn == null || caseBoard == null)
            return;

        MoveFromAnAreaToAnOther(caseBoard);
        CameraTwily.Instance.SetFocusPlayer(false);
    }

    public void MovePawnToTheCaseAsClient(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("MovePawnToTheCaseAsClient called with null card.");
            return;
        }

        if (pawn == null)
        {
            if (!gameStarted)
                SpawnPawn();

            if (pawn == null)
            {
                Debug.LogWarning("Pawn still null after SpawnPawn.");
                return;
            }
        }

        var board = BoardManager.Instance;
        if (board == null)
        {
            Debug.LogError("BoardManager.Instance is null.");
            return;
        }

        // IMPORTANT: use the area id from the card, not the card id.
        // Replace 'card.areaId' with your actual field name that represents the area.
        var area = board.GetAreaById(card.IdArea);
        if (area == null || area.CasesOnArea == null || area.CasesOnArea.Count == 0)
        {
            Debug.LogWarning($"Area not found or empty for areaId={card.IdArea}.");
            return;
        }

        CaseBoard caseBoard = null;

        foreach (var caseb in area.CasesOnArea)
        {
            if (card.TypeCard == TypeCard.QUESTION)
            {
                // If your card has a dedicated case id, prefer that (e.g., card.caseId).
                // Fallback to card.id only if that is really the target case id.
                var targetCaseId = card.Id != 0 ? card.Id : card.Id;

                if (caseb.CaseData != null && caseb.CaseData.Id == targetCaseId)
                {
                    caseBoard = caseb;
                    break; // leave the loop after a match
                }
            }
            else
            {
                if (caseb.TypeCase == card.TypeCard)
                {
                    caseBoard = caseb;
                    break; // leave the loop after a match
                }
            }
        }

        if (caseBoard == null)
        {
            Debug.LogWarning($"No matching case in area {card.IdArea} for card {card.Id} ({card.TypeCard}).");
            return;
        }

        caseBoard.Visited = true;

        MovePawnToTheCase(caseBoard);
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
    {
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

        // finally, go to the case
        yield return StartCoroutine(MoveToPosition(caseBoard.transform.position, moveSpeed));


        caseBoard.Unlock();

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
