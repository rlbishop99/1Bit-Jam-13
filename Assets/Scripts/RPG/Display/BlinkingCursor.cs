using TMPro;
using UnityEngine;

/// <summary>
/// Plasmalot: Toggles the visibility of a cursor Text component on/off to create a blinking effect.
/// The cursor is only visible when the Player's input is unlocked. When the Player's input is locked, the cursor is hidden.
/// </summary>
public class BlinkingCursor : MonoBehaviour
{
    [SerializeField, Tooltip("Handles raw keyboard capture/submission.")]
    private KeyboardInputHandler m_InputHandler;

    [SerializeField, Tooltip("The TMP text component (e.g. a '|' character) whose visibility is toggled to create the blink.")]
    private TMP_Text m_CursorObject;

    [SerializeField, Tooltip("Seconds between each cursor visibility toggle.")]
    private float m_BlinkIntervalSeconds = 0.5f;

    private bool m_bIsBlinkingEnabled;
    private float m_BlinkTimer;

    private void OnEnable()
    {
        m_InputHandler.OnInputLocked += _HandleInputLocked;
        m_InputHandler.OnInputUnlocked += _HandleInputUnlocked;
        _HandleInputUnlocked();
    }

    private void OnDisable()
    {
        m_InputHandler.OnInputLocked -= _HandleInputLocked;
        m_InputHandler.OnInputUnlocked -= _HandleInputUnlocked;
    }

    private void Update()
    {
        if (!m_bIsBlinkingEnabled) return;

        m_BlinkTimer += Time.deltaTime;
        if (m_BlinkTimer >= m_BlinkIntervalSeconds)
        {
            m_BlinkTimer = 0.0f;
            m_CursorObject.enabled = !m_CursorObject.enabled;
        }
    }

    private void _HandleInputLocked()
    {
        m_bIsBlinkingEnabled = false;
        m_CursorObject.enabled = false;
    }

    private void _HandleInputUnlocked()
    {
        m_bIsBlinkingEnabled = true;
        m_BlinkTimer = 0.0f;
        m_CursorObject.enabled = true;
    }
}
