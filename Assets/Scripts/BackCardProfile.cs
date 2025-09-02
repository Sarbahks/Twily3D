using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackCardProfile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private GameObject totalbackCardObject;

    [SerializeField]
    private GameObject backGroundArea2;
    
    [SerializeField]
    private GameObject backGroundArea3;

    [SerializeField]
    private Image picture;

    [SerializeField]
    private TextMeshProUGUI infos;

    public void SetupBackCard(CardData card)
    {
        if(card.IdArea == 2)
        {
            backGroundArea2.SetActive(true);
            backGroundArea3.SetActive(false);
        }
        else
        {
            backGroundArea2.SetActive(false);
            backGroundArea3.SetActive(true);
        }
     
        var sprite = SpecialInGameManager.Instance.GetImageFromId(card.AttachedDocupentId);
        if (sprite != null)
        {
        picture.sprite = sprite;
        }
        infos.text = card.Title;
    }

    // Called when the mouse/finger is pressed down
    public void OnPointerDown(PointerEventData eventData)
    {
        totalbackCardObject.SetActive(false);
     
    }

    // Called when the mouse/finger is released
    public void OnPointerUp(PointerEventData eventData)
    {
        totalbackCardObject.SetActive(true);
        
    }
}
