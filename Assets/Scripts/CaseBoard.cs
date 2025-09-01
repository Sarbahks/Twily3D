using System;
using TMPro;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public class CaseBoard : MonoBehaviour
{
    [SerializeField]
    private CardData cardData;
    [SerializeField]
    private TextMeshPro textCase;

    [SerializeField]
    private TypeCard typeCase;
    public CardData CaseData { get => cardData; set => cardData = value; }
    public TextMeshPro TextCase { get => textCase; set => textCase = value; }
    public TypeCard TypeCase { get => typeCase; set => typeCase = value; }
    public bool Visited { get => visited; set => visited = value; }

    private int areaCase;

    [SerializeField]
    private GameObject unlocked;
       [SerializeField]
    private GameObject type1;       
    [SerializeField]
    private GameObject type2;       
    [SerializeField]
    private GameObject type3;       
    [SerializeField]
    private GameObject type4;


    [SerializeField]
    private GameObject caseText;

    [SerializeField]
    private GameObject bonusIcon;
    [SerializeField]
    private GameObject profileIcon;
     [SerializeField]
    private GameObject defiIcon;
     [SerializeField]
    private GameObject kpiIcon;
     [SerializeField]
    private GameObject profilemanagementIcon;

    [SerializeField]
    private bool visited;




    private void SetUpIconCase(TypeCard type)
    {
        DisableAllCaseIcons();

        switch (type)
        {
            case TypeCard.BONUS:
                bonusIcon.SetActive(true);
                break;
            case TypeCard.PROFILE:
                profileIcon.SetActive(true);
                break;
            case TypeCard.DEFI:
                defiIcon.SetActive(true);
                break;
            case TypeCard.KPI:
                kpiIcon.SetActive(true);
                break;
            case TypeCard.PROFILMANAGEMENT:
                profilemanagementIcon.SetActive(true);
                break;
            default:
                caseText.SetActive(true); // Default fallback to show text (typically for QUESTION)
                break;
        }
    }

    private void DisableAllCaseIcons()
    {
        caseText.SetActive(false);
        bonusIcon.SetActive(false);
        profileIcon.SetActive(false);
        defiIcon.SetActive(false);
        kpiIcon.SetActive(false);
        profilemanagementIcon.SetActive(false);
    }

    public void DesactivateCase()
    {
        unlocked.SetActive(true);
        type1.SetActive(false);
        type2.SetActive(false);
        type3.SetActive(false);         
        type4.SetActive(false);

    }

    public void ActivateCase()
    {
        unlocked.SetActive(false);
        type1.SetActive(false);
        type2.SetActive(false);
        type3.SetActive(false);
        type4.SetActive(false);


        switch (areaCase)
        {
            case 1:
                type1.SetActive(true);
                break;
                case 2:
                type2.SetActive(true);
                break; 
            case 3:
                type3.SetActive(true);
                break; 
            case 4:  
                type4.SetActive(true);
                break;

        }
    }

    public void InitializeCase(TypeCard type, CardData value, int idArea)
    {

            areaCase = idArea;
            SetUpIconCase(type);
            if(value.Unlocked)
        {
            ActivateCase();
        }
            else
        {
            DesactivateCase();
        }
        

       // }
    }

    public void Unlock()
    {
        ActivateCase();
        visited = true;
    }
}



