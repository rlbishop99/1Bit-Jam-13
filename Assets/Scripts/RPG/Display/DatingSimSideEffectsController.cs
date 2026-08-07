using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Plasmalot: Owns the Dating Sim's decorative side-of-screen effects - repeating hearts that rotate back and
/// forth on a correct answer, expanding/collapsing anger symbols on an incorrect answer. Every decoration
/// object is pre-placed in the scene and driven by a looping DOTween while its effect is playing.
/// </summary>
public class DatingSimSideEffectsController : MonoBehaviour
{
    [SerializeField, Tooltip("Pre-placed heart decoration objects along both screen edges.")]
    private List<RectTransform> m_HeartObjects;

    [SerializeField, Tooltip("Pre-placed anger symbol decoration objects along both screen edges.")]
    private List<RectTransform> m_AngerObjects;

    [SerializeField, Tooltip("Degrees each heart rotates back and forth from its resting rotation.")]
    private float m_HeartRotationAngle = 15.0f;

    [SerializeField, Tooltip("Seconds for one heart rotation swing.")]
    private float m_HeartRotationDuration = 1.0f;

    [SerializeField, Tooltip("Scale multiplier each anger symbol expands to from its resting scale.")]
    private float m_AngerScaleMultiplier = 1.3f;

    [SerializeField, Tooltip("Seconds for one anger expand/collapse cycle.")]
    private float m_AngerScaleDuration = 0.4f;

    private readonly List<Tween> m_ActiveTweens = new List<Tween>();
    private readonly Dictionary<RectTransform, Quaternion> m_RestingRotations = new Dictionary<RectTransform, Quaternion>();
    private readonly Dictionary<RectTransform, Vector3> m_RestingScales = new Dictionary<RectTransform, Vector3>();
    private GameEnums.eDatingSimSideEffect m_ActiveEffect = GameEnums.eDatingSimSideEffect.None;

    private void Awake()
    {
        foreach (RectTransform heart in m_HeartObjects)
        {
            m_RestingRotations[heart] = heart.localRotation;
        }
        foreach (RectTransform anger in m_AngerObjects)
        {
            m_RestingScales[anger] = anger.localScale;
        }
    }

    public void PlayHearts()
    {
        if (m_ActiveEffect == GameEnums.eDatingSimSideEffect.Hearts) return;

        StopAll();
        m_ActiveEffect = GameEnums.eDatingSimSideEffect.Hearts;
        foreach (RectTransform heart in m_HeartObjects)
        {
            heart.gameObject.SetActive(true);
            Vector3 restingEuler = m_RestingRotations[heart].eulerAngles;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(heart.DOLocalRotate(restingEuler + new Vector3(0, 0, m_HeartRotationAngle), m_HeartRotationDuration).SetEase(Ease.Linear));
            sequence.Append(heart.DOLocalRotate(restingEuler, m_HeartRotationDuration).SetEase(Ease.Linear));
            sequence.Append(heart.DOLocalRotate(restingEuler + new Vector3(0, 0, -m_HeartRotationAngle), m_HeartRotationDuration).SetEase(Ease.Linear));
            sequence.Append(heart.DOLocalRotate(restingEuler, m_HeartRotationDuration).SetEase(Ease.Linear));
            sequence.SetLoops(-1, LoopType.Restart);
            m_ActiveTweens.Add(sequence);
        }
    }

    public void PlayAnger()
    {
        if (m_ActiveEffect == GameEnums.eDatingSimSideEffect.Anger) return;

        StopAll();
        m_ActiveEffect = GameEnums.eDatingSimSideEffect.Anger;
        foreach (RectTransform anger in m_AngerObjects)
        {
            anger.gameObject.SetActive(true);
            Tween tween = anger
                .DOScale(m_RestingScales[anger] * m_AngerScaleMultiplier, m_AngerScaleDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear);
            m_ActiveTweens.Add(tween);
        }
    }

    public void StopAll()
    {
        m_ActiveEffect = GameEnums.eDatingSimSideEffect.None;

        foreach (Tween tween in m_ActiveTweens)
        {
            tween?.Kill();
        }
        m_ActiveTweens.Clear();

        foreach (RectTransform heart in m_HeartObjects)
        {
            heart.localRotation = m_RestingRotations[heart];
            heart.gameObject.SetActive(false);
        }
        foreach (RectTransform anger in m_AngerObjects)
        {
            anger.localScale = m_RestingScales[anger];
            anger.gameObject.SetActive(false);
        }
    }
}
