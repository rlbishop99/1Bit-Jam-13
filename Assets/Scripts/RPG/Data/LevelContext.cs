using UnityEngine;

/// <summary>
/// Plasmalot: Minimal scaffolding so PromptResponses/DialogueProcessor can tag which Level/Variation
/// is active. Full Level-loading/switching logic will be added later.
/// </summary>
public class LevelContext : MonoBehaviour
{
    private static LevelContext m_Instance;
    public static LevelContext Instance => m_Instance;

    [SerializeField, Tooltip("The Level currently active in the scene.")]
    private GameEnums.eLevelID m_CurrentLevelID = GameEnums.eLevelID.Forest;

    [SerializeField, Tooltip("The Variation of the current Level currently active in the scene.")]
    private GameEnums.eVariationID m_CurrentVariationID = GameEnums.eVariationID.Default;

    public GameEnums.eLevelID CurrentLevelID => m_CurrentLevelID;
    public GameEnums.eVariationID CurrentVariationID => m_CurrentVariationID;

    private void Awake()
    {
        m_Instance = this;
    }

    public void SetLevelAndVariation(GameEnums.eLevelID levelID, GameEnums.eVariationID variationID)
    {
        m_CurrentLevelID = levelID;
        m_CurrentVariationID = variationID;
    }
}
