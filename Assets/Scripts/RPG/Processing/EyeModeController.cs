using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Plasmalot: Owns the swap between Text-Based RPG mode and Spot-the-Difference mode for the current Level.
/// Opening eyes fades in over the dialogue UI, then slides the eyelid panels off-screen (up/down) to reveal
/// a Variation image; closing eyes (the only input read while eyes are open) reverses the sequence.
/// Each open-eyes roll picks a Layer first (gated by RequiredLayer, no immediate repeat), then independently
/// picks one of that Layer's Variations (no immediate repeat within that Layer).
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

    [SerializeField, Tooltip("The GameObject that contains the X to close eyes prompt. Shown while eyes are open.")]
    private GameObject m_CloseEyesPrompt;

    [SerializeField, Tooltip("Root GameObject of the dialogue UI, hidden while eyes are open.")]
    private GameObject m_DialogueUIRoot;

    [FormerlySerializedAs("m_VariationSources")]
    [SerializeField, Tooltip("Every PromptResponses Layer source for the current Level. Index 0 is treated as the default Layer.")]
    private List<PromptResponses> m_LayerSources;

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

    private int m_LastLayerIndex = -1;
    private List<int> m_LastVariationIndexPerLayer;
    private bool m_bIsEyesOpen;
    private bool m_bIsTransitioning;
    private bool m_bIsInitialized;
    private List<GameObject> m_AllVariationObjects;
    private PromptResponses.LayerVariation m_PendingVariation;
    private int? m_ForcedVariationIndexOverride;

    public event Action OnEyesClosed;

    /// <summary>
    /// Plasmalot: Forces the next OpenEyes() roll on the current Layer to pick this Variation index instead of
    /// rolling randomly. Consumed (cleared) by that single roll. Used by scripted sequences like the Forest tutorial.
    /// </summary>
    public void SetForcedVariationOverride(int variationIndex) => m_ForcedVariationIndexOverride = variationIndex;

    public PromptResponses CurrentVariationSource => m_LayerSources[m_LastLayerIndex];

    private void Awake()
    {
        _EnsureInitialized();
    }

    // m_SpotDifferenceCanvasRoot (this component's own GameObject) starts inactive, so Awake is
    // deferred until it's first activated. OpenEyes() needs these caches before that happens, so
    // it calls this too; the flag makes re-entry from both call sites harmless.
    private void _EnsureInitialized()
    {
        if (m_bIsInitialized) return;
        m_bIsInitialized = true;

        m_TopLidClosedPos = m_TopLid.anchoredPosition;
        m_TopLidOpenPos = m_TopLidClosedPos + Vector2.up * -m_TopLid.rect.height * 1.5f;

        m_BottomLidClosedPos = m_BottomLid.anchoredPosition;
        m_BottomLidOpenPos = m_BottomLidClosedPos + Vector2.down * -m_BottomLid.rect.height * 1.5f;

        m_LastVariationIndexPerLayer = new List<int>();
        m_AllVariationObjects = new List<GameObject>();
        foreach (PromptResponses layer in m_LayerSources)
        {
            m_LastVariationIndexPerLayer.Add(-1);

            foreach (PromptResponses.LayerVariation variation in layer.Variations)
            {
                foreach (GameObject obj in variation.VariationObjects)
                {
                    if (obj != null && !m_AllVariationObjects.Contains(obj))
                    {
                        m_AllVariationObjects.Add(obj);
                    }
                }
            }
        }
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
        _EnsureInitialized();

        m_PendingVariation = _ChooseVariation();
        m_VariationImageDisplay.sprite = m_PendingVariation.VariationImage;
        m_VariationImageDisplay.enabled = false;

        // Plasmalot: Disable every previous Variation's objects now, before the canvas becomes active, so none of them are momentarily visible during the fade-in. 
        // The chosen Variation's objects are only enabled once the eyelids actually begin opening.
        _DisableAllVariationObjects();

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
        m_CloseEyesPrompt.SetActive(true);
        _EnableVariationObjects(m_PendingVariation);

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
        m_CloseEyesPrompt.SetActive(false);

        m_bIsTransitioning = false;

        OnEyesClosed?.Invoke();
    }

    private PromptResponses.LayerVariation _ChooseVariation()
    {
        int currentLayer = GameProgressManager.Instance.GetCurrentLayer(LevelContext.Instance.CurrentLevelID);

        // The active Layer is static until GameProgressManager advances it, so this is a direct
        // lookup, not a roll: pick the highest-RequiredLayer source the Player has reached.
        int activeLayerIndex = -1;
        for (int i = 0; i < m_LayerSources.Count; i++)
        {
            if (m_LayerSources[i].Variations.Count == 0 || m_LayerSources[i].RequiredLayer > currentLayer) continue;

            if (activeLayerIndex == -1 || m_LayerSources[i].RequiredLayer > m_LayerSources[activeLayerIndex].RequiredLayer)
            {
                activeLayerIndex = i;
            }
        }
        m_LastLayerIndex = activeLayerIndex;

        IReadOnlyList<PromptResponses.LayerVariation> variations = m_LayerSources[activeLayerIndex].Variations;
        int lastVariationIndex = m_LastVariationIndexPerLayer[activeLayerIndex];

        int chosenVariationIndex;
        if (m_ForcedVariationIndexOverride.HasValue)
        {
            chosenVariationIndex = m_ForcedVariationIndexOverride.Value;
            m_ForcedVariationIndexOverride = null;
        }
        else if (lastVariationIndex == -1 || variations.Count == 1)
        {
            chosenVariationIndex = 0;
        }
        else
        {
            List<int> repeatCandidates = new List<int>();
            for (int i = 0; i < variations.Count; i++)
            {
                if (i != lastVariationIndex) repeatCandidates.Add(i);
            }
            chosenVariationIndex = repeatCandidates[UnityEngine.Random.Range(0, repeatCandidates.Count)];
        }
        m_LastVariationIndexPerLayer[activeLayerIndex] = chosenVariationIndex;

        return variations[chosenVariationIndex];
    }

    private void _DisableAllVariationObjects()
    {
        foreach (GameObject obj in m_AllVariationObjects)
        {
            obj.SetActive(false);
        }
    }

    private void _EnableVariationObjects(PromptResponses.LayerVariation chosenVariation)
    {
        foreach (GameObject obj in chosenVariation.VariationObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}
