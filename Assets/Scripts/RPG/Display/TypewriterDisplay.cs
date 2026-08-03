using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Plasmalot: Handles the typewriter effect for displaying text in the UI.
/// </summary>
public class TypewriterDisplay : MonoBehaviour
{
    [SerializeField, Tooltip("TMP text component that displays the printed text.")]
    private TMP_Text m_OutputText;

    [SerializeField, Tooltip("Characters printed per second during the typewriter effect.")]
    private float m_CharactersPerSecond = 30.0f;

    [SerializeField, Tooltip("SFX played for each non-whitespace character printed during the typewriter effect.")]
    private AudioClip m_DigitalPromptTextClip;

    private bool m_bIsTypewriting;
    private Coroutine m_TypewriterCoroutine;

    public bool IsTypewriting => m_bIsTypewriting;

    public void PlayTypewriter(string fullText, Action onComplete)
    {
        if (m_TypewriterCoroutine != null)
        {
            StopCoroutine(m_TypewriterCoroutine);
        }

        m_TypewriterCoroutine = StartCoroutine(_TypewriterRoutine(fullText, onComplete));
    }

    private IEnumerator _TypewriterRoutine(string fullText, Action onComplete)
    {
        m_bIsTypewriting = true;
        m_OutputText.text = string.Empty;

        float secondsPerCharacter = 1.0f / Mathf.Max(m_CharactersPerSecond, 0.01f);
        foreach (char character in fullText)
        {
            m_OutputText.text += character;

            if (!char.IsWhiteSpace(character) && m_DigitalPromptTextClip != null)
            {
                AudioManager.Instance.PlaySFXOneShot(m_DigitalPromptTextClip);
            }

            yield return new WaitForSeconds(secondsPerCharacter);
        }

        m_bIsTypewriting = false;
        m_TypewriterCoroutine = null;
        onComplete?.Invoke();
    }
}
