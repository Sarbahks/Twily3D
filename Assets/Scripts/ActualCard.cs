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
    private GameObject bgProfile;
    [SerializeField]
    private GameObject bgKpi;

    [SerializeField]
    private GameObject bgProfileManagement;




    [SerializeField]
    private TextMeshProUGUI respondButtonText;



    [SerializeField]
    private TwilyButton respondButton;

    public CardData BoardCase { get => boardCase; set => boardCase = value; }
    public int Id { get => id; set => id = value; }
    public TextMeshProUGUI Title { get => title; set => title = value; }





    //TODO change it so it matche the new card
    public void Initialize(CardData card, bool isplayerTurn)
    {
        boardCase = card;
   
         

        id = card.Id;
        SetupCardBG(card);


        if(isplayerTurn)
        {
            respondButtonText.text = "Répondre";
        }
        else
        {
            respondButtonText.text = "Voir la carte";
        }
        respondButton.gameObject.SetActive(true);

        InGameMenuManager.Instance.HighlightCard(card);
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
                bgBonus.SetActive(true);
                break;
            case TypeCard.KPI:
                bgKpi.SetActive(true);
                break;
            case TypeCard.PROFILMANAGEMENT:
                bgProfileManagement.SetActive(true);
                break;


        }

        title.text = cd.Title;
        question.text = cd.Question;
        consigne.text = cd.Instruction;
  
    }


    private void DesactivateAllBGS()
    {



        // Deactivate all backgrounds
        bgType1.SetActive(false);
        bgType2.SetActive(false);
        bgType3.SetActive(false);
        bgType4.SetActive(false);
        bgBonus.SetActive(false);
        bgDefi.SetActive(false);
        bgKpi.SetActive(false);
        bgProfileManagement.SetActive(false);

    }

    public void GoToCanvas()
    {
        InGameMenuManager.Instance.ShowMenu();
        InGameMenuManager.Instance.OpenPageMenu(MenuType.MENUCARDSPLAYER);
    }

}
