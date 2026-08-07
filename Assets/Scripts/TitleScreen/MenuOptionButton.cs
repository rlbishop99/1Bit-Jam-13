using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Plasmalot: Hover/press scale feedback for a Title Screen menu option button. Scales up on hover, dips
/// slightly on mouse down, and pops back up on release before automatically settling back to its resting
/// scale after a short hold.
/// </summary>
public class MenuOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Tooltip("Scale multiplier (of the resting scale) reached while hovered.")]
    private float m_HoverScaleMultiplier = 1.2f;

    [SerializeField, Tooltip("Scale multiplier (of the resting scale) reached while the mouse button is held down.")]
    private float m_PressScaleMultiplier = 1.1f;

    [SerializeField, Tooltip("Seconds for each scale transition (hover in/out, press, release).")]
    private float m_ScaleTransitionDuration = 0.1f;

    [SerializeField, Tooltip("Seconds the button holds at the hover scale after release before automatically returning to resting scale.")]
    private float m_PostReleaseHoldDuration = 1f;

    private Vector3 m_RestingScale;
    private Tween m_ScaleTween;
    private bool m_bIsPressed;

    private void Awake()
    {
        m_RestingScale = transform.localScale;
    }

    private void OnDisable()
    {
        m_ScaleTween?.Kill();
        m_bIsPressed = false;
        transform.localScale = m_RestingScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_bIsPressed) return;

        m_ScaleTween?.Kill();
        m_ScaleTween = transform.DOScale(m_RestingScale * m_HoverScaleMultiplier, m_ScaleTransitionDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_bIsPressed = false;
        m_ScaleTween?.Kill();
        m_ScaleTween = transform.DOScale(m_RestingScale, m_ScaleTransitionDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_bIsPressed = true;
        m_ScaleTween?.Kill();
        m_ScaleTween = transform.DOScale(m_RestingScale * m_PressScaleMultiplier, m_ScaleTransitionDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_bIsPressed = false;
        m_ScaleTween?.Kill();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(m_RestingScale * m_HoverScaleMultiplier, m_ScaleTransitionDuration));
        sequence.AppendInterval(m_PostReleaseHoldDuration);
        sequence.Append(transform.DOScale(m_RestingScale, m_ScaleTransitionDuration));
        m_ScaleTween = sequence;
    }
}
