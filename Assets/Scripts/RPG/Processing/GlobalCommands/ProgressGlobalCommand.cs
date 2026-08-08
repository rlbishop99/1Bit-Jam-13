using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: Global "progress" command - reports how many collectible Items have been found out of
/// ItemsManager's configured total (e.g. "3/8 items collected.").
/// </summary>
public class ProgressGlobalCommand : GlobalCommand
{
    private static readonly List<IReadOnlyList<string>> m_KeywordSets = new List<IReadOnlyList<string>>
    {
        new List<string> { "progress" },
    };

    public override string DisplayName => "progress";
    public override IReadOnlyList<IReadOnlyList<string>> KeywordSets => m_KeywordSets;

    public override string GetResponse(GlobalCommandContext context)
    {
        int collected = ItemsManager.Instance.EverCollectedCount;
        int total = ItemsManager.Instance.TotalCollectibleCount;
        return $"Progress: {collected}/{total} items collected.";
    }
}
