using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: One instance lives on the Level, one on each Variation. DialogueProcessor evaluates
/// Entries from every active PromptResponses component together, so a Variation's keywords/responses are simply additive to the Level's.
/// </summary>
public class PromptResponses : MonoBehaviour
{
    [Serializable]
    public struct Entry
    {
        [SerializeField, Tooltip("Keywords (from the assigned KeywordsSO word bank) required to trigger this response.")]
        private List<string> m_Keywords;

        [SerializeField, TextArea(5, 15), Tooltip("The response text printed to the screen when this Entry's Intent Threshold is met.")]
        private string m_Response;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Minimum Intent Score (0-100) this Entry's own input must reach to be eligible to trigger. Lower this for easy/forgiving prompts, raise it for prompts that require a more specific/precise input.")]
        private float m_RequiredIntentThreshold;

        public List<string> Keywords => m_Keywords;
        public string Response => m_Response;
        public float RequiredIntentThreshold => m_RequiredIntentThreshold;
    }

    [Serializable]
    public struct TransitionEntry
    {
        [SerializeField, Tooltip("Keywords (from the assigned KeywordsSO word bank) required to trigger this transition.")]
        private List<string> m_Keywords;

        [SerializeField, TextArea(3, 8), Tooltip("Response text printed before the Player transitions to the Target Level.")]
        private string m_Response;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Minimum Intent Score (0-100) this Entry's own input must reach to be eligible to trigger. Lower this for easy/forgiving prompts, raise it for prompts that require a more specific/precise input.")]
        private float m_RequiredIntentThreshold;

        [SerializeField, Tooltip("The Level the Player transitions to when this Entry triggers.")]
        private GameEnums.eLevelID m_TargetLevelID;

        public List<string> Keywords => m_Keywords;
        public string Response => m_Response;
        public float RequiredIntentThreshold => m_RequiredIntentThreshold;
        public GameEnums.eLevelID TargetLevelID => m_TargetLevelID;
    }

    [Serializable]
    public struct EyeOpenEntry
    {
        [SerializeField, Tooltip("Keywords (from the assigned KeywordsSO word bank) required to trigger opening the Player's eyes.")]
        private List<string> m_Keywords;

        [SerializeField, TextArea(3, 8), Tooltip("Response text printed before the Player's eyes open and the Spot-the-Difference image is revealed.")]
        private string m_Response;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Minimum Intent Score (0-100) this Entry's own input must reach to be eligible to trigger. Lower this for easy/forgiving prompts, raise it for prompts that require a more specific/precise input.")]
        private float m_RequiredIntentThreshold;

        public List<string> Keywords => m_Keywords;
        public string Response => m_Response;
        public float RequiredIntentThreshold => m_RequiredIntentThreshold;
    }

    [SerializeField, Tooltip("Word bank of keywords available for this Level/Variation.")]
    private KeywordsSO m_KeywordsSO;

    [SerializeField, Tooltip("Configurable list of Prompt -> Response mappings for this Level/Variation.")]
    private List<Entry> m_PromptResponseEntries = new List<Entry>();

    [SerializeField, Tooltip("Configurable list of Prompt -> Level Transition mappings for this Level/Variation.")]
    private List<TransitionEntry> m_TransitionEntries = new List<TransitionEntry>();

    [SerializeField, Tooltip("Configurable list of Prompt -> Open Eyes mappings for this Level/Variation.")]
    private List<EyeOpenEntry> m_EyeOpenEntries = new List<EyeOpenEntry>();

    [SerializeField, TextArea(2, 4), Tooltip("Response shown when no Entry meets the Intent Threshold.")]
    private string m_FallbackResponse = "Nothing happens.";

    [SerializeField, TextArea(2, 5), Tooltip("Text shown the first time the Player enters this Level/Variation.")]
    private string m_IntroResponse;

    [SerializeField, Tooltip("The Spot-the-Difference image shown for this Level/Variation when the Player opens their eyes.")]
    private Sprite m_VariationImage;

    public KeywordsSO KeywordsSO => m_KeywordsSO;
    public IReadOnlyList<Entry> PromptResponseEntries => m_PromptResponseEntries;
    public IReadOnlyList<TransitionEntry> TransitionEntries => m_TransitionEntries;
    public IReadOnlyList<EyeOpenEntry> EyeOpenEntries => m_EyeOpenEntries;
    public string FallbackResponse => m_FallbackResponse;
    public string IntroResponse => m_IntroResponse;
    public Sprite VariationImage => m_VariationImage;
}
