using DG.Tweening;
using UnityEngine;

/// <summary>
/// Plasmalot: Continuously scales the Game Title up and down between two multipliers of its starting scale,
/// giving it a slow "breathing" pulse on the Title Screen.
/// </summary>
public class GameTitlePulse : MonoBehaviour
{
    [SerializeField, Tooltip("Smallest scale multiplier (of the starting scale) reached mid-pulse.")]
    private float m_MinScaleMultiplier = 0.8f;

    [SerializeField, Tooltip("Largest scale multiplier (of the starting scale) reached mid-pulse.")]
    private float m_MaxScaleMultiplier = 1.2f;

    [SerializeField, Tooltip("Seconds for one min-to-max (or max-to-min) swing of the pulse.")]
    private float m_PulseDuration = 2f;

    private Tween m_PulseTween;

    private void Start()
    {
        Vector3 baseScale = transform.localScale;
        transform.localScale = baseScale * m_MinScaleMultiplier;

        m_PulseTween = transform.DOScale(baseScale * m_MaxScaleMultiplier, m_PulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        m_PulseTween?.Kill();
    }
}
