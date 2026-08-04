using System.Collections.Generic;

/// <summary>
/// Plasmalot: Static lookup from a Level's eLevelID to the name of the Unity Scene that represents it,
/// used by LevelContext to resolve which Scene to load on transition.
/// </summary>
public static class LevelSceneMap
{
    private static readonly Dictionary<GameEnums.eLevelID, string> m_kSceneNamesByLevelID = new Dictionary<GameEnums.eLevelID, string>
    {
        { GameEnums.eLevelID.Forest, "Forest" },
        { GameEnums.eLevelID.Glade, "Glade" },
        { GameEnums.eLevelID.Bridge, "Bridge" },
        { GameEnums.eLevelID.Cave, "Cave" },
        { GameEnums.eLevelID.Cemetery, "Cemetery" },
        { GameEnums.eLevelID.WizardTower, "Wizard Tower" },
        { GameEnums.eLevelID.FairyHouse, "Fairy House" },
    };

    public static string GetSceneName(GameEnums.eLevelID levelID) => m_kSceneNamesByLevelID[levelID];
}
