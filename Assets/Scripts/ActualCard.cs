using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActualCard : MonoBehaviour
{
    [SerializeField]
    private CardData boardCase;

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
    private GameObject bgType1;

    [SerializeField]
    private GameObject bgType2;

    [SerializeField]
    private GameObject bgType3;

    [SerializeField]
    private GameObject bgType4;

    [SerializeField]
    private GameObject bgBonus;
    [SerializeField]
    private GameObject bgDefi;
    [SerializeField]
    private GameObject bgProfile;
    [SerializeField]
    private GameObject bgKpi;

    [SerializeField]
    private GameObject bgProfileManagement;


    [SerializeField]
    private TextMeshProUGUI specialTitle;

    [SerializeField]
    private TextMeshProUGUI specialQuestion;
    [SerializeField]
    private TextMeshProUGUI specialConsigne;



    [SerializeField]
    private TextMeshProUGUI profileTitle;
    [SerializeField]
    private TextMeshProUGUI profileDescription;
    [SerializeField]
    private TextMeshProUGUI profileDiploma;
    [SerializeField]
    private TextMeshProUGUI profileRole;
    [SerializeField]
    private TextMeshProUGUI profileExp;
    [SerializeField]
    private TextMeshProUGUI profileStrongPoint;
    [SerializeField]
    private TextMeshProUGUI profileWeakPoint;

    [SerializeField]
    private GameObject questionObject;

    [SerializeField]
    private GameObject specialObject;

    [SerializeField]
    private GameObject profileObject;



    [SerializeField]
    private TwilyButton validationButton;

    public CardData BoardCase { get => boardCase; set => boardCase = value; }
    public int Id { get => id; set => id = value; }
    public TextMeshProUGUI Title { get => title; set => title = value; }
    public TMP_InputField ResponseInput { get => responseInput; set => responseInput = value; }



    public void SendResponse()
    {
        boardCase.Response = responseInput.text;
        LobbySceneManager.Instance.OnValidateAnswer(boardCase);
        validationButton.gameObject.SetActive(false);
    }

    //TODO change it so it matche the new card
    public void Initialize(CardData card, bool isplayerTurn)
    {
        boardCase = card;
        if(card.AttachedDocupentId > 0)
        {
            linkedButton.gameObject.SetActive(true);
        }
        else
        {
            linkedButton.gameObject.SetActive(true);
        }
        id = card.Id;
        SetupCardBG(card);

        validationButton.gameObject.SetActive(isplayerTurn);
    }

    




    private void SetupCardBG(CardData cd)
    {
        DesactivateAllBGS();

        switch (cd.TypeCard)
        {
            case TypeCard.QUESTION:
                questionObject.SetActive(true);

                switch (cd.IdArea)
                {
                    case 1: bgType1.SetActive(true); break;
                    case 2: bgType2.SetActive(true); break;
                    case 3: bgType3.SetActive(true); break;
                    case 4: bgType4.SetActive(true); break;
                    default:
                        Debug.LogWarning($"Unknown idArea: {cd.IdArea}");
                        break;
                }
                title.text = cd.Title;
                question.text = cd.Question;
                consigne.text = cd.Instruction;

                break;

            case TypeCard.BONUS:
            case TypeCard.DEFI:
            case TypeCard.KPI:
            case TypeCard.PROFILMANAGEMENT:
                specialObject.SetActive(true);

                specialTitle.text = cd.Title;
                specialQuestion.text = cd.Question;
                specialConsigne.text = cd.Instruction;
                break;

            case TypeCard.PROFILE:
                profileObject.SetActive(true);

                profileTitle.text = cd.Title;
                profileDescription.text = cd.Description;
                profileDiploma.text = cd.Degree;
                profileRole.text = cd.Role;
                profileExp.text = cd.Experience;
                profileStrongPoint.text = cd.StrongPoints;
                profileWeakPoint.text = cd.WeakPoints;
                break;
        }
    }


    private void DesactivateAllBGS()
    {
        questionObject.SetActive(false);
        specialObject.SetActive(false);
        profileObject.SetActive(false);


        // Deactivate all backgrounds
        bgType1.SetActive(false);
        bgType2.SetActive(false);
        bgType3.SetActive(false);
        bgType4.SetActive(false);


    }
    [SerializeField]
    private GameObject linkedButton;

    public void OpenLinkedDoc()
    {
        SpecialInGameManager.Instance.OpenCardLinkedDoc(boardCase.AttachedDocupentId);  
            }


}
