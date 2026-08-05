using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Plasmalot: Handles keyboard input for the Player's dialogue input. 
/// This is a separate component from DialogueProcessor so that the Input System can be used to capture text input without interfering with other Input System actions.
/// </summary>
public class KeyboardInputHandler : MonoBehaviour
{
    [SerializeField, Tooltip("SFX pool; one is randomly selected and played on every character add/remove.")]
    private AudioClip[] m_TypingSFXClips;

    [SerializeField, Tooltip("SFX played when the input string is submitted.")]
    private AudioClip m_SubmitSFXClip;

    [SerializeField, Tooltip("Seconds Backspace must be held before repeat-deletion kicks in.")]
    private float m_BackspaceRepeatInitialDelay = 0.4f;

    [SerializeField, Tooltip("Seconds between each character removed while Backspace is held past the initial delay.")]
    private float m_BackspaceRepeatInterval = 0.05f;

    private StringBuilder m_RawInputBuffer = new StringBuilder();
    private bool m_bIsInputLocked;
    private float m_NextBackspaceRepeatTime;

    public string RawInput => m_RawInputBuffer.ToString();
    public bool IsInputLocked => m_bIsInputLocked;

    public event Action<string> OnInputTextChanged;
    public event Action<string> OnInputSubmitted;
    public event Action OnInputLocked;
    public event Action OnInputUnlocked;

    private void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += _HandleTextInput;
        }
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= _HandleTextInput;
        }
    }

    private void Update()
    {
        if (m_bIsInputLocked || Keyboard.current == null) return;

        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            _RemoveLastCharacter();
            m_NextBackspaceRepeatTime = Time.unscaledTime + m_BackspaceRepeatInitialDelay;
        }
        else if (Keyboard.current.backspaceKey.isPressed && Time.unscaledTime >= m_NextBackspaceRepeatTime)
        {
            _RemoveLastCharacter();
            m_NextBackspaceRepeatTime = Time.unscaledTime + m_BackspaceRepeatInterval;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            _SubmitInput();
        }
    }

    public void LockInput()
    {
        m_bIsInputLocked = true;
        OnInputLocked?.Invoke();
    }

    public void UnlockInput()
    {
        m_RawInputBuffer.Clear();
        m_bIsInputLocked = false;
        OnInputUnlocked?.Invoke();
        OnInputTextChanged?.Invoke(RawInput);
    }

    public void ClearInput()
    {
        m_RawInputBuffer.Clear();
        OnInputTextChanged?.Invoke(RawInput);
    }

    private void _HandleTextInput(char inputChar)
    {
        if (m_bIsInputLocked || char.IsControl(inputChar)) return;

        m_RawInputBuffer.Append(inputChar);
        _PlayRandomSfx(m_TypingSFXClips);
        OnInputTextChanged?.Invoke(RawInput);
    }

    private void _RemoveLastCharacter()
    {
        if (m_RawInputBuffer.Length == 0) return;

        m_RawInputBuffer.Length -= 1;
        _PlayRandomSfx(m_TypingSFXClips);
        OnInputTextChanged?.Invoke(RawInput);
    }

    private void _SubmitInput()
    {
        if (m_RawInputBuffer.Length == 0) return;

        string submittedInput = RawInput;
        _PlaySfx(m_SubmitSFXClip);
        LockInput();
        OnInputSubmitted?.Invoke(submittedInput);
    }

    private void _PlaySfx(AudioClip clip)
    {
        if (clip == null) return;

        AudioManager.Instance.PlaySFXOneShot(clip);
    }

    private void _PlayRandomSfx(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        _PlaySfx(clips[UnityEngine.Random.Range(0, clips.Length)]);
    }
}
