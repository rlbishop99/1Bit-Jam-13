using System.Collections.Generic;

/// <summary>
/// Plasmalot: Owns every baked-in GlobalCommand and evaluates them against a submission's words.
/// Stateless and code-defined, so it needs no scene wiring - the same commands are available
/// identically in every Level/Layer/Variation without being assigned anywhere.
/// </summary>
public static class GlobalCommandManager
{
    private static readonly List<GlobalCommand> m_Commands = new List<GlobalCommand>
    {
        new ItemsGlobalCommand(),
        new ProgressGlobalCommand(),
        new HelpGlobalCommand(),
        new QuitGlobalCommand(),
    };

    public static IReadOnlyList<GlobalCommand> Commands => m_Commands;

    /// <summary>
    /// Plasmalot: Matches every registered GlobalCommand's KeywordSets against words. 
    /// Unlike PromptResponses, a GlobalCommand only fires when words consists of exactly its keyword set and nothing else.
    public static bool TryFindBestMatch(string[] words, float currentBestScore, GlobalCommandContext context, out string response, out float score)
    {
        response = null;
        score = currentBestScore;
        bool bFoundMatch = false;

        foreach (GlobalCommand command in m_Commands)
        {
            foreach (IReadOnlyList<string> keywordSet in command.KeywordSets)
            {
                if (!IsExactMatch(words, keywordSet)) continue;

                float commandScore = 100.0f;
                if (commandScore >= command.RequiredIntentThreshold && commandScore > score)
                {
                    score = commandScore;
                    response = command.GetResponse(context);
                    bFoundMatch = true;
                }
            }
        }

        return bFoundMatch;
    }

    private static bool IsExactMatch(string[] words, IReadOnlyList<string> keywordSet)
    {
        if (keywordSet == null || keywordSet.Count == 0 || words.Length != keywordSet.Count) return false;

        HashSet<string> inputWordSet = new HashSet<string>(words);
        foreach (string keyword in keywordSet)
        {
            if (!inputWordSet.Contains(keyword.ToLowerInvariant())) return false;
        }

        return true;
    }
}
