using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Plasmalot: Global "help" command - prints a random sample of the current Layer's recognized words
/// (from every active source's KeywordsSO/BaseKeywordsSO) plus the full list of GlobalCommands.
/// </summary>
public class HelpGlobalCommand : GlobalCommand
{
    private const int m_kSampleSize = 3;

    private static readonly List<IReadOnlyList<string>> m_KeywordSets = new List<IReadOnlyList<string>>
    {
        new List<string> { "help" },
    };

    public override string DisplayName => "help";
    public override IReadOnlyList<IReadOnlyList<string>> KeywordSets => m_KeywordSets;

    public override string GetResponse(GlobalCommandContext context)
    {
        HashSet<string> sampleWords = new HashSet<string>();

        foreach (PromptResponses source in context.ActiveSources)
        {
            if (source.RequiredLayer > context.CurrentLayer) continue;

            if (source.KeywordsSO != null) _AddSample(sampleWords, source.KeywordsSO.Keywords);
            if (source.BaseKeywordsSO != null) _AddSample(sampleWords, source.BaseKeywordsSO.Keywords);
        }

        string helpfulWordsLine = sampleWords.Count > 0
            ? "Helpful words: " + string.Join(", ", sampleWords)
            : "Helpful words: none come to mind right now.";

        string globalCommandsLine = "Global commands: " + string.Join(", ", GlobalCommandManager.Commands.Select(command => command.DisplayName));

        return $"{helpfulWordsLine}\n{globalCommandsLine}";
    }

    private static void _AddSample(HashSet<string> destination, IReadOnlyList<string> pool)
    {
        if (pool == null || pool.Count == 0) return;

        List<string> remaining = new List<string>(pool);
        int sampleCount = Mathf.Min(m_kSampleSize, remaining.Count);

        for (int i = 0; i < sampleCount; i++)
        {
            int index = Random.Range(0, remaining.Count);
            destination.Add(remaining[index]);
            remaining.RemoveAt(index);
        }
    }
}
