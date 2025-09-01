using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMovementPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Camera cameraTwily;
    [SerializeField] private float moveSpeed = 0.05f;   // How much drag translates into movement
    [SerializeField] private float smoothTime = 0.2f;   // Lower = snappier, Higher = smoother

    private Vector2 lastPointerPosition;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero; // For SmoothDamp
    private bool isDragging = false;

    private void Start()
    {
        if (cameraTwily == null)
            cameraTwily = Camera.main;

        targetPosition = cameraTwily.transform.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        lastPointerPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 delta = eventData.position - lastPointerPosition;
        lastPointerPosition = eventData.position;

        // Translate drag into movement on XZ plane
        Vector3 moveDirection = new Vector3(delta.x, 0, delta.y) * moveSpeed;
        targetPosition += moveDirection;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void Update()
    {
        // Smoothly move the camera towards targetPosition
        cameraTwily.transform.position = Vector3.SmoothDamp(
            cameraTwily.transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
