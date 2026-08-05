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
    };

    public static IReadOnlyList<GlobalCommand> Commands => m_Commands;

    /// <summary>
    /// Plasmalot: Scores every registered GlobalCommand's KeywordSets against words, only reporting a match
    /// if it beats currentBestScore (whatever the Level/Variation's own Entries already found).
    /// </summary>
    public static bool TryFindBestMatch(string[] words, float currentBestScore, GlobalCommandContext context, out string response, out float score)
    {
        response = null;
        score = currentBestScore;
        bool bFoundMatch = false;

        foreach (GlobalCommand command in m_Commands)
        {
            foreach (IReadOnlyList<string> keywordSet in command.KeywordSets)
            {
                float commandScore = IntentScorer.CalculateIntentScore(words, keywordSet);
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
}
