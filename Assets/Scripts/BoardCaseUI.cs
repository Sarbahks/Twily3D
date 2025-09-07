using Newtonsoft.Json.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardCaseUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private CardData boardCard;

    [SerializeField]
    private int id;

    [SerializeField]
    private TextMeshProUGUI title;

    [SerializeField]
    private TextMeshProUGUI question;
    [SerializeField]
    private TextMeshProUGUI consigne;



    [SerializeField]
    private TMP_InputField responseInput;

    [SerializeField]
    private GameObject StatusAdmin;

    [SerializeField]
    private GameObject StatusPlayer;

    [SerializeField]
    private GameObject bgType1;
    
    [SerializeField]
    private GameObject bgType2;
    
    [SerializeField]
    private GameObject bgType3;
    
    [SerializeField]
    private GameObject bgType4;
        [SerializeField]
    private GameObject bgType5;

        [SerializeField]
    private GameObject bgBonus;
        [SerializeField]
    private GameObject bgDefi;
         
    [SerializeField]
    private GameObject bgKpi;

        [SerializeField]
    private GameObject bgProfileManagement;

 






    public CardData BoardCard { get => boardCard; set => boardCard = value; }
    public int Id { get => id; set => id = value; }
    public TextMeshProUGUI Title { get => title; set => title = value; }
    public TMP_InputField ResponseInput { get => responseInput; set => responseInput = value; }

    [SerializeField]
    private GameObject greyEffect;


    [SerializeField]
    private TextMeshProUGUI statusPlayer;

    [SerializeField]
    private TextMeshProUGUI statusAdmin;


    [SerializeField]
    private GameObject noneIconPlayer;
        [SerializeField]
    private GameObject waitIconPlayer;
        [SerializeField]
    private GameObject badIconPlayer;
        [SerializeField]
    private GameObject midIconPlayer;
        [SerializeField]
    private GameObject goodIconPlayer;

    
    [SerializeField]
    private GameObject noneIconAdmin;
        [SerializeField]
    private GameObject waitIconAdmin;
        [SerializeField]
    private GameObject badIconAdmin;
        [SerializeField]
    private GameObject midIconAdmin;
        [SerializeField]
    private GameObject goodIconAdmin;

    [SerializeField]
    private bool selected = false;


    [SerializeField]
    private GameObject selectedEffect;

    public void Initialize(CardData bc)
    {
        ApplyCaseData(bc, isAdmin: false);

        
    }

    public void HilightCard()
    {
        selectedEffect.SetActive(true);
    }

    public void DelightCard()
    {
        selectedEffect.SetActive(false);
    }

    public void InitializeAdmin(CardData bc)
    {
        ApplyCaseData(bc, isAdmin: true);
    }

    private void ApplyCaseData(CardData bc, bool isAdmin)
    {
     greyEffect.SetActive(!bc.Unlocked);


        SetupCardBG(bc);
        SetupCardInfo(bc);
        boardCard = bc;
        id = bc.Id;

        //setup the admin stuff
        if(isAdmin)
        {
            SetupValidationAdmin(bc);
        }
        else
        {
            SetupValidationPlayer(bc);
        }
    }

    private void SetupCardInfo(CardData bc)
    {
        //setup values of the card depending on the card

        responseInput.text = bc.Response;
        title.text = bc.Title;
        consigne.text = bc.Instruction;
        question.text = bc.Question;    
    }

    private void DisableIconResult()
    {
        noneIconPlayer.SetActive(false);
        waitIconPlayer.SetActive(false);
        badIconPlayer.SetActive(false);
        midIconPlayer.SetActive(false);
        goodIconPlayer.SetActive(false);

        noneIconAdmin.SetActive(false);
        waitIconAdmin.SetActive(false);
        badIconAdmin.SetActive(false);
        midIconAdmin.SetActive(false);
        goodIconAdmin.SetActive(false);

    }

    private void SetupValidationPlayer(CardData bc)
    {
        StatusAdmin.SetActive(false);
        StatusPlayer.SetActive(true);

        DisableIconResult();
        switch (bc.AutoEvaluationResult)
        {
            case EvaluationResult.NONE:
                noneIconPlayer.SetActive(true);
                statusPlayer.text = "Non validé";
                break;
            case EvaluationResult.WAITING:
                waitIconPlayer.SetActive(true);
                statusPlayer.text = "En attente";
                break;
            case EvaluationResult.BAD:
                badIconPlayer.SetActive(true);
                statusPlayer.text = "Mauvais";
                break;
            case EvaluationResult.MID:
                midIconPlayer.SetActive(true);
                statusPlayer.text = "Moyen";
                break;
            case EvaluationResult.GOOD:
                statusPlayer.text = "Bon";
                goodIconPlayer.SetActive(true);
                break;
        }
    }

    [SerializeField]
    private GameObject verifButton;

    private void SetupValidationAdmin(CardData bc)
    {
        if(!bc.NeedProEvaluation)
        {
            StatusAdmin.SetActive(false);
            StatusPlayer.SetActive(false);
            verifButton.SetActive(false);
        }
        else
        {

            StatusAdmin.SetActive(true);
            StatusPlayer.SetActive(false);
            verifButton.SetActive(true);
            DisableIconResult();
            switch (bc.ProEvaluationResult)
            {
                case EvaluationResult.NONE:
                    noneIconAdmin.SetActive(true);
                    statusAdmin.text = "Non validé";
                    break;
                case EvaluationResult.WAITING:
                    waitIconAdmin.SetActive(true);
                    statusAdmin.text = "En attente";
                    break;
                case EvaluationResult.BAD:
                    badIconAdmin.SetActive(true);
                    statusAdmin.text = "Mauvais";
                    break;
                case EvaluationResult.MID:
                    midIconAdmin.SetActive(true);
                    statusAdmin.text = "Moyen";
                    break;
                case EvaluationResult.GOOD:
                    goodIconAdmin.SetActive(true);
                    statusAdmin.text = "Bon";
                    break;
            }
        }


    }

    private void SetupCardBG(CardData cd)
    {
        DesactivateAllBGS();

        switch (cd.TypeCard)
        {
            case TypeCard.QUESTION:
               
                switch (cd.IdArea)
                {
                    case 1: bgType1.SetActive(true); break;
                    case 2: bgType2.SetActive(true); break;
                    case 3: bgType3.SetActive(true); break;
                    case 4: bgType4.SetActive(true); break;
                    case 5: bgType5.SetActive(true); break;
                    default:
                        Debug.LogWarning($"Unknown idArea: {cd.IdArea}");
                        break;
                }
                break;

            case TypeCard.BONUS:
                bgBonus.SetActive(true);
                break;
            case TypeCard.DEFI:
                bgDefi.SetActive(true);
                break;
            case TypeCard.KPI:
                bgKpi.SetActive(true);
                break;

            case TypeCard.PROFILMANAGEMENT:
                bgProfileManagement.SetActive(true);
                break;

        }
    }


    private void DesactivateAllBGS()
    {



        // Deactivate all backgrounds
        bgType1.SetActive(false);
        bgType2.SetActive(false);
        bgType3.SetActive(false);
        bgType4.SetActive(false);

        bgKpi.SetActive(false);
        bgBonus.SetActive(false) ;
        bgProfileManagement.SetActive(false);
        bgDefi.SetActive(false);    


    }





    public void ValidationAdmin()
    {
        LobbySceneManager.Instance.ChangeCardAdminState(BoardCard.Id, EvaluationResult.GOOD);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(boardCard == null) return;

        bool canrespond = LobbySceneManager.Instance.CanRespondCard && LobbySceneManager.Instance.CurrentGameState.CurrentPlayerId == Authentificator.Instance.Id;

        ZoomedCard.Instance.Initialize(boardCard, canrespond);
    }
}
