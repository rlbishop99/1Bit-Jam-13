using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Plasmalot: Global "items"/"inventory" command - prints the names of every Item currently in the Player's inventory.
/// </summary>
public class ItemsGlobalCommand : GlobalCommand
{
    private static readonly List<IReadOnlyList<string>> m_KeywordSets = new List<IReadOnlyList<string>>
    {
        new List<string> { "items" },
        new List<string> { "inventory" },
        new List<string> { "item" },
    };

    public override string DisplayName => "items";
    public override IReadOnlyList<IReadOnlyList<string>> KeywordSets => m_KeywordSets;

    public override string GetResponse(GlobalCommandContext context)
    {
        IReadOnlyList<ItemSO> items = ItemsManager.Instance.Items;
        if (items.Count == 0) return "You aren't carrying any items.";

        return "ITEMS: " + string.Join(", ", items.Select(item => item.ItemName));
    }
}
