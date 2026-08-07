using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Plasmalot: Drives the scripted Forest Layer 1 tutorial.
/// Lets the Player's first 2 prompts play out normally; the 3rd prompt is replaced entirely by a forced Fairy
/// line explaining how to open your eyes. The next 3 eye-open rolls are then forced to reveal (in order) nothing,
/// the Fairy alone, then the Fairy with her Fragment - with the eyes-closed line after each of the first three
/// overridden with the Fairy's own scripted dialogue (the third of those confirming the Fragment is now visible
/// and takeable). Once the Fragment is successfully taken, input stays locked and the game waits for a single
/// keypress before printing the Fairy's 5th line (venture to the Glade) and unlocking input again.
/// While a step is awaiting one specific action, every other submission is replaced with that step's own
/// fallback line via DialogueProcessor.ResponseGate, rather than falling through to generic/unrelated responses.
/// Each step's override is only armed once the previous one has actually been consumed (via
/// DialogueProcessor.OnEyesClosedResponseComplete), since EyeModeController/DialogueProcessor only hold one
/// pending override at a time - arming early would silently clobber whichever override hadn't been read yet.
/// Disables itself once the Layer has advanced past 1, so it never replays on a later visit to the Forest.
/// </summary>
public class ForestTutorialController : MonoBehaviour
{
    [SerializeField, Tooltip("Captures raw Player input; used here to count the first 2 free prompts.")]
    private KeyboardInputHandler m_InputHandler;

    [SerializeField, Tooltip("Same DialogueProcessor driving normal Forest dialogue; this controller arms its response/eyes-closed overrides and gate.")]
    private DialogueProcessor m_DialogueProcessor;

    [SerializeField, Tooltip("Same EyeModeController driving the Forest's Spot-the-Difference reveals; this controller forces its next 3 variation rolls.")]
    private EyeModeController m_EyeModeController;

    [SerializeField, Tooltip("Same TypewriterDisplay used for normal dialogue; used here to print the Fairy's 5th line directly, after the keypress beat.")]
    private TypewriterDisplay m_TypewriterDisplay;

    [SerializeField, Tooltip("Index into the Forest Layer 1 PromptResponses' Variations list for a reveal with no Fairy/Fragment visible (1st eye-open).")]
    private int m_NoFairyVariationIndex;

    [SerializeField, Tooltip("Index into the Forest Layer 1 PromptResponses' Variations list for a reveal with only the Fairy visible (2nd eye-open).")]
    private int m_FairyOnlyVariationIndex;

    [SerializeField, Tooltip("Index into the Forest Layer 1 PromptResponses' Variations list for a reveal with the Fairy and her Fragment visible (3rd eye-open).")]
    private int m_FairyAndFragmentVariationIndex;

    [SerializeField, TextArea(3, 8), Tooltip("Forced Fairy line #1, replacing the Player's 3rd prompt entirely. Should explain how to open your eyes.")]
    private string m_FairyPrompt1_ExplainEyeOpen = "\"Wait... you're not from around here, are you?\" A voice, close to your ear. \"Close your eyes tight, then try to OPEN them. Really try.\"";

    [SerializeField, TextArea(3, 8), Tooltip("Forced Fairy line #2, printed automatically when the Player closes their eyes after the 1st (empty) reveal. Should tell the Player to open their eyes again.")]
    private string m_FairyPrompt2_TryAgain = "\"Hah, not like that. Eyes closed, then OPEN them. I promise I'm here.\"";

    [SerializeField, TextArea(3, 8), Tooltip("Forced Fairy line #3, printed automatically when the Player closes their eyes after the 2nd (Fairy-only) reveal. Should tell the Player to take her Fragment.")]
    private string m_FairyPrompt3_TakeFragment = "\"There, see? Now - I'm carrying a Fragment you'll be needing. Open your eyes and TAKE it from me.\"";

    [SerializeField, TextArea(3, 8), Tooltip("Forced Fairy line #4, printed automatically when the Player closes their eyes after the 3rd (Fairy+Fragment) reveal. Should confirm the Fragment is now visible/takeable.")]
    private string m_FairyPrompt4_YouSawIt = "\"You saw the Fragment, right? Good. Now you should be able to TAKE it from me. It's impossible to interact with things you can't see...\"";

    [SerializeField, TextArea(3, 8), Tooltip("Forced Fairy line #5, printed after a keypress following the successful Fragment take. Should tell the Player to venture further, towards the Glade.")]
    private string m_FairyPrompt5_VentureToGlade = "\"Good. Now - venture further, past the treeline. The Glade awaits.\"";

    [SerializeField, Tooltip("SFX played the moment Fairy Prompt 1 actually starts printing.")]
    private AudioClip m_FairyPrompt1SFX;

    [SerializeField, Tooltip("SFX played the moment Fairy Prompt 2 actually starts printing.")]
    private AudioClip m_FairyPrompt2SFX;

    [SerializeField, Tooltip("SFX played the moment Fairy Prompt 3 actually starts printing.")]
    private AudioClip m_FairyPrompt3SFX;

    [SerializeField, Tooltip("SFX played the moment Fairy Prompt 4 actually starts printing.")]
    private AudioClip m_FairyPrompt4SFX;

    [SerializeField, Tooltip("SFX played the moment Fairy Prompt 5 actually starts printing.")]
    private AudioClip m_FairyPrompt5SFX;

    [SerializeField, TextArea(2, 5), Tooltip("Shown for any input other than an eye-open attempt while awaiting the 1st (empty) reveal.")]
    private string m_Fallback1_EyeOpen = "\"Not that. Just try to OPEN your eyes.\"";

    [SerializeField, TextArea(2, 5), Tooltip("Shown for any input other than an eye-open attempt while awaiting the 2nd (Fairy-only) reveal.")]
    private string m_Fallback2_EyeOpen = "\"Eyes closed, then OPEN them. Try again.\"";

    [SerializeField, TextArea(2, 5), Tooltip("Shown for any input other than an eye-open attempt while awaiting the 3rd (Fairy+Fragment) reveal - e.g. a take-fragment attempt before it's actually been seen.")]
    private string m_Fallback3_EyeOpen = "\"You can't take what you can't see. Open your eyes first.\"";

    [SerializeField, TextArea(2, 5), Tooltip("Shown for any input other than a take-fragment attempt, once the Fragment has actually been seen (after Prompt 4).")]
    private string m_Fallback4_TakeFragment = "\"It's right here. TAKE it.\"";

    [SerializeField, TextArea(2, 5), Tooltip("Shown for any input other than heading to the Glade once the Player is free to venture onward.")]
    private string m_Fallback5_Glade = "\"The Glade, remember? That way.\"";

    private GameEnums.eForestTutorialStep m_Step = GameEnums.eForestTutorialStep.FreePrompts;
    private int m_PromptCount;
    private bool m_bJustCompletedTakeFragment;

    private void Start()
    {
        if (GameProgressManager.Instance.GetCurrentLayer(GameEnums.eLevelID.Forest) != 1)
        {
            enabled = false;
            return;
        }

        // LevelContext.Instance is only guaranteed to be assigned by this point (its own Awake has definitely run
        // by Start, whereas this component's OnEnable can fire before LevelContext's Awake does).
        LevelContext.Instance.OnTransitionStarted += _OnTransitionStarted;
    }

    private void OnEnable()
    {
        m_InputHandler.OnInputSubmitted += _OnPromptSubmitted;
        m_DialogueProcessor.OnEyesClosedResponseComplete += _OnEyesClosedResponseComplete;
        m_DialogueProcessor.OnBeforeInputUnlock += _OnBeforeInputUnlock;
        m_DialogueProcessor.ResponseGate = _GateResponse;
    }

    private void OnDisable()
    {
        m_InputHandler.OnInputSubmitted -= _OnPromptSubmitted;
        m_DialogueProcessor.OnEyesClosedResponseComplete -= _OnEyesClosedResponseComplete;
        m_DialogueProcessor.OnBeforeInputUnlock -= _OnBeforeInputUnlock;
        m_DialogueProcessor.ResponseGate = null;
        if (LevelContext.Instance != null)
        {
            LevelContext.Instance.OnTransitionStarted -= _OnTransitionStarted;
        }
    }

    private void Update()
    {
        if (m_Step != GameEnums.eForestTutorialStep.AwaitingContinueKeypress || Keyboard.current == null) return;
        if (!Keyboard.current.anyKey.wasPressedThisFrame) return;

        m_Step = GameEnums.eForestTutorialStep.Done;

        // Suppressing the unlock after the take-fragment response also suppressed the buffer clear that would
        // normally have happened then, so the take-fragment text would otherwise still be sitting in the input line.
        m_InputHandler.ClearInput();

        AudioManager.Instance.PlaySFXOneShot(m_FairyPrompt5SFX);
        m_TypewriterDisplay.PlayTypewriter(m_FairyPrompt5_VentureToGlade, _OnFairyPrompt5Complete);
    }

    private void _OnPromptSubmitted(string rawInput)
    {
        if (m_Step != GameEnums.eForestTutorialStep.FreePrompts) return;

        m_PromptCount++;
        if (m_PromptCount < 2) return;

        // Plasmalot: The 3rd submission (about to happen) is replaced entirely by the Fairy's forced first line.
        m_Step = GameEnums.eForestTutorialStep.AwaitingEyeOpen1;
        m_DialogueProcessor.SetNextResponseOverride(m_FairyPrompt1_ExplainEyeOpen, m_FairyPrompt1SFX);

        // Safe to arm now: eyes are still closed and can't open until the Player acts on that forced line.
        m_EyeModeController.SetForcedVariationOverride(m_NoFairyVariationIndex);
        m_DialogueProcessor.SetNextEyesClosedResponse(m_FairyPrompt2_TryAgain, m_FairyPrompt2SFX);
    }

    /// <summary>Restricts input to whichever single action the current step is waiting on; everything else shows that step's own fallback.</summary>
    private string _GateResponse(bool bFoundMatch, bool bIsTransition, bool bIsEyeOpen)
    {
        switch (m_Step)
        {
            case GameEnums.eForestTutorialStep.AwaitingEyeOpen1:
                return bIsEyeOpen ? null : m_Fallback1_EyeOpen;

            case GameEnums.eForestTutorialStep.AwaitingEyeOpen2:
                return bIsEyeOpen ? null : m_Fallback2_EyeOpen;

            case GameEnums.eForestTutorialStep.AwaitingEyeOpen3:
                return bIsEyeOpen ? null : m_Fallback3_EyeOpen;

            case GameEnums.eForestTutorialStep.AwaitingTakeFragment:
                if (bFoundMatch && !bIsTransition && !bIsEyeOpen)
                {
                    m_bJustCompletedTakeFragment = true;
                    return null;
                }
                return m_Fallback4_TakeFragment;

            case GameEnums.eForestTutorialStep.AwaitingGladeTravel:
                return bIsTransition ? null : m_Fallback5_Glade;

            default:
                return null;
        }
    }

    private void _OnEyesClosedResponseComplete()
    {
        switch (m_Step)
        {
            case GameEnums.eForestTutorialStep.AwaitingEyeOpen1: // Close after eye-open #1 (nothing visible) just finished showing "try again".
                m_Step = GameEnums.eForestTutorialStep.AwaitingEyeOpen2;
                m_EyeModeController.SetForcedVariationOverride(m_FairyOnlyVariationIndex);
                m_DialogueProcessor.SetNextEyesClosedResponse(m_FairyPrompt3_TakeFragment, m_FairyPrompt3SFX);
                break;

            case GameEnums.eForestTutorialStep.AwaitingEyeOpen2: // Close after eye-open #2 (Fairy only) just finished showing "take my fragment".
                m_Step = GameEnums.eForestTutorialStep.AwaitingEyeOpen3;
                m_EyeModeController.SetForcedVariationOverride(m_FairyAndFragmentVariationIndex);
                m_DialogueProcessor.SetNextEyesClosedResponse(m_FairyPrompt4_YouSawIt, m_FairyPrompt4SFX);
                break;

            case GameEnums.eForestTutorialStep.AwaitingEyeOpen3: // Close after eye-open #3 (Fairy + Fragment) just finished confirming it's takeable.
                m_Step = GameEnums.eForestTutorialStep.AwaitingTakeFragment;
                break;
        }
    }

    private void _OnBeforeInputUnlock()
    {
        if (!m_bJustCompletedTakeFragment) return;

        m_bJustCompletedTakeFragment = false;
        m_DialogueProcessor.SuppressNextUnlock();
        m_Step = GameEnums.eForestTutorialStep.AwaitingContinueKeypress;
    }

    private void _OnFairyPrompt5Complete()
    {
        m_Step = GameEnums.eForestTutorialStep.AwaitingGladeTravel;
        m_InputHandler.UnlockInput();
    }

    private void _OnTransitionStarted(GameEnums.eLevelID targetLevelID)
    {
        if (targetLevelID != GameEnums.eLevelID.Glade) return;

        GameProgressManager.Instance.AdvanceLayer(GameEnums.eLevelID.Forest, 2);
    }
}
