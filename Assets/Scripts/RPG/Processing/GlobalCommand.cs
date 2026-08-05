using System.Collections.Generic;

/// <summary>
/// Plasmalot: Base class for a baked-in Prompt -> Response mapping that DialogueProcessor evaluates
/// alongside the active Level/Variation's Entries on every submission, regardless of Level, Layer, or
/// Variation. Add a new subclass and register it in DialogueProcessor's GlobalCommands list to add one.
/// </summary>
public abstract class GlobalCommand
{
    /// <summary>Name shown for this command by the Help command. Not itself matched against input.</summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Every independent set of keywords that can trigger this command.
    /// Each set is scored on its own via IntentScorer, same as a PromptResponses Entry's Keywords
    /// so a multi-word set requires every word in that set to be present, but any one set matching is enough to trigger the command.
    /// </summary>
    public abstract IReadOnlyList<IReadOnlyList<string>> KeywordSets { get; }

    public virtual float RequiredIntentThreshold => 100.0f;

    public abstract string GetResponse(GlobalCommandContext context);
}
