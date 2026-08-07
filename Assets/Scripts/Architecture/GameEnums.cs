/// <summary>
/// Plasmalot: Centralized static container for all game enums, organized by region for clarity.
/// </summary>
public static class GameEnums
{
    #region Level Enums
    public enum eLevelID
    {
        Forest = 0,
        Glade = 1,
        Bridge = 2,
        Cave = 3,
        Cemetery = 4,
        WizardTower = 5,
        FairyHouse = 6,
    }

    public enum eVariationID
    {
        Default = 0,
        Variation1 = 1,
        Variation2 = 2,
    }
    #endregion

    #region PromptResponses Enums
    public enum ePresenceRequirement
    {
        MustBePresent,
        MustBeAbsent,
    }
    #endregion

    #region Dating Sim Enums
    public enum eDatingSimState
    {
        Inactive,
        AwaitingLaunchKeypress,
        Typing,
        AwaitingAnswerSelection,
        AwaitingResultContinueKeypress,
        AwaitingFrontFacingTransitionKeypress,
        AwaitingFinalContinueKeypress,
    }

    public enum eDatingSimSpriteTier
    {
        Normal,
        Blush,
        HeavyBlush,
        FrontFacingNormal,
        FrontFacingBlush,
        FrontFacingHeavyBlush,
        Horse,
    }

    public enum eDatingSimSideEffect
    {
        None,
        Hearts,
        Anger,
    }
    #endregion

    #region Forest Tutorial Enums
    public enum eForestTutorialStep
    {
        FreePrompts,
        AwaitingEyeOpen1,
        AwaitingEyeOpen2,
        AwaitingEyeOpen3,
        AwaitingTakeFragment,
        AwaitingContinueKeypress,
        AwaitingGladeTravel,
        Done,
    }
    #endregion
}
