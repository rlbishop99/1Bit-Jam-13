using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: Global "Quit" command - prompts the Player to confirm returning to the Title Screen, then arms
/// DialogueProcessor to interpret the Player's next submission as the yes/no answer.
/// </summary>
public class QuitGlobalCommand : GlobalCommand
{
    private static readonly List<IReadOnlyList<string>> m_KeywordSets = new List<IReadOnlyList<string>>
    {
        new List<string> { "quit" },
    };

    public override string DisplayName => "quit";
    public override IReadOnlyList<IReadOnlyList<string>> KeywordSets => m_KeywordSets;

    public override string GetResponse(GlobalCommandContext context)
    {
        context.DialogueProcessor.RequestQuitConfirmation();
        return "Return to the Title Screen? Your progress will not be saved and you'll have to start from scratch.";
    }
}
