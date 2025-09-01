using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class TwilyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField]
    private Image buttonImage;

    [SerializeField]
    private Image buttonImageSide;

    [SerializeField]
    private float pressScale = 0.95f;
    [SerializeField]
    private float pressDuration = 0.1f;
    [SerializeField]
    private Color pressedColor = new Color(0.8f, 0.8f, 0.8f); // Slightly darker

    [SerializeField]
    private UnityEvent onClick;

    private Color originalColor;
    private Vector3 originalScale;

    public UnityEvent OnClick { get => onClick; set => onClick = value; }

    private void Awake()
    {
        if (buttonImage != null)
            originalColor = buttonImage.color;

        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ClickEffect());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        ResetVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }

    private IEnumerator ClickEffect()
    {
        if (buttonImage != null)
            buttonImage.color = pressedColor;

        transform.localScale = originalScale * pressScale;

        yield return new WaitForSeconds(pressDuration);

        ResetVisuals();
    }

    private void ResetVisuals()
    {
        transform.localScale = originalScale;

        if (buttonImage != null)
            buttonImage.color = originalColor;
    }
}
