using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomingPicture : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameObject ZoomedGO;

    public void OnPointerClick(PointerEventData eventData)
    {
        ZoomedGO.SetActive(true);
    }

    public void CloseZoom()
    {
        ZoomedGO.SetActive(false);
    }
}
