using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardProfile : MonoBehaviour
{
    [SerializeField]
    private CardData profileData;

    [Header("Card area 2")]
    [SerializeField]
    private GameObject cardType2;

    [SerializeField]
    private TextMeshProUGUI nomPrenomAge2;

    [SerializeField]
    private TextMeshProUGUI experience2;
    [SerializeField]
    private TextMeshProUGUI chezLens2;
    [SerializeField]
    private TextMeshProUGUI integration2;

    [SerializeField]
    private TextMeshProUGUI diplome2;

    [SerializeField]
    private TextMeshProUGUI qualif2;

    [SerializeField]
    private TextMeshProUGUI qualite2;

    [SerializeField]
    private TextMeshProUGUI modeTravail2;


    [SerializeField]
    private TextMeshProUGUI pointsForts2;
        
    [SerializeField]
    private TextMeshProUGUI pointsFaible2;

    [SerializeField]
    private Image photo2;









    [Header("Card area 3")]
    [SerializeField]
    private GameObject cardType3;

    [SerializeField]
    private TextMeshProUGUI nomPrenomAge3;

    [SerializeField]
    private TextMeshProUGUI experience3;
    [SerializeField]
    private TextMeshProUGUI chezLens3;
    [SerializeField]
    private TextMeshProUGUI ancienService3;

    [SerializeField]
    private TextMeshProUGUI diplome3;

    [SerializeField]
    private TextMeshProUGUI qualif3;

    [SerializeField]
    private TextMeshProUGUI qualite3;



    [SerializeField]
    private TextMeshProUGUI pointsForts3;

    [SerializeField]
    private TextMeshProUGUI pointsFaible3;


    [SerializeField]
    private Image photo3;



    public void SetupCard(CardData card)
    {
        profileData = card;
        //setup card, manage patter depending the area id
        if (card.IdArea == 2)
        {
            nomPrenomAge2.text = card.Title;
            experience2.text = card.Experience;
            chezLens2.text = card.Seniority;
            integration2.text = card.OldService;
            diplome2.text = card.Degree;


            qualif2.text = card.Role;
            qualite2.text = card.Description;
            modeTravail2.text = card.Instruction;

            pointsForts2.text = card.StrongPoints;
            pointsFaible2.text = card.WeakPoints;

            var sprite = SpecialInGameManager.Instance.GetImageFromId(card.AttachedDocupentId);
            if (sprite != null)
            {
                photo2.sprite = sprite;
            }

            cardType2.SetActive(true);
            cardType3.SetActive(false);

        }
        else
        {

                nomPrenomAge3.text = card.Title;
                experience3.text = card.Experience;
                chezLens3.text = card.Seniority;
                ancienService3.text = card.OldService;
                diplome3.text = card.Degree;


                qualif3.text = card.Role;
             ;
                qualite3.text = card.Description;

                pointsForts3.text = card.StrongPoints;
                pointsFaible3.text = card.WeakPoints;

                var sprite = SpecialInGameManager.Instance.GetImageFromId(card.AttachedDocupentId);
                if (sprite != null)
                {
                    photo3.sprite = sprite;
                }
            


            cardType2.SetActive(false);
            cardType3.SetActive(true);
        }
    }
}
