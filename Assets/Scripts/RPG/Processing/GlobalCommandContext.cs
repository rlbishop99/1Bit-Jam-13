using System.Collections.Generic;

/// <summary>
/// Plasmalot: Snapshot of the current submission's Level/Layer state, passed to GlobalCommand.GetResponse so commands can tailor their response to what's actually active right now.
/// </summary>
public readonly struct GlobalCommandContext
{
    public readonly IReadOnlyList<PromptResponses> ActiveSources;
    public readonly int CurrentLayer;

    public GlobalCommandContext(IReadOnlyList<PromptResponses> activeSources, int currentLayer)
    {
        ActiveSources = activeSources;
        CurrentLayer = currentLayer;
    }
}
