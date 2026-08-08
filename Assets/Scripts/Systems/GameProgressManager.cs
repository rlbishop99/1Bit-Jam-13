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
    private readonly HashSet<string> m_ActivatedMarkerKeys = new HashSet<string>();

    private void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;

        if (transform.parent != null)
        {
            Debug.LogWarning($"[{name}] GameProgressManager was parented under '{transform.parent.name}'; unparenting so DontDestroyOnLoad actually persists it across scene loads.", this);
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);
    }

    public int GetCurrentLayer(GameEnums.eLevelID levelID) =>
        m_CurrentLayerByLevel.TryGetValue(levelID, out int layer) ? layer : 1;

    public void AdvanceLayer(GameEnums.eLevelID levelID, int newLayer)
    {
        if (newLayer > GetCurrentLayer(levelID))
        {
            m_CurrentLayerByLevel[levelID] = newLayer;
            Debug.Log($"[GameProgressManager] {levelID} advanced to Layer {newLayer}.");
        }
    }

    /// <summary>
    /// Plasmalot: Records that the given marker GameObject has been activated, and activates it now.
    /// Call ApplyPersistedMarkerState() on scene load to re-apply this to markers that start inactive in the scene.
    /// </summary>
    public void ActivateMarker(GameObject marker)
    {
        if (marker == null) return;

        m_ActivatedMarkerKeys.Add(_GetMarkerKey(marker));
        marker.SetActive(true);
    }

    /// <summary>
    /// Plasmalot: Re-activates marker if it was previously activated.
    /// Safe to call every time a marker's owning scene loads, even if marker was never activated.
    /// </summary>
    public void ApplyPersistedMarkerState(GameObject marker)
    {
        if (marker != null && m_ActivatedMarkerKeys.Contains(_GetMarkerKey(marker)))
        {
            marker.SetActive(true);
        }
    }

    private static string _GetMarkerKey(GameObject marker)
    {
        string path = marker.name;
        for (Transform parent = marker.transform.parent; parent != null; parent = parent.parent)
        {
            path = parent.name + "/" + path;
        }
        return marker.scene.name + "/" + path;
    }
}
