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

        [SerializeField, TextArea(2, 5), Tooltip("The response text printed to the screen when this Entry's Intent Threshold is met.")]
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

    [SerializeField, TextArea(2, 4), Tooltip("Response shown when no Entry meets the Intent Threshold.")]
    private string m_FallbackResponse = "Nothing happens.";

    [SerializeField, TextArea(2, 5), Tooltip("Text shown the first time the Player enters this Level/Variation.")]
    private string m_IntroResponse;

    public KeywordsSO KeywordsSO => m_KeywordsSO;
    public IReadOnlyList<Entry> PromptResponseEntries => m_PromptResponseEntries;
    public string FallbackResponse => m_FallbackResponse;
    public string IntroResponse => m_IntroResponse;
}
