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
}
