using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Plasmalot: Hover/press scale feedback for a Title Screen menu option button. Scales up on hover, dips
/// slightly on mouse down, and pops back up on release before automatically settling back to its resting
/// scale after a short hold. Ignores all pointer events while the sibling Button is non-interactable, since
/// Unity's EventSystem still dispatches IPointer*Handler callbacks to a GameObject regardless of its
/// Button.interactable state - only Button's own click/visual-transition logic respects that flag.
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

    [Header("Audio")]
    [SerializeField, Tooltip("SFX played when the menu option is hovered.")]
    private AudioClip m_MenuOptionHoverSFX;

    [SerializeField, Tooltip("SFX played when the menu option is pressed.")]
    private AudioClip m_MenuOptionPressSFX;

    private Button m_Button;
    private Vector3 m_RestingScale;
    private Tween m_ScaleTween;
    private bool m_bIsPressed;

    private void Awake()
    {
        m_Button = GetComponent<Button>();
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
        if (m_bIsPressed || (m_Button != null && !m_Button.interactable)) return;

        AudioManager.Instance.PlaySFXOneShot(m_MenuOptionHoverSFX);

        m_ScaleTween?.Kill();
        m_ScaleTween = transform.DOScale(m_RestingScale * m_HoverScaleMultiplier, m_ScaleTransitionDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_bIsPressed) return;

        m_ScaleTween?.Kill();
        m_ScaleTween = transform.DOScale(m_RestingScale, m_ScaleTransitionDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_Button != null && !m_Button.interactable) return;

        m_bIsPressed = true;
        m_ScaleTween?.Kill();
        m_ScaleTween = transform.DOScale(m_RestingScale * m_PressScaleMultiplier, m_ScaleTransitionDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!m_bIsPressed) return;

        m_bIsPressed = false;
        m_ScaleTween?.Kill();

        AudioManager.Instance.PlaySFXOneShot(m_MenuOptionPressSFX);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(m_RestingScale * m_HoverScaleMultiplier, m_ScaleTransitionDuration));
        sequence.AppendInterval(m_PostReleaseHoldDuration);
        sequence.Append(transform.DOScale(m_RestingScale, m_ScaleTransitionDuration));
        m_ScaleTween = sequence;
    }
}
