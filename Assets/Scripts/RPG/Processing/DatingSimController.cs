using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Plasmalot: Owns the "Light the Unicorn" Dating Sim mini-game triggered from Glade Layer 1.
/// Toggles its own canvas over the dialogue UI, asks a run of randomly-drawn DatingSimQuestionSOs,
/// and tracks a correct/incorrect streak that drives both the Unicorn's sprite tier
/// and the side-of-screen heart/anger effects.
/// Winning at 8 correct advances the Glade Layer; losing at 3 incorrect kicks the Player back out and
/// marks the date as attempted, so PromptResponses can gate a different "already attempted" line onto a retry.
/// Both the initial launch and each answer's result are gated behind an "any key" beat (same pattern as
/// ForestTutorialController's post-Fragment continue prompt) rather than advancing the instant text finishes typing.
/// </summary>
public class DatingSimController : MonoBehaviour
{
    [SerializeField, Tooltip("Root GameObject of the Dating Sim canvas. Starts inactive.")]
    private GameObject m_DatingSimCanvasRoot;

    [SerializeField, Tooltip("Child RectTransform to scale in.")]
    private RectTransform m_PopInRoot;

    [SerializeField, Tooltip("Root GameObject of the dialogue UI, hidden while the Dating Sim is active.")]
    private GameObject m_DialogueUIRoot;

    [SerializeField, Tooltip("Same DialogueProcessor driving normal Level dialogue; used to re-play the current Layer's Intro Response once the Dating Sim closes.")]
    private DialogueProcessor m_DialogueProcessor;

    [SerializeField, Tooltip("Input buffer that gets cleared as the date starts so the prompt that triggered it isn't left sitting in the input line.")]
    private KeyboardInputHandler m_InputHandler;

    [SerializeField, Tooltip("Types the current question/result text to the Dating Sim's own text box.")]
    private TypewriterDisplay m_DatingSimTypewriter;

    [SerializeField, Tooltip("Root GameObject of the question/result text box.")]
    private GameObject m_QuestionTextRoot;

    [SerializeField, Tooltip("Image the Unicorn sprite collections are cycled through.")]
    private DatingSimSpriteCycler m_SpriteCycler;

    [SerializeField, Tooltip("Owns the heart/anger side-of-screen decoration effects.")]
    private DatingSimSideEffectsController m_SideEffectsController;

    [SerializeField, Tooltip("Root GameObject of the four answer option texts; hidden while a question is still being typed out.")]
    private GameObject m_AnswerOptionsRoot;

    [SerializeField, Tooltip("The four answer option text components, one per screen row in top-to-bottom order.")]
    private TMP_Text[] m_AnswerOptionTexts;

    [SerializeField, Tooltip("Pointer moved alongside the currently-selected answer option's row. A position/visibility indicator only - 1-bit jam rules only allow two colors on screen, so selection can't be shown with a highlight color.")]
    private RectTransform m_SelectionPointer;

    [SerializeField, Tooltip("Base collection (Front/Back/Closed Eyes) shown at 0 correct answers.")]
    private Sprite[] m_NormalCollection;

    [SerializeField, Tooltip("Blush collection shown at 1-2 correct answers.")]
    private Sprite[] m_BlushCollection;

    [SerializeField, Tooltip("Heavy Blush collection shown at 3-4 correct answers.")]
    private Sprite[] m_HeavyBlushCollection;

    [SerializeField, Tooltip("Front-Facing Normal collection shown at 5 correct answers.")]
    private Sprite[] m_FrontFacingNormalCollection;

    [SerializeField, Tooltip("Front-Facing Blush collection shown at 6-7 correct answers.")]
    private Sprite[] m_FrontFacingBlushCollection;

    [SerializeField, Tooltip("Front-Facing Heavy Blush collection shown at 8+ correct answers.")]
    private Sprite[] m_FrontFacingHeavyBlushCollection;

    [SerializeField, Tooltip("Hyperrealistic horse collection shown after an incorrect answer, until the next correct answer.")]
    private Sprite[] m_HorseCollection;

    [SerializeField, Tooltip("Pool of questions the Dating Sim draws from at random, without repeats, each attempt.")]
    private List<DatingSimQuestionSO> m_QuestionPool;

    [SerializeField, Min(1), Tooltip("Number of correct answers needed to win the date.")]
    private int m_CorrectAnswersToWin = 8;

    [SerializeField, Min(1), Tooltip("Number of incorrect answers that reject the date.")]
    private int m_IncorrectAnswersToLose = 3;

    [SerializeField, TextArea(2, 4), Tooltip("Text shown after a correct answer, before the next question.")]
    private string m_CorrectAnswerResponseText = "The Unicorn's eyes light up!";

    [SerializeField, TextArea(2, 4), Tooltip("Text shown after an incorrect answer, before the next question.")]
    private string m_IncorrectAnswerResponseText = "The Unicorn recoils in disgust!";

    [SerializeField, TextArea(2, 4), Tooltip("Intermediate text shown once, right after the correct answer that first pushes the sprite tier from Heavy Blush to Front-Facing Normal (5th correct answer).")]
    private string m_FrontFacingTransitionText = "It looks like you're successfully endearing yourself to Light. It looks like she's more interested in what you have to say.";

    [SerializeField, TextArea(2, 4), Tooltip("Text shown once the date is won.")]
    private string m_SuccessResponseText = "Light nuzzles you affectionately. The date was a success!";

    [SerializeField, TextArea(2, 4), Tooltip("Text shown once the date is rejected.")]
    private string m_FailureResponseText = "Light rears back, insulted, and storms off. The date is over.";

    [SerializeField, Min(1), Tooltip("The Glade Layer advanced to once the date is won.")]
    private int m_LayerToAdvanceTo = 2;

    [SerializeField, Tooltip("Marker GameObject enabled once a date has been attempted, so a retry can be gated onto a different PromptResponses Entry.")]
    private GameObject m_HasAttemptedDateMarker;

    [SerializeField, Tooltip("SFX played when the Dating Sim starts.")]
    private AudioClip m_DateStartSFXClip;

    [SerializeField, Tooltip("SFX played on a correct answer.")]
    private AudioClip m_CorrectAnswerSFXClip;

    [SerializeField, Tooltip("SFX played on an incorrect answer.")]
    private AudioClip m_IncorrectAnswerSFXClip;

    [SerializeField, Tooltip("SFX played once the date is won.")]
    private AudioClip m_DateWonSFXClip;

    [SerializeField, Tooltip("SFX played once the date is rejected.")]
    private AudioClip m_DateLostSFXClip;

    [SerializeField, Tooltip("Seconds for the canvas to grow from scale 0 to its full size when the Dating Sim launches.")]
    private float m_CanvasPopInDuration = 0.35f;

    [SerializeField, Tooltip("Scale the canvas briefly dips to right after reaching full size, before settling back to 1 - gives the pop-in a bouncy feel.")]
    private float m_CanvasBounceDipScale = 0.92f;

    [SerializeField, Tooltip("Seconds for each half (dip, then settle) of the post-pop-in bounce.")]
    private float m_CanvasBounceDuration = 0.12f;

    [SerializeField, Tooltip("Seconds for the canvas to shrink from full size back to scale 0 once the date is over, before it's actually disabled.")]
    private float m_CanvasPopOutDuration = 0.25f;

    private GameEnums.eDatingSimState m_State = GameEnums.eDatingSimState.Inactive;
    private int m_CorrectCount;
    private int m_IncorrectCount;
    private bool m_bWonDate;
    private bool m_bPendingFrontFacingTransition;
    private int m_SelectedIndex;
    private List<DatingSimQuestionSO> m_DrawPool;
    private DatingSimQuestionSO m_CurrentQuestion;
    private readonly int[] m_ShuffledAnswerIndices = new int[4];

    /// <summary>
    /// Plasmalot: Called once the PromptResponses response that launches the date finishes typing. Doesn't begin
    /// the date itself - input is already locked at this point (DialogueProcessor returns before unlocking), so
    /// this just waits for a single keypress before actually swapping to the Dating Sim canvas.
    /// </summary>
    public void StartDatingSim()
    {
        m_State = GameEnums.eDatingSimState.AwaitingLaunchKeypress;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        switch (m_State)
        {
            case GameEnums.eDatingSimState.AwaitingLaunchKeypress:
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    _BeginDatingSim();
                }
                break;

            case GameEnums.eDatingSimState.AwaitingAnswerSelection:
                _HandleAnswerSelectionInput();
                break;

            case GameEnums.eDatingSimState.AwaitingResultContinueKeypress:
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    _ProceedAfterResult();
                }
                break;

            case GameEnums.eDatingSimState.AwaitingFrontFacingTransitionKeypress:
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    _ContinueAfterFrontFacingTransition();
                }
                break;

            case GameEnums.eDatingSimState.AwaitingFinalContinueKeypress:
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    _FinishDatingSim();
                }
                break;
        }
    }

    private void _BeginDatingSim()
    {
        // Set here so the Update() switch stops matching
        // AwaitingLaunchKeypress immediately - otherwise a keypress during the pop-in animation below would
        // re-trigger this same method before the first question is ever asked.
        m_State = GameEnums.eDatingSimState.Typing;

        m_CorrectCount = 0;
        m_IncorrectCount = 0;
        m_DrawPool = new List<DatingSimQuestionSO>(m_QuestionPool);
        m_InputHandler.ClearInput();

        m_DialogueUIRoot.SetActive(false);
        m_DatingSimCanvasRoot.SetActive(true);
        m_QuestionTextRoot.SetActive(false);
        m_SideEffectsController.StopAll();

        AudioManager.Instance.PlayMusic(m_DateStartSFXClip, true, 1f);

        _ApplySpriteTier(_TierForCorrectCount(0));
        _PlayCanvasPopInAnimation(_AskNextQuestion);
    }

    private void _PlayCanvasPopInAnimation(TweenCallback onComplete)
    {
        m_PopInRoot.localScale = Vector3.zero;

        Sequence popInSequence = DOTween.Sequence();
        popInSequence.Append(m_PopInRoot.DOScale(1f, m_CanvasPopInDuration).SetEase(Ease.OutSine));
        popInSequence.Append(m_PopInRoot.DOScale(m_CanvasBounceDipScale, m_CanvasBounceDuration).SetEase(Ease.InOutSine));
        popInSequence.Append(m_PopInRoot.DOScale(1f, m_CanvasBounceDuration).SetEase(Ease.InOutSine));
        popInSequence.OnComplete(onComplete);
    }

    private void _HandleAnswerSelectionInput()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            m_SelectedIndex = (m_SelectedIndex - 1 + m_AnswerOptionTexts.Length) % m_AnswerOptionTexts.Length;
            _RefreshAnswerDisplay();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            m_SelectedIndex = (m_SelectedIndex + 1) % m_AnswerOptionTexts.Length;
            _RefreshAnswerDisplay();
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            _SubmitAnswer();
        }
    }

    private GameEnums.eDatingSimSpriteTier _TierForCorrectCount(int correctCount)
    {
        if (correctCount >= 8) return GameEnums.eDatingSimSpriteTier.FrontFacingHeavyBlush;
        if (correctCount >= 6) return GameEnums.eDatingSimSpriteTier.FrontFacingBlush;
        if (correctCount >= 5) return GameEnums.eDatingSimSpriteTier.FrontFacingNormal;
        if (correctCount >= 3) return GameEnums.eDatingSimSpriteTier.HeavyBlush;
        if (correctCount >= 1) return GameEnums.eDatingSimSpriteTier.Blush;
        return GameEnums.eDatingSimSpriteTier.Normal;
    }

    private void _ApplySpriteTier(GameEnums.eDatingSimSpriteTier tier, bool bRandomStartFrame = false)
    {
        Sprite[] collection = tier switch
        {
            GameEnums.eDatingSimSpriteTier.Blush => m_BlushCollection,
            GameEnums.eDatingSimSpriteTier.HeavyBlush => m_HeavyBlushCollection,
            GameEnums.eDatingSimSpriteTier.FrontFacingNormal => m_FrontFacingNormalCollection,
            GameEnums.eDatingSimSpriteTier.FrontFacingBlush => m_FrontFacingBlushCollection,
            GameEnums.eDatingSimSpriteTier.FrontFacingHeavyBlush => m_FrontFacingHeavyBlushCollection,
            GameEnums.eDatingSimSpriteTier.Horse => m_HorseCollection,
            _ => m_NormalCollection,
        };
        m_SpriteCycler.SetCollection(collection, bRandomStartFrame);
    }

    private void _AskNextQuestion()
    {
        int index = Random.Range(0, m_DrawPool.Count);
        m_CurrentQuestion = m_DrawPool[index];
        m_DrawPool.RemoveAt(index);
        _ShuffleAnswerIndices();

        m_State = GameEnums.eDatingSimState.Typing;
        m_SelectedIndex = 0;
        m_AnswerOptionsRoot.SetActive(false);
        m_QuestionTextRoot.SetActive(true);
        m_SpriteCycler.StartCycling();

        m_DatingSimTypewriter.PlayTypewriter(m_CurrentQuestion.QuestionText, _OnQuestionTyped);
    }

    private void _ShuffleAnswerIndices()
    {
        for (int i = 0; i < m_ShuffledAnswerIndices.Length; i++)
        {
            m_ShuffledAnswerIndices[i] = i;
        }

        for (int i = m_ShuffledAnswerIndices.Length - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (m_ShuffledAnswerIndices[i], m_ShuffledAnswerIndices[swapIndex]) = (m_ShuffledAnswerIndices[swapIndex], m_ShuffledAnswerIndices[i]);
        }
    }

    private void _OnQuestionTyped()
    {
        _RefreshAnswerDisplay();
        m_AnswerOptionsRoot.SetActive(true);
        m_State = GameEnums.eDatingSimState.AwaitingAnswerSelection;
    }

    private void _RefreshAnswerDisplay()
    {
        IReadOnlyList<string> options = m_CurrentQuestion.AnswerOptions;
        for (int i = 0; i < m_AnswerOptionTexts.Length; i++)
        {
            m_AnswerOptionTexts[i].text = options[m_ShuffledAnswerIndices[i]];
        }

        Vector2 pointerPosition = m_SelectionPointer.anchoredPosition;
        pointerPosition.y = m_AnswerOptionTexts[m_SelectedIndex].rectTransform.anchoredPosition.y;
        m_SelectionPointer.anchoredPosition = pointerPosition;
    }

    private void _SubmitAnswer()
    {
        m_State = GameEnums.eDatingSimState.Typing;
        m_SpriteCycler.StopCycling();

        bool bWasCorrect = m_ShuffledAnswerIndices[m_SelectedIndex] == m_CurrentQuestion.CorrectAnswerIndex;
        string resultText;
        if (bWasCorrect)
        {
            m_CorrectCount++;

            // The Heavy Blush -> Front-Facing Normal changeover gets its own intermediate beat instead of swapping sprites immediately like every other tier change.
            m_bPendingFrontFacingTransition = m_CorrectCount == 5;
            if (!m_bPendingFrontFacingTransition)
            {
                _ApplySpriteTier(_TierForCorrectCount(m_CorrectCount), bRandomStartFrame: true);
            }

            m_SideEffectsController.PlayHearts();
            AudioManager.Instance.PlaySFXOneShot(m_CorrectAnswerSFXClip);
            resultText = m_CorrectAnswerResponseText;
        }
        else
        {
            m_IncorrectCount++;
            _ApplySpriteTier(GameEnums.eDatingSimSpriteTier.Horse);
            m_SideEffectsController.PlayAnger();
            AudioManager.Instance.PlaySFXOneShot(m_IncorrectAnswerSFXClip);
            resultText = m_IncorrectAnswerResponseText;
        }

        m_DatingSimTypewriter.PlayTypewriter(resultText, _OnResultTyped);
    }

    private void _OnResultTyped()
    {
        m_State = GameEnums.eDatingSimState.AwaitingResultContinueKeypress;
    }

    private void _ProceedAfterResult()
    {
        if (m_CorrectCount >= m_CorrectAnswersToWin)
        {
            _CompleteDatingSimSuccess();
            return;
        }

        if (m_IncorrectCount >= m_IncorrectAnswersToLose)
        {
            _CompleteDatingSimFailure();
            return;
        }

        if (m_bPendingFrontFacingTransition)
        {
            m_bPendingFrontFacingTransition = false;
            _ShowFrontFacingTransition();
            return;
        }

        _AskNextQuestion();
    }

    private void _ShowFrontFacingTransition()
    {
        m_State = GameEnums.eDatingSimState.Typing;
        m_DatingSimTypewriter.PlayTypewriter(m_FrontFacingTransitionText, _OnFrontFacingTransitionTyped);
    }

    private void _OnFrontFacingTransitionTyped()
    {
        m_State = GameEnums.eDatingSimState.AwaitingFrontFacingTransitionKeypress;
    }

    private void _ContinueAfterFrontFacingTransition()
    {
        _ApplySpriteTier(GameEnums.eDatingSimSpriteTier.FrontFacingNormal, bRandomStartFrame: true);
        _AskNextQuestion();
    }

    private void _CompleteDatingSimSuccess()
    {
        m_State = GameEnums.eDatingSimState.Typing;
        m_bWonDate = true;
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlaySFXOneShot(m_DateWonSFXClip);

        m_DatingSimTypewriter.PlayTypewriter(m_SuccessResponseText, _OnFinalMessageTyped);
    }

    private void _CompleteDatingSimFailure()
    {
        m_State = GameEnums.eDatingSimState.Typing;
        m_bWonDate = false;
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlaySFXOneShot(m_DateLostSFXClip);

        if (m_HasAttemptedDateMarker != null)
        {
            m_HasAttemptedDateMarker.SetActive(true);
        }

        m_DatingSimTypewriter.PlayTypewriter(m_FailureResponseText, _OnFinalMessageTyped);
    }

    private void _OnFinalMessageTyped()
    {
        m_State = GameEnums.eDatingSimState.AwaitingFinalContinueKeypress;
    }

    private void _FinishDatingSim()
    {
        if (m_bWonDate)
        {
            GameProgressManager.Instance.AdvanceLayer(LevelContext.Instance.CurrentLevelID, m_LayerToAdvanceTo);
        }

        m_State = GameEnums.eDatingSimState.Inactive;

        _PlayCanvasPopOutAnimation(_EndDatingSim);
    }

    private void _PlayCanvasPopOutAnimation(TweenCallback onComplete)
    {
        Sequence popOutSequence = DOTween.Sequence();
        popOutSequence.Append(m_PopInRoot.DOScale(0f, m_CanvasPopOutDuration).SetEase(Ease.InSine));
        popOutSequence.OnComplete(onComplete);
    }

    private void _EndDatingSim()
    {
        // Plasmalot: Reset the Dating Sim side icons now that the Canvas has scaled down.
        m_SideEffectsController.StopAll();

        // Plasmalot: Hide the answer text here so a retried date's pop-in doesn't briefly show the previous attempt's answer options underneath it.
        m_AnswerOptionsRoot.SetActive(false);
        
        m_DatingSimCanvasRoot.SetActive(false);
        m_DialogueUIRoot.SetActive(true);
        m_DialogueProcessor.PlayCurrentLayerIntro();
    }
}
