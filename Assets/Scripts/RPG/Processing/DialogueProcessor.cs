using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: Handles the processing of Player input and determines which response to display based on the keywords present in the input. 
/// This component is responsible for evaluating all active PromptResponses sources and selecting the most appropriate response. 
/// </summary>
public class DialogueProcessor : MonoBehaviour
{
    [SerializeField, Tooltip("Handles raw keyboard capture/submission.")]
    private KeyboardInputHandler m_InputHandler;

    [SerializeField, Tooltip("Types responses to the screen.")]
    private TypewriterDisplay m_TypewriterDisplay;

    [SerializeField, Tooltip("Every PromptResponses source active for the current Level/Variation.")]
    private List<PromptResponses> m_ActivePromptResponsesSources;

    [SerializeField, Tooltip("Owns the Text-RPG to Spot-the-Difference mode swap triggered by an EyeOpenEntry.")]
    private EyeModeController m_EyeModeController;

    private bool m_bHasPlayedIntro;
    private bool m_bPendingTransition;
    private GameEnums.eLevelID m_PendingTargetLevelID;
    private bool m_bPendingEyeOpen;
    private bool m_bPendingLayerAdvance;
    private int m_PendingLayerToAdvanceTo;
    private ItemSO m_PendingRewardItem;
    private PromptResponses m_PendingRewardItemSource;
    private int m_PendingRewardItemEntryIndex;
    private string m_EyesClosedResponseOverride;
    private AudioClip m_EyesClosedResponseOverrideSFX;
    private string m_NextResponseOverride;
    private AudioClip m_NextResponseOverrideSFX;
    private bool m_bSuppressNextUnlock;

    /// <summary>
    /// Plasmalot: Fired once the eyes-closed response (forced or default) has finished typing and input has
    /// unlocked. Scripted sequences can use this as the safe point to arm the *next* eyes-closed/variation
    /// override, since by now any override for the close that just happened has already been consumed.
    /// </summary>
    public event Action OnEyesClosedResponseComplete;

    /// <summary>
    /// Plasmalot: Fired just before input would normally unlock after a response with nothing pending. 
    /// Scripted sequences can call SuppressNextUnlock() from a handler to keep input locked instead.
    /// </summary>
    public event Action OnBeforeInputUnlock;

    /// <summary>
    /// Plasmalot: If set, called after every submission is scoredbut before its outcome is committed. Return null to let the
    /// resolved outcome through unchanged, or a fallback string to show that text instead and cancel any
    /// transition/eye-open/layer-advance this submission would otherwise have triggered. 
    /// Used by scripted sequences to restrict input to only the one action they're currently expecting.
    /// </summary>
    public Func<bool, bool, bool, string> ResponseGate { get; set; }

    /// <summary>
    /// Plasmalot: Overrides the single next "eyes closed" response with this text instead of
    /// CurrentVariationSource.IntroResponse, optionally playing sfx the moment it's actually displayed.
    /// Consumed by that one close. Used by scripted sequences.
    /// </summary>
    public void SetNextEyesClosedResponse(string overrideText, AudioClip sfx = null)
    {
        m_EyesClosedResponseOverride = overrideText;
        m_EyesClosedResponseOverrideSFX = sfx;
    }

    /// <summary>
    /// Plasmalot: Replaces the entire next input submission's handling - no keyword scoring runs, this text is
    /// shown as the response instead (optionally playing sfx the moment it's actually displayed), and no
    /// transition/eye-open/layer-advance can be triggered by it. Consumed by that one submission. Used by
    /// scripted sequences to replace a prompt with forced dialogue.
    /// </summary>
    public void SetNextResponseOverride(string overrideText, AudioClip sfx = null)
    {
        m_NextResponseOverride = overrideText;
        m_NextResponseOverrideSFX = sfx;
    }

    /// <summary>Suppresses the input unlock that would otherwise follow the OnBeforeInputUnlock this frame.</summary>
    public void SuppressNextUnlock() => m_bSuppressNextUnlock = true;

    private void OnEnable()
    {
        m_InputHandler.OnInputSubmitted += _HandleInputSubmitted;
        m_EyeModeController.OnEyesClosed += _HandleEyesClosed;
    }

    private void OnDisable()
    {
        m_InputHandler.OnInputSubmitted -= _HandleInputSubmitted;
        m_EyeModeController.OnEyesClosed -= _HandleEyesClosed;
    }

    private void Start()
    {
        if (m_bHasPlayedIntro || m_ActivePromptResponsesSources == null || m_ActivePromptResponsesSources.Count == 0) return;

        int currentLayer = GameProgressManager.Instance.GetCurrentLayer(LevelContext.Instance.CurrentLevelID);
        PromptResponses activeLevelSource = _ResolveActiveLevelSource(currentLayer);

        Debug.Log($"[DialogueProcessor] Start(): Level={LevelContext.Instance.CurrentLevelID}, Layer={currentLayer}, ActiveLevelSource={(activeLevelSource != null ? activeLevelSource.name : "NULL")}, RequiredLayer={(activeLevelSource != null ? activeLevelSource.RequiredLayer.ToString() : "-")}");

        m_InputHandler.LockInput();
        m_TypewriterDisplay.PlayTypewriter(activeLevelSource.IntroResponse, _OnIntroComplete);
    }

    /// <summary>
    /// Plasmalot: Picks whichever Is Level Source-flagged source has the highest Required Layer at or below
    /// currentLayer - the same "highest Required Layer reached" rule EyeModeController uses to pick the active
    /// Layer's Variations, so the Level's Intro/Fallback text always matches whichever Layer is actually active.
    /// </summary>
    private PromptResponses _ResolveActiveLevelSource(int currentLayer)
    {
        PromptResponses activeSource = null;
        foreach (PromptResponses source in m_ActivePromptResponsesSources)
        {
            if (!source.IsLevelSource || source.RequiredLayer > currentLayer) continue;

            if (activeSource == null || source.RequiredLayer > activeSource.RequiredLayer)
            {
                activeSource = source;
            }
        }
        return activeSource;
    }

    private void _OnIntroComplete()
    {
        m_bHasPlayedIntro = true;
        m_InputHandler.UnlockInput();
    }

    private void _HandleInputSubmitted(string rawInput)
    {
        if (m_NextResponseOverride != null)
        {
            string overrideResponse = m_NextResponseOverride;
            m_NextResponseOverride = null;

            m_bPendingTransition = false;
            m_bPendingEyeOpen = false;
            m_bPendingLayerAdvance = false;
            m_PendingRewardItem = null;
            m_PendingRewardItemSource = null;

            AudioManager.Instance.PlaySFXOneShot(m_NextResponseOverrideSFX);
            m_NextResponseOverrideSFX = null;

            m_TypewriterDisplay.PlayTypewriter(overrideResponse, _OnResponseComplete);
            return;
        }

        string[] words = InputSanitizer.SanitizeAndSplit(rawInput);
        int currentLayer = GameProgressManager.Instance.GetCurrentLayer(LevelContext.Instance.CurrentLevelID);
        PromptResponses activeLevelSource = _ResolveActiveLevelSource(currentLayer);

        float bestScore = -1.0f;
        string bestResponse = null;
        bool bFoundEligibleMatch = false;
        bool bBestIsTransition = false;
        bool bBestIsEyeOpen = false;
        bool bBestAdvancesLayer = false;
        GameEnums.eLevelID bestTargetLevelID = default;
        int bestLayerToAdvanceTo = default;
        ItemSO bestRewardItem = null;
        PromptResponses bestRewardItemSource = null;
        int bestRewardItemEntryIndex = default;
        AudioClip bestTriggerSFX = null;

        foreach (PromptResponses source in m_ActivePromptResponsesSources)
        {
            if (source.RequiredLayer > currentLayer) continue;

            IReadOnlyList<PromptResponses.Entry> entries = source.PromptResponseEntries;
            for (int i = 0; i < entries.Count; i++)
            {
                PromptResponses.Entry entry = entries[i];
                if (!entry.IsGateSatisfied()) continue;

                float score = IntentScorer.CalculateIntentScore(words, entry.KeywordGroups);
                if (score >= entry.RequiredIntentThreshold && score > bestScore)
                {
                    bestScore = score;
                    bestResponse = entry.Response;
                    bFoundEligibleMatch = true;
                    bBestIsTransition = false;
                    bBestIsEyeOpen = false;
                    bBestAdvancesLayer = entry.AdvancesLayer;
                    bestLayerToAdvanceTo = entry.LayerToAdvanceTo;
                    bestRewardItem = entry.RewardItem;
                    bestRewardItemSource = source;
                    bestRewardItemEntryIndex = i;
                    bestTriggerSFX = entry.TriggerSFX;
                }
            }

            foreach (PromptResponses.TransitionEntry entry in source.TransitionEntries)
            {
                float score = IntentScorer.CalculateIntentScore(words, entry.KeywordGroups);
                if (score >= entry.RequiredIntentThreshold && score > bestScore)
                {
                    bestScore = score;
                    bestResponse = entry.Response;
                    bFoundEligibleMatch = true;
                    bBestIsTransition = true;
                    bBestIsEyeOpen = false;
                    bestTargetLevelID = entry.TargetLevelID;
                    bBestAdvancesLayer = false;
                    bestLayerToAdvanceTo = default;
                    bestRewardItem = null;
                    bestRewardItemSource = null;
                    bestTriggerSFX = null;
                }
            }

            foreach (PromptResponses.EyeOpenEntry entry in source.EyeOpenEntries)
            {
                float score = IntentScorer.CalculateIntentScore(words, entry.KeywordGroups);
                if (score >= entry.RequiredIntentThreshold && score > bestScore)
                {
                    bestScore = score;
                    bestResponse = entry.Response;
                    bFoundEligibleMatch = true;
                    bBestIsTransition = false;
                    bBestIsEyeOpen = true;
                    bBestAdvancesLayer = false;
                    bestLayerToAdvanceTo = default;
                    bestRewardItem = null;
                    bestRewardItemSource = null;
                    bestTriggerSFX = null;
                }
            }
        }

        GlobalCommandContext globalCommandContext = new GlobalCommandContext(m_ActivePromptResponsesSources, currentLayer);
        if (GlobalCommandManager.TryFindBestMatch(words, bestScore, globalCommandContext, out string globalCommandResponse, out float globalCommandScore))
        {
            bestScore = globalCommandScore;
            bestResponse = globalCommandResponse;
            bFoundEligibleMatch = true;
            bBestIsTransition = false;
            bBestIsEyeOpen = false;
            bBestAdvancesLayer = false;
            bestLayerToAdvanceTo = default;
            bestRewardItem = null;
            bestRewardItemSource = null;
            bestTriggerSFX = null;
        }

        string chosenResponse = bFoundEligibleMatch
            ? bestResponse
            : activeLevelSource.FallbackResponse;

        string gateFallback = ResponseGate?.Invoke(bFoundEligibleMatch, bBestIsTransition, bBestIsEyeOpen);
        if (gateFallback != null)
        {
            chosenResponse = gateFallback;
            bFoundEligibleMatch = false;
        }

        m_bPendingTransition = bFoundEligibleMatch && bBestIsTransition;
        m_PendingTargetLevelID = bestTargetLevelID;
        m_bPendingEyeOpen = bFoundEligibleMatch && bBestIsEyeOpen;
        m_bPendingLayerAdvance = bFoundEligibleMatch && bBestAdvancesLayer;
        m_PendingLayerToAdvanceTo = bestLayerToAdvanceTo;
        m_PendingRewardItem = bFoundEligibleMatch ? bestRewardItem : null;
        m_PendingRewardItemSource = bFoundEligibleMatch ? bestRewardItemSource : null;
        m_PendingRewardItemEntryIndex = bestRewardItemEntryIndex;

        if (bFoundEligibleMatch && bestTriggerSFX != null)
        {
            AudioManager.Instance.PlaySFXOneShot(bestTriggerSFX);
        }

        m_TypewriterDisplay.PlayTypewriter(chosenResponse, _OnResponseComplete);
    }

    private void _OnResponseComplete()
    {
        if (m_bPendingLayerAdvance)
        {
            m_bPendingLayerAdvance = false;
            GameProgressManager.Instance.AdvanceLayer(LevelContext.Instance.CurrentLevelID, m_PendingLayerToAdvanceTo);
        }

        if (m_PendingRewardItem != null)
        {
            ItemSO rewardItem = m_PendingRewardItem;
            PromptResponses rewardItemSource = m_PendingRewardItemSource;
            int rewardItemEntryIndex = m_PendingRewardItemEntryIndex;
            m_PendingRewardItem = null;
            m_PendingRewardItemSource = null;

            ItemsManager.Instance.GrantItem(rewardItem);
            rewardItemSource.RemoveEntryAt(rewardItemEntryIndex);

            Debug.Log($"[DialogueProcessor] Granted Reward Item '{rewardItem.ItemName}' from PromptResponses source '{rewardItemSource.name}' at Entry index {rewardItemEntryIndex}.");
        }

        if (m_bPendingTransition)
        {
            m_bPendingTransition = false;
            LevelContext.Instance.TransitionToLevel(m_PendingTargetLevelID);
            return;
        }

        if (m_bPendingEyeOpen)
        {
            m_bPendingEyeOpen = false;
            m_EyeModeController.OpenEyes();
            return;
        }

        OnBeforeInputUnlock?.Invoke();
        if (!m_bSuppressNextUnlock)
        {
            m_InputHandler.UnlockInput();
        }
        m_bSuppressNextUnlock = false;
    }

    private void _HandleEyesClosed()
    {
        m_InputHandler.ClearInput();

        string response = m_EyesClosedResponseOverride ?? m_EyeModeController.CurrentVariationSource.IntroResponse;
        m_EyesClosedResponseOverride = null;

        AudioManager.Instance.PlaySFXOneShot(m_EyesClosedResponseOverrideSFX);
        m_EyesClosedResponseOverrideSFX = null;

        m_TypewriterDisplay.PlayTypewriter(response, _OnEyesClosedIntroComplete);
    }

    private void _OnEyesClosedIntroComplete()
    {
        m_InputHandler.UnlockInput();
        OnEyesClosedResponseComplete?.Invoke();
    }
}
