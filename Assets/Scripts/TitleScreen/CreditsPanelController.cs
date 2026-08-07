using DG.Tweening;
using UnityEngine;

/// <summary>
/// Plasmalot: Owns the Title Screen's Credits panel. Original scale is cached on scene load; Open() scales
/// the panel up from zero with a small dip-then-bounce, Close() scales it back to zero and deactivates it
/// until Open() is called again.
/// </summary>
public class CreditsPanelController : MonoBehaviour
{
    [SerializeField, Tooltip("Seconds for the initial scale-up on Open().")]
    private float m_OpenDuration = 0.4f;

    [SerializeField, Tooltip("Seconds for each half of the small de-scale/scale-up bounce on Open().")]
    private float m_BounceDuration = 0.15f;

    [SerializeField, Tooltip("Scale multiplier (of the cached original scale) the panel briefly dips to mid-bounce.")]
    private float m_BounceDipScaleMultiplier = 0.9f;

    [SerializeField, Tooltip("Seconds for the scale-down on Close().")]
    private float m_CloseDuration = 0.2f;

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

        m_Sequence?.Kill();
        m_Sequence = DOTween.Sequence();
        m_Sequence.Append(transform.DOScale(m_OriginalScale, m_OpenDuration).SetEase(Ease.OutSine));
        m_Sequence.Append(transform.DOScale(m_OriginalScale * m_BounceDipScaleMultiplier, m_BounceDuration).SetEase(Ease.InOutSine));
        m_Sequence.Append(transform.DOScale(m_OriginalScale, m_BounceDuration).SetEase(Ease.InOutSine));
    }

    public void Close()
    {
        m_Sequence?.Kill();
        m_Sequence = DOTween.Sequence();
        m_Sequence.Append(transform.DOScale(m_OriginalScale * (1 + (1 - m_BounceDipScaleMultiplier)), m_BounceDuration).SetEase(Ease.InOutSine));
        m_Sequence.Append(transform.DOScale(Vector3.zero, m_CloseDuration).SetEase(Ease.InSine));
        m_Sequence.OnComplete(() => gameObject.SetActive(false));
    }

    private void OnDestroy()
    {
        m_Sequence?.Kill();
    }
}
