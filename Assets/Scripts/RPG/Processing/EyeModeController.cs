using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Plasmalot: Owns the swap between Text-Based RPG mode and Spot-the-Difference mode for the current Level.
/// Opening eyes fades in over the dialogue UI, then slides the eyelid panels off-screen (up/down) to reveal
/// a Variation image; closing eyes (the only input read while eyes are open) reverses the sequence.
/// </summary>
public class EyeModeController : MonoBehaviour
{
    [SerializeField, Tooltip("Root GameObject of the Spot-the-Difference canvas. Starts inactive.")]
    private GameObject m_SpotDifferenceCanvasRoot;

    [SerializeField, Tooltip("CanvasGroup faded in/out to cover/reveal the dialogue UI underneath.")]
    private CanvasGroup m_SpotDifferenceCanvasGroup;

    [SerializeField, Tooltip("Full-screen Image the chosen Variation sprite is assigned to.")]
    private Image m_VariationImageDisplay;

    [SerializeField, Tooltip("Upper eyelid panel; tweens up and off-screen to open.")]
    private RectTransform m_TopLid;

    [SerializeField, Tooltip("Lower eyelid panel; tweens down and off-screen to open.")]
    private RectTransform m_BottomLid;

    [SerializeField, Tooltip("Root GameObject of the dialogue UI, hidden while eyes are open.")]
    private GameObject m_DialogueUIRoot;

    [SerializeField, Tooltip("Every PromptResponses source for the current Level/Variation. Index 0 is treated as the default Variation.")]
    private List<PromptResponses> m_VariationSources;

    [SerializeField, Tooltip("Duration (seconds) of the Spot-the-Difference canvas fade in/out.")]
    private float m_CanvasFadeDuration = 0.5f;

    [SerializeField, Tooltip("Duration (seconds) of the eyelid open/close tween.")]
    private float m_EyelidTweenDuration = 1f;

    [SerializeField, Tooltip("SFX played as the eyelids wipe open.")]
    private AudioClip m_EyeOpenSFXClip;

    [SerializeField, Tooltip("SFX played as the eyelids wipe closed.")]
    private AudioClip m_EyeCloseSFXClip;

    private Vector2 m_TopLidClosedPos;
    private Vector2 m_TopLidOpenPos;
    private Vector2 m_BottomLidClosedPos;
    private Vector2 m_BottomLidOpenPos;

    private int m_LastVariationIndex = -1;
    private bool m_bIsEyesOpen;
    private bool m_bIsTransitioning;

    public event Action OnEyesClosed;

    public PromptResponses CurrentVariationSource => m_VariationSources[m_LastVariationIndex];

    private void Awake()
    {
        m_TopLidClosedPos = m_TopLid.anchoredPosition;
        m_TopLidOpenPos = m_TopLidClosedPos + Vector2.up * m_TopLid.rect.height * 15;

        m_BottomLidClosedPos = m_BottomLid.anchoredPosition;
        m_BottomLidOpenPos = m_BottomLidClosedPos + Vector2.down * m_BottomLid.rect.height * 15;
    }

    private void Update()
    {
        if (!m_bIsEyesOpen || m_bIsTransitioning || Keyboard.current == null) return;

        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            CloseEyes();
        }
    }

    public void OpenEyes()
    {
        Sprite chosenSprite = _ChooseVariationSprite();
        m_VariationImageDisplay.sprite = chosenSprite;
        m_VariationImageDisplay.enabled = false;

        m_bIsTransitioning = true;
        m_TopLid.anchoredPosition = m_TopLidClosedPos;
        m_BottomLid.anchoredPosition = m_BottomLidClosedPos;

        m_SpotDifferenceCanvasGroup.alpha = 0f;
        m_SpotDifferenceCanvasRoot.SetActive(true);

        m_SpotDifferenceCanvasGroup.DOFade(1f, m_CanvasFadeDuration).OnComplete(_BeginEyelidOpenTween);
    }

    public void CloseEyes()
    {
        m_bIsTransitioning = true;
        m_bIsEyesOpen = false;

        if (m_EyeCloseSFXClip != null)
        {
            AudioManager.Instance.PlaySFXOneShot(m_EyeCloseSFXClip);
        }

        Sequence closeSequence = DOTween.Sequence();
        closeSequence.Join(m_TopLid.DOAnchorPos(m_TopLidClosedPos, m_EyelidTweenDuration));
        closeSequence.Join(m_BottomLid.DOAnchorPos(m_BottomLidClosedPos, m_EyelidTweenDuration)).WaitForCompletion();
        closeSequence.OnComplete(() => _OnEyesFullyClosed());
    }

    private void _BeginEyelidOpenTween()
    {
        m_DialogueUIRoot.SetActive(false);
        m_VariationImageDisplay.enabled = true;

        if (m_EyeOpenSFXClip != null)
        {
            AudioManager.Instance.PlaySFXOneShot(m_EyeOpenSFXClip);
        }

        Sequence openSequence = DOTween.Sequence();
        openSequence.Join(m_TopLid.DOAnchorPos(m_TopLidOpenPos, m_EyelidTweenDuration));
        openSequence.Join(m_BottomLid.DOAnchorPos(m_BottomLidOpenPos, m_EyelidTweenDuration));
        openSequence.OnComplete(_OnEyesFullyOpen);
    }

    private void _OnEyesFullyOpen()
    {
        m_bIsTransitioning = false;
        m_bIsEyesOpen = true;
    }

    private void _OnEyesFullyClosed()
    {
        m_SpotDifferenceCanvasRoot.SetActive(false);
        m_DialogueUIRoot.SetActive(true);

        m_bIsTransitioning = false;

        OnEyesClosed?.Invoke();
    }

    private Sprite _ChooseVariationSprite()
    {
        int currentLayer = GameProgressManager.Instance.GetCurrentLayer(LevelContext.Instance.CurrentLevelID);

        List<int> eligibleIndices = new List<int>();
        for (int i = 0; i < m_VariationSources.Count; i++)
        {
            if (m_VariationSources[i].VariationImage != null && m_VariationSources[i].RequiredLayer <= currentLayer)
            {
                eligibleIndices.Add(i);
            }
        }

        int chosenIndex;
        if (m_LastVariationIndex == -1)
        {
            chosenIndex = eligibleIndices.Contains(0) ? 0 : eligibleIndices[0];
        }
        else
        {
            List<int> repeatCandidates = eligibleIndices.FindAll(i => i != m_LastVariationIndex);
            chosenIndex = repeatCandidates.Count > 0
                ? repeatCandidates[UnityEngine.Random.Range(0, repeatCandidates.Count)]
                : eligibleIndices[UnityEngine.Random.Range(0, eligibleIndices.Count)];
        }

        m_LastVariationIndex = chosenIndex;
        return m_VariationSources[chosenIndex].VariationImage;
    }
}
