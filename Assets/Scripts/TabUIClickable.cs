using UnityEngine;
using UnityEngine.EventSystems;

public class TabUIClickable : UIClickable, IPointerDownHandler
{

    [SerializeField]
    private GameObject selected;
    
    [SerializeField]
    private GameObject unSelected;

    [SerializeField]
    private MenuType tabType;

    public MenuType TabType { get => tabType; set => tabType = value; }

    public void SetSelected(bool isSelected)
    {
        selected.SetActive(isSelected);
        unSelected.SetActive(!isSelected);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        InGameMenuManager.Instance.OpenPageMenu(tabType);
    }


}
