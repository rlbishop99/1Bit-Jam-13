using UnityEngine;

/// <summary>
/// Plasmalot: ScriptableObject describing a collectible Item that can be granted to the Player  and stored in the persistent ItemsManager inventory.
/// </summary>
[CreateAssetMenu(fileName = "NewItemSO", menuName = "RPG/Item SO")]
public class ItemSO : ScriptableObject
{
    [SerializeField, Tooltip("Display name of the Item.")]
    private string m_ItemName;

    [SerializeField, TextArea(2, 5), Tooltip("Description of the Item.")]
    private string m_Description;

    [SerializeField, Tooltip("Icon representing the Item.")]
    private Sprite m_Icon;

    public string ItemName => m_ItemName;
    public string Description => m_Description;
    public Sprite Icon => m_Icon;
}
