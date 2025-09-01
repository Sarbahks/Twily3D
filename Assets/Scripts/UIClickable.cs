using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIClickable : MonoBehaviour,IPointerClickHandler
{
    [SerializeField]
    private UnityEvent onClick;

    public UnityEvent OnClick { get => onClick; set => onClick = value; }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
}
