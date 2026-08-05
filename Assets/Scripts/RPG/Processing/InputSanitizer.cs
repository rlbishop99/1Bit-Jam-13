using System;
using System.Text.RegularExpressions;

/// <summary>
/// Plasmalot: Static utility class that sanitizes Player input for use by the DialogueProcessor. 
/// Removes non-alphanumeric characters and splits the input into an array of lowercase words.
/// </summary>
public static class InputSanitizer
{
    private static readonly Regex m_kNonAlphanumericRegex = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);

    public static string[] SanitizeAndSplit(string rawInput)
    {
        string stripped = m_kNonAlphanumericRegex.Replace(rawInput, string.Empty).ToLowerInvariant();
        return stripped.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
