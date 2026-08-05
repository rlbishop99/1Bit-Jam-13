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

    [SerializeField, Tooltip("Every PromptResponses source active for the current Level/Variation. Index 0 is treated as the Level source (used for its FallbackResponse/IntroResponse); the rest are Variation sources.")]
    private List<PromptResponses> m_ActivePromptResponsesSources;

    [SerializeField, Tooltip("Owns the Text-RPG to Spot-the-Difference mode swap triggered by an EyeOpenEntry.")]
    private EyeModeController m_EyeModeController;

    private bool m_bHasPlayedIntro;
    private bool m_bPendingTransition;
    private GameEnums.eLevelID m_PendingTargetLevelID;
    private bool m_bPendingEyeOpen;
    private bool m_bPendingLayerAdvance;
    private int m_PendingLayerToAdvanceTo;

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

        m_InputHandler.LockInput();
        m_TypewriterDisplay.PlayTypewriter(m_ActivePromptResponsesSources[0].IntroResponse, _OnIntroComplete);
    }

    private void _OnIntroComplete()
    {
        m_bHasPlayedIntro = true;
        m_InputHandler.UnlockInput();
    }

    private void _HandleInputSubmitted(string rawInput)
    {
        string[] words = InputSanitizer.SanitizeAndSplit(rawInput);
        int currentLayer = GameProgressManager.Instance.GetCurrentLayer(LevelContext.Instance.CurrentLevelID);

        float bestScore = -1.0f;
        string bestResponse = null;
        bool bFoundEligibleMatch = false;
        bool bBestIsTransition = false;
        bool bBestIsEyeOpen = false;
        bool bBestAdvancesLayer = false;
        GameEnums.eLevelID bestTargetLevelID = default;
        int bestLayerToAdvanceTo = default;

        foreach (PromptResponses source in m_ActivePromptResponsesSources)
        {
            if (source.RequiredLayer > currentLayer) continue;

            foreach (PromptResponses.Entry entry in source.PromptResponseEntries)
            {
                if (!entry.IsGateSatisfied()) continue;

                float score = IntentScorer.CalculateIntentScore(words, entry.Keywords);
                if (score >= entry.RequiredIntentThreshold && score > bestScore)
                {
                    bestScore = score;
                    bestResponse = entry.Response;
                    bFoundEligibleMatch = true;
                    bBestIsTransition = false;
                    bBestIsEyeOpen = false;
                    bBestAdvancesLayer = entry.AdvancesLayer;
                    bestLayerToAdvanceTo = entry.LayerToAdvanceTo;
                }
            }

            foreach (PromptResponses.TransitionEntry entry in source.TransitionEntries)
            {
                float score = IntentScorer.CalculateIntentScore(words, entry.Keywords);
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
                }
            }

            foreach (PromptResponses.EyeOpenEntry entry in source.EyeOpenEntries)
            {
                float score = IntentScorer.CalculateIntentScore(words, entry.Keywords);
                if (score >= entry.RequiredIntentThreshold && score > bestScore)
                {
                    bestScore = score;
                    bestResponse = entry.Response;
                    bFoundEligibleMatch = true;
                    bBestIsTransition = false;
                    bBestIsEyeOpen = true;
                    bBestAdvancesLayer = false;
                    bestLayerToAdvanceTo = default;
                }
            }
        }

        string chosenResponse = bFoundEligibleMatch
            ? bestResponse
            : m_ActivePromptResponsesSources[0].FallbackResponse;

        m_bPendingTransition = bFoundEligibleMatch && bBestIsTransition;
        m_PendingTargetLevelID = bestTargetLevelID;
        m_bPendingEyeOpen = bFoundEligibleMatch && bBestIsEyeOpen;
        m_bPendingLayerAdvance = bFoundEligibleMatch && bBestAdvancesLayer;
        m_PendingLayerToAdvanceTo = bestLayerToAdvanceTo;

        m_TypewriterDisplay.PlayTypewriter(chosenResponse, _OnResponseComplete);
    }

    private void _OnResponseComplete()
    {
        if (m_bPendingLayerAdvance)
        {
            m_bPendingLayerAdvance = false;
            GameProgressManager.Instance.AdvanceLayer(LevelContext.Instance.CurrentLevelID, m_PendingLayerToAdvanceTo);
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

        m_InputHandler.UnlockInput();
    }

    private void _HandleEyesClosed()
    {
        m_InputHandler.ClearInput();
        m_TypewriterDisplay.PlayTypewriter(m_EyeModeController.CurrentVariationSource.IntroResponse, _OnEyesClosedIntroComplete);
    }

    private void _OnEyesClosedIntroComplete()
    {
        m_InputHandler.UnlockInput();
    }
}
