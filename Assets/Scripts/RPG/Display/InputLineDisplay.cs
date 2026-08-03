using TMPro;
using UnityEngine;

/// <summary>
/// Plasmalot: Mirrors the player's current typed input in the UI.
/// </summary>
public class InputLineDisplay : MonoBehaviour
{
    [SerializeField, Tooltip("Handles raw keyboard capture/submission.")]
    private KeyboardInputHandler m_InputHandler;

    [SerializeField, Tooltip("TMP text component that mirrors the player's current typed input.")]
    private TMP_Text m_InputLineText;

    private void OnEnable()
    {
        m_InputHandler.OnInputTextChanged += _HandleInputTextChanged;
    }

    private void OnDisable()
    {
        m_InputHandler.OnInputTextChanged -= _HandleInputTextChanged;
    }

    private void _HandleInputTextChanged(string newText)
    {
        m_InputLineText.text = newText;
    }
}
