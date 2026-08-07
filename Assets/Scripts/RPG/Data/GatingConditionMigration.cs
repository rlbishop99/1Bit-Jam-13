using System.Collections.Generic;

/// <summary>
/// Plasmalot: One-time upgrade path from an Entry's old single Gating Object/Presence Requirement fields to the
/// Gating Conditions list, run from PromptResponses.Entry's OnAfterDeserialize. Mirrors KeywordGroupMigration's
/// wrap-legacy-value-in-a-list-once approach so pre-existing data keeps gating identically until a designer adds
/// further conditions by hand.
/// </summary>
public static class GatingConditionMigration
{
    public static void MigrateLegacyGatingObject(UnityEngine.GameObject legacyGatingObject, GameEnums.ePresenceRequirement legacyPresenceRequirement, ref List<PromptResponses.GatingCondition> gatingConditions)
    {
        if ((gatingConditions != null && gatingConditions.Count > 0) || legacyGatingObject == null) return;

        gatingConditions = new List<PromptResponses.GatingCondition> { new PromptResponses.GatingCondition(legacyGatingObject, legacyPresenceRequirement) };
    }
}
