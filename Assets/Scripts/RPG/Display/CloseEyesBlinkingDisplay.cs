using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plasmalot: Toggles the visibility of a cursor Text component on/off to create a blinking effect.
/// The cursor is only visible when the Player's input is unlocked. When the Player's input is locked, the cursor is hidden.
/// </summary>
public class CloseEyesBlinkingDisplay : MonoBehaviour
{

    [SerializeField, Tooltip("The TMP text component (e.g. a '|' character) whose visibility is toggled to create the blink.")]
    private Image m_CloseEyesDisplay;

    [SerializeField, Tooltip("Seconds between each cursor visibility toggle.")]
    private float m_BlinkIntervalSeconds = 0.5f;
    private float m_BlinkTimer;

    private void Update()
    {
        m_BlinkTimer += Time.deltaTime;
        if (m_BlinkTimer >= m_BlinkIntervalSeconds)
        {
            m_BlinkTimer = 0.0f;
            m_CloseEyesDisplay.enabled = !m_CloseEyesDisplay.enabled;
        }
    }
}