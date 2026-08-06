using System.Collections.Generic;

/// <summary>
/// Plasmalot: Static utility class that calculates an Intent score based on the number of required keywords present in the Player's input.
/// </summary>
public static class IntentScorer
{
    public static float CalculateIntentScore(string[] inputWords, IReadOnlyList<string> requiredKeywords)
    {
        if (requiredKeywords == null || requiredKeywords.Count == 0) return 0.0f;

        HashSet<string> inputWordSet = new HashSet<string>(inputWords);
        int matchedCount = 0;

        foreach (string keyword in requiredKeywords)
        {
            if (inputWordSet.Contains(keyword.ToLowerInvariant()))
            {
                matchedCount++;
            }
        }

        float score = (matchedCount / (float)requiredKeywords.Count) * 100.0f;
        return score > 100.0f ? 100.0f : score;
    }

    /// <summary>
    /// Plasmalot: Same fraction-based scoring as the flat-keyword overload, but each KeywordGroup is one required "concept
    /// slot" - any single Synonym present in the input satisfies the whole group.
    /// </summary>
    public static float CalculateIntentScore(string[] inputWords, IReadOnlyList<KeywordGroup> requiredGroups)
    {
        if (requiredGroups == null || requiredGroups.Count == 0) return 0.0f;

        HashSet<string> inputWordSet = new HashSet<string>(inputWords);
        int matchedGroups = 0;

        foreach (KeywordGroup group in requiredGroups)
        {
            if (group.Synonyms == null) continue;

            foreach (string synonym in group.Synonyms)
            {
                if (inputWordSet.Contains(synonym.ToLowerInvariant()))
                {
                    matchedGroups++;
                    break;
                }
            }
        }

        float score = (matchedGroups / (float)requiredGroups.Count) * 100.0f;
        return score > 100.0f ? 100.0f : score;
    }
}
