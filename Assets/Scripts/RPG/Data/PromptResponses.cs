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
    public struct GatingCondition
    {
        [SerializeField, Tooltip("The object whose presence is checked. Leave unset to remove this condition.")]
        private GameObject m_GatingObject;

        [SerializeField, Tooltip("Whether the Gating Object must currently be present or absent for this condition to be satisfied.")]
        private GameEnums.ePresenceRequirement m_PresenceRequirement;

        public GatingCondition(GameObject gatingObject, GameEnums.ePresenceRequirement presenceRequirement)
        {
            m_GatingObject = gatingObject;
            m_PresenceRequirement = presenceRequirement;
        }

        public GameObject GatingObject => m_GatingObject;
        public GameEnums.ePresenceRequirement PresenceRequirement => m_PresenceRequirement;

        public bool IsSatisfied()
        {
            if (m_GatingObject == null) return true;

            bool bIsPresent = m_GatingObject.activeSelf;
            return m_PresenceRequirement == GameEnums.ePresenceRequirement.MustBePresent ? bIsPresent : !bIsPresent;
        }
    }

    [Serializable]
    public struct Entry : ISerializationCallbackReceiver
    {
        [SerializeField, Tooltip("Legacy flat keyword list, kept only so old data can be migrated into Keyword Groups on load. No longer edited directly.")]
        private List<string> m_Keywords;

        [SerializeField, Tooltip("Groups of interchangeable keywords required to trigger this response. Every group must have at least one matching word present.")]
        private List<KeywordGroup> m_KeywordGroups;

        [SerializeField, TextArea(5, 15), Tooltip("The response text printed to the screen when this Entry's Intent Threshold is met.")]
        private string m_Response;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Minimum Intent Score (0-100) this Entry's own input must reach to be eligible to trigger. Lower this for easy/forgiving prompts, raise it for prompts that require a more specific/precise input.")]
        private float m_RequiredIntentThreshold;

        [SerializeField, Tooltip("If true, triggering this Entry permanently advances the current Level's Layer.")]
        private bool m_bAdvancesLayer;

        [SerializeField, Min(1), Tooltip("The Layer the current Level advances to when this Entry triggers. Only used if Advances Layer is true.")]
        private int m_LayerToAdvanceTo;

        [SerializeField, Tooltip("If true, triggering this Entry launches the Dating Sim instead of unlocking input once the Response finishes typing.")]
        private bool m_bStartsDatingSim;

        [SerializeField, Tooltip("Legacy single gating object, kept only so old data can be migrated into Gating Conditions on load. No longer edited directly.")]
        private GameObject m_GatingObject;

        [SerializeField, Tooltip("Legacy single gating requirement, kept only so old data can be migrated into Gating Conditions on load. No longer edited directly.")]
        private GameEnums.ePresenceRequirement m_PresenceRequirement;

        [SerializeField, Tooltip("Every condition that must be satisfied (AND) for this Entry to be eligible. Leave empty for an ungated Entry.")]
        private List<GatingCondition> m_GatingConditions;

        [SerializeField, Tooltip("The SFX that plays when this Entry is triggered. Optional.")]
        private AudioClip m_TriggerSFX;

        [SerializeField, Tooltip("If set, triggering this Entry grants this Item to the Player's inventory and removes this Entry from future consideration, so it can only be granted once. Leave unset for an Entry with no reward.")]
        private ItemSO m_RewardItem;

        [SerializeField, Tooltip("If set, triggering this Entry activates this marker GameObject. Optional.")]
        private GameObject m_MarkerToActivate;

        public List<KeywordGroup> KeywordGroups => m_KeywordGroups;
        public string Response => m_Response;
        public float RequiredIntentThreshold => m_RequiredIntentThreshold;
        public bool AdvancesLayer => m_bAdvancesLayer;
        public int LayerToAdvanceTo => m_LayerToAdvanceTo;
        public bool StartsDatingSim => m_bStartsDatingSim;
        public IReadOnlyList<GatingCondition> GatingConditions => m_GatingConditions;
        public AudioClip TriggerSFX => m_TriggerSFX;
        public ItemSO RewardItem => m_RewardItem;
        public GameObject MarkerToActivate => m_MarkerToActivate;

        public bool IsGateSatisfied()
        {
            if (m_GatingConditions == null) return true;

            foreach (GatingCondition condition in m_GatingConditions)
            {
                if (!condition.IsSatisfied()) return false;
            }
            return true;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            KeywordGroupMigration.MigrateLegacyKeywords(m_Keywords, ref m_KeywordGroups);
            GatingConditionMigration.MigrateLegacyGatingObject(m_GatingObject, m_PresenceRequirement, ref m_GatingConditions);
        }
    }

    [Serializable]
    public struct TransitionEntry : ISerializationCallbackReceiver
    {
        [SerializeField, Tooltip("Legacy flat keyword list, kept only so old data can be migrated into Keyword Groups on load. No longer edited directly.")]
        private List<string> m_Keywords;

        [SerializeField, Tooltip("Groups of interchangeable keywords required to trigger this transition. Every group must have at least one matching word present.")]
        private List<KeywordGroup> m_KeywordGroups;

        [SerializeField, TextArea(3, 8), Tooltip("Response text printed before the Player transitions to the Target Level.")]
        private string m_Response;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Minimum Intent Score (0-100) this Entry's own input must reach to be eligible to trigger. Lower this for easy/forgiving prompts, raise it for prompts that require a more specific/precise input.")]
        private float m_RequiredIntentThreshold;

        [SerializeField, Tooltip("The Level the Player transitions to when this Entry triggers.")]
        private GameEnums.eLevelID m_TargetLevelID;

        public List<KeywordGroup> KeywordGroups => m_KeywordGroups;
        public string Response => m_Response;
        public float RequiredIntentThreshold => m_RequiredIntentThreshold;
        public GameEnums.eLevelID TargetLevelID => m_TargetLevelID;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            KeywordGroupMigration.MigrateLegacyKeywords(m_Keywords, ref m_KeywordGroups);
        }
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
    public struct EyeOpenEntry : ISerializationCallbackReceiver
    {
        [SerializeField, Tooltip("Legacy flat keyword list, kept only so old data can be migrated into Keyword Groups on load. No longer edited directly.")]
        private List<string> m_Keywords;

        [SerializeField, Tooltip("Groups of interchangeable keywords required to trigger opening the Player's eyes. Every group must have at least one matching word present;")]
        private List<KeywordGroup> m_KeywordGroups;

        [SerializeField, TextArea(3, 8), Tooltip("Response text printed before the Player's eyes open and the Spot-the-Difference image is revealed.")]
        private string m_Response;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Minimum Intent Score (0-100) this Entry's own input must reach to be eligible to trigger. Lower this for easy/forgiving prompts, raise it for prompts that require a more specific/precise input.")]
        private float m_RequiredIntentThreshold;

        public List<KeywordGroup> KeywordGroups => m_KeywordGroups;
        public string Response => m_Response;
        public float RequiredIntentThreshold => m_RequiredIntentThreshold;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            KeywordGroupMigration.MigrateLegacyKeywords(m_Keywords, ref m_KeywordGroups);
        }
    }

    [SerializeField, Min(1), Tooltip("The Layer this Level/Variation's Entries, Transitions, EyeOpenEntries, and Variation Image belong to. Only active once the Level's current Layer reaches this value.")]
    private int m_RequiredLayer = 1;

    [SerializeField, Tooltip("Marks this as a Level source. DialogueProcessor sources its FallbackResponse/IntroResponse from whichever Level source has the highest Required Layer.")]
    private bool m_bIsLevelSource;

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
    public bool IsLevelSource => m_bIsLevelSource;
    public KeywordsSO KeywordsSO => m_KeywordsSO;
    public KeywordsSO BaseKeywordsSO => m_BaseKeywordsSO;
    public IReadOnlyList<Entry> PromptResponseEntries => m_PromptResponseEntries;
    public IReadOnlyList<TransitionEntry> TransitionEntries => m_TransitionEntries;
    public IReadOnlyList<EyeOpenEntry> EyeOpenEntries => m_EyeOpenEntries;
    public string FallbackResponse => m_FallbackResponse;
    public string IntroResponse => m_IntroResponse;
    public IReadOnlyList<LayerVariation> Variations => m_Variations;

    /// <summary>
    /// Plasmalot: Removes the Entry at index from PromptResponseEntries so it can't be re-triggered.
    /// </summary>
    public void RemoveEntryAt(int index) => m_PromptResponseEntries.RemoveAt(index);

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
