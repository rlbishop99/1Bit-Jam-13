using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: Singleton that tracks the current Layer reached in each Level for the duration of the play session.
/// Layers only ever advance forward (never back down); every Level starts at Layer 1.
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    private static GameProgressManager m_Instance;
    public static GameProgressManager Instance => m_Instance;

    private readonly Dictionary<GameEnums.eLevelID, int> m_CurrentLayerByLevel = new Dictionary<GameEnums.eLevelID, int>();

    private void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetCurrentLayer(GameEnums.eLevelID levelID) =>
        m_CurrentLayerByLevel.TryGetValue(levelID, out int layer) ? layer : 1;

    public void AdvanceLayer(GameEnums.eLevelID levelID, int newLayer)
    {
        if (newLayer > GetCurrentLayer(levelID))
        {
            m_CurrentLayerByLevel[levelID] = newLayer;
        }
    }
}
