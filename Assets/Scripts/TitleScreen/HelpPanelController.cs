using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plasmalot: Owns the Title Screen's Help panel. Original scale is cached on scene load; Open() scales
/// the panel up from zero with a small dip-then-bounce, Close() scales it back to zero and deactivates it
/// until Open() is called again. Open() also disables the primary menu buttons underneath and enables this
/// panel's own buttons, so both can't be interacted with at once; Close() disables this panel's buttons
/// immediately (so they can't be clicked again mid-animation) and re-enables the primary menu buttons only
/// once the close animation has fully finished.
/// </summary>
public class HelpPanelController : MonoBehaviour
{
    [SerializeField, Tooltip("Seconds for the initial scale-up on Open().")]
    private float m_OpenDuration = 0.4f;

    [SerializeField, Tooltip("Seconds for each half of the small de-scale/scale-up bounce on Open().")]
    private float m_BounceDuration = 0.15f;

    [SerializeField, Tooltip("Scale multiplier (of the cached original scale) the panel briefly dips to mid-bounce.")]
    private float m_BounceDipScaleMultiplier = 0.9f;

    [SerializeField, Tooltip("Seconds for the scale-down on Close().")]
    private float m_CloseDuration = 0.2f;

    [SerializeField, Tooltip("The 3 primary menu buttons (Play Game, Credits, Quit Game) underneath this panel. Disabled while this panel is open.")]
    private Button[] m_PrimaryMenuButtons;

    [SerializeField, Tooltip("This panel's own buttons. Enabled only while this panel is open.")]
    private Button[] m_PanelButtons;

    private Vector3 m_OriginalScale;
    private Sequence m_Sequence;

    private void Awake()
    {
        m_OriginalScale = transform.localScale;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;

        _SetButtonsInteractable(m_PrimaryMenuButtons, false);
        _SetButtonsInteractable(m_PanelButtons, true);

        m_Sequence?.Kill();
        m_Sequence = DOTween.Sequence();
        m_Sequence.Append(transform.DOScale(m_OriginalScale, m_OpenDuration).SetEase(Ease.OutSine));
        m_Sequence.Append(transform.DOScale(m_OriginalScale * m_BounceDipScaleMultiplier, m_BounceDuration).SetEase(Ease.InOutSine));
        m_Sequence.Append(transform.DOScale(m_OriginalScale, m_BounceDuration).SetEase(Ease.InOutSine));
    }

    public void Close()
    {
        _SetButtonsInteractable(m_PanelButtons, false);

        m_Sequence?.Kill();
        m_Sequence = DOTween.Sequence();
        m_Sequence.Append(transform.DOScale(m_OriginalScale * (1 + (1 - m_BounceDipScaleMultiplier)), m_BounceDuration).SetEase(Ease.InOutSine));
        m_Sequence.Append(transform.DOScale(Vector3.zero, m_CloseDuration).SetEase(Ease.InSine));
        m_Sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            _SetButtonsInteractable(m_PrimaryMenuButtons, true);
        });
    }

    private void OnDestroy()
    {
        m_Sequence?.Kill();
    }

    private static void _SetButtonsInteractable(Button[] buttons, bool bInteractable)
    {
        foreach (Button button in buttons)
        {
            button.interactable = bInteractable;
        }
    }
}
