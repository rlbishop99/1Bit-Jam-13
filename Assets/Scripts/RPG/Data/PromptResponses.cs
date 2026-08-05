using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: One instance lives on the Level, one on each Variation. DialogueProcessor evaluates
/// Entries from every active PromptResponses component together, so a Variation's keywords/responses are simply additive to the Level's.
/// </summary>
public class PromptResponses : MonoBehaviour
{
    public enum ePresenceRequirement
    {
        MustBePresent,
        MustBeAbsent,
    }

    [Serializable]
    public struct Entry
    {
        [SerializeField, Tooltip("Keywords (from the assigned KeywordsSO word bank) required to trigger this response.")]
        private List<string> m_Keywords;

        [SerializeField, TextArea(5, 15), Tooltip("The response text printed to the screen when this Entry's Intent Threshold is met.")]
        private string m_Response;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Minimum Intent Score (0-100) this Entry's own input must reach to be eligible to trigger. Lower this for easy/forgiving prompts, raise it for prompts that require a more specific/precise input.")]
        private float m_RequiredIntentThreshold;

        [SerializeField, Tooltip("If true, triggering this Entry permanently advances the current Level's Layer.")]
        private bool m_bAdvancesLayer;

        [SerializeField, Min(1), Tooltip("The Layer the current Level advances to when this Entry triggers. Only used if Advances Layer is true.")]
        private int m_LayerToAdvanceTo;

        [SerializeField, Tooltip("If set, this Entry is only eligible when this object's presence (last rolled when eyes were opened) matches Presence Requirement. Leave unset for an ungated Entry.")]
        private GameObject m_GatingObject;

        [SerializeField, Tooltip("Whether the Gating Object must currently be present or absent for this Entry to be eligible. Ignored if Gating Object is unset.")]
        private ePresenceRequirement m_PresenceRequirement;

        public List<string> Keywords => m_Keywords;
        public string Response => m_Response;
        public float RequiredIntentThreshold => m_RequiredIntentThreshold;
        public bool AdvancesLayer => m_bAdvancesLayer;
        public int LayerToAdvanceTo => m_LayerToAdvanceTo;
        public GameObject GatingObject => m_GatingObject;
        public ePresenceRequirement PresenceRequirement => m_PresenceRequirement;

        public bool IsGateSatisfied()
        {
            if (m_GatingObject == null) return true;

            bool bIsPresent = m_GatingObject.activeSelf;
            return m_PresenceRequirement == ePresenceRequirement.MustBePresent ? bIsPresent : !bIsPresent;
        }
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
    public struct LayerVariation
    {
        [SerializeField, Tooltip("The Spot-the-Difference image shown when this Variation is rolled. Typically the same base image shared across every Variation on this Layer.")]
        private Sprite m_VariationImage;

        [SerializeField, Tooltip("Decorative objects enabled when this Variation is rolled. Every object referenced by any Variation on any Layer is disabled first, so only these remain active.")]
        private List<GameObject> m_VariationObjects;

        public Sprite VariationImage => m_VariationImage;
        public IReadOnlyList<GameObject> VariationObjects => m_VariationObjects;
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

    [SerializeField, Min(1), Tooltip("The Layer this Level/Variation's Entries, Transitions, EyeOpenEntries, and Variation Image belong to. Only active once the Level's current Layer reaches this value.")]
    private int m_RequiredLayer = 1;

    [SerializeField, Tooltip("Word bank of keywords available for this Level/Variation.")]
    private KeywordsSO m_KeywordsSO;

    [SerializeField, Tooltip("Level-wide Base Keywords bank, always legitimate regardless of Layer or Variation.")]
    private KeywordsSO m_BaseKeywordsSO;

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

    [SerializeField, Tooltip("Every Variation available on this Layer. One is randomly rolled (excluding the previous roll on this Layer) each time eyes open while this Layer is active.")]
    private List<LayerVariation> m_Variations = new List<LayerVariation>();

    public int RequiredLayer => m_RequiredLayer;
    public KeywordsSO KeywordsSO => m_KeywordsSO;
    public KeywordsSO BaseKeywordsSO => m_BaseKeywordsSO;
    public IReadOnlyList<Entry> PromptResponseEntries => m_PromptResponseEntries;
    public IReadOnlyList<TransitionEntry> TransitionEntries => m_TransitionEntries;
    public IReadOnlyList<EyeOpenEntry> EyeOpenEntries => m_EyeOpenEntries;
    public string FallbackResponse => m_FallbackResponse;
    public string IntroResponse => m_IntroResponse;
    public IReadOnlyList<LayerVariation> Variations => m_Variations;

    private void OnValidate()
    {
        for (int i = 0; i < m_PromptResponseEntries.Count; i++)
        {
            Entry entry = m_PromptResponseEntries[i];
            if (entry.AdvancesLayer && entry.LayerToAdvanceTo <= m_RequiredLayer)
            {
                Debug.LogWarning($"[{name}] PromptResponseEntries[{i}] AdvancesLayer to {entry.LayerToAdvanceTo}, which is not higher than this component's Required Layer ({m_RequiredLayer}). Layer To Advance To must be greater than the current Required Layer.", this);
            }
        }
    }
}
