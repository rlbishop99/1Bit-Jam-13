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
    private readonly HashSet<ItemSO> m_EverCollectedItems = new HashSet<ItemSO>();

    public IReadOnlyList<ItemSO> Items => m_Items;
    public int TotalCollectibleCount => m_TotalCollectibleCount;

    /// <summary>
    /// Plasmalot: Count of every distinct Item ever granted this session, regardless of whether it was later removed.
    /// Only ever grows - use this for "X/Y collected" progress reporting instead of Items.Count, which reflects the current inventory.
    /// </summary>
    public int EverCollectedCount => m_EverCollectedItems.Count;

    /// <summary>
    /// Plasmalot: Raised after an Item is added to the inventory.
    /// </summary>
    public event Action<ItemSO> OnItemGranted;

    /// <summary>
    /// Plasmalot: Raised after an Item is removed from the inventory.
    /// </summary>
    public event Action<ItemSO> OnItemRemoved;

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

    /// <summary>
    /// Plasmalot: Whether this Item has ever been granted this session, regardless of whether it's still in the current inventory.
    /// </summary>
    public bool HasEverCollected(ItemSO item) => m_EverCollectedItems.Contains(item);

    public void GrantItem(ItemSO item)
    {
        if (item == null || HasItem(item)) return;

        m_Items.Add(item);
        m_EverCollectedItems.Add(item);
        Debug.Log($"[ItemsManager] Granted Item '{item.ItemName}'.");
        OnItemGranted?.Invoke(item);
    }

    /// <summary>
    /// Plasmalot: Removes item from the current inventory.
    /// Does not affect EverCollectedCount - once collected, an Item stays counted even after removal.
    /// </summary>
    public void RemoveItem(ItemSO item)
    {
        if (item == null || !m_Items.Remove(item)) return;

        Debug.Log($"[ItemsManager] Removed Item '{item.ItemName}'.");
        OnItemRemoved?.Invoke(item);
    }

    /// <summary>
    /// Plasmalot: Wipes the entire session's inventory and collection progress, e.g. when the Player quits back to the Title Screen.
    /// </summary>
    public void ResetProgress()
    {
        m_Items.Clear();
        m_EverCollectedItems.Clear();
    }
}
