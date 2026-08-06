using System.Collections.Generic;

/// <summary>
/// Plasmalot: Shared one-time upgrade path from the old flat Keywords list to Keyword Groups, run from
/// each PromptResponses entry struct's OnAfterDeserialize. Wraps each legacy word in its own single-synonym
/// group so pre-existing data keeps scoring identically until a designer merges words into real synonym groups.
/// </summary>
public static class KeywordGroupMigration
{
    public static void MigrateLegacyKeywords(List<string> legacyKeywords, ref List<KeywordGroup> keywordGroups)
    {
        if ((keywordGroups != null && keywordGroups.Count > 0) || legacyKeywords == null || legacyKeywords.Count == 0) return;

        keywordGroups = new List<KeywordGroup>(legacyKeywords.Count);
        foreach (string keyword in legacyKeywords)
        {
            keywordGroups.Add(new KeywordGroup(new List<string> { keyword }));
        }
    }
}
