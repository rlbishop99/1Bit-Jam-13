using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: Singleton that holds the Player's persistent inventory of granted Items for the
/// duration of the play session, surviving scene loads.
/// </summary>
public class ItemsManager : MonoBehaviour
{
    private static ItemsManager m_Instance;
    public static ItemsManager Instance => m_Instance;

    [SerializeField, Min(0), Tooltip("Total number of collectible Items in the game, used by the Progress global command. Update this if the total changes.")]
    private int m_TotalCollectibleCount = 8;

    private readonly List<ItemSO> m_Items = new List<ItemSO>();

    public IReadOnlyList<ItemSO> Items => m_Items;
    public int TotalCollectibleCount => m_TotalCollectibleCount;

    /// <summary>
    /// Plasmalot: Raised after an Item is added to the inventory.
    /// </summary>
    public event Action<ItemSO> OnItemGranted;

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
            Debug.LogWarning($"[{name}] ItemsManager was parented under '{transform.parent.name}'; unparenting so DontDestroyOnLoad actually persists it across scene loads.", this);
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);
    }

    public bool HasItem(ItemSO item) => m_Items.Contains(item);

    public void GrantItem(ItemSO item)
    {
        if (item == null || HasItem(item)) return;

        m_Items.Add(item);
        Debug.Log($"[ItemsManager] Granted Item '{item.ItemName}'.");
        OnItemGranted?.Invoke(item);
    }
}
