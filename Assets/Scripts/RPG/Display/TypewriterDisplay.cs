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

    [SerializeField, Tooltip("Play the SFX every N non-whitespace characters printed.")]
    private int m_CharactersPerSFX = 3;

    private bool m_bIsTypewriting;
    private Coroutine m_TypewriterCoroutine;
    private string m_PendingFullText;
    private Action m_PendingOnComplete;

    public bool IsTypewriting => m_bIsTypewriting;

    public void PlayTypewriter(string fullText, Action onComplete)
    {
        if (m_TypewriterCoroutine != null)
        {
            StopCoroutine(m_TypewriterCoroutine);
        }

        m_PendingFullText = fullText;
        m_PendingOnComplete = onComplete;
        m_TypewriterCoroutine = StartCoroutine(_TypewriterRoutine(fullText, onComplete));
    }

    /// <summary>
    /// Plasmalot: Immediately finishes the in-progress typewriter effect, showing the full text and firing the
    /// same onComplete callback that would have fired naturally. No-op if nothing is currently typewriting.
    /// </summary>
    public void SkipTypewriter()
    {
        if (!m_bIsTypewriting) return;

        StopCoroutine(m_TypewriterCoroutine);
        m_TypewriterCoroutine = null;
        m_bIsTypewriting = false;

        m_OutputText.text = m_PendingFullText;

        Action onComplete = m_PendingOnComplete;
        m_PendingFullText = null;
        m_PendingOnComplete = null;
        onComplete?.Invoke();
    }

    private IEnumerator _TypewriterRoutine(string fullText, Action onComplete)
    {
        m_bIsTypewriting = true;
        m_OutputText.text = string.Empty;

        float secondsPerCharacter = 1.0f / Mathf.Max(m_CharactersPerSecond, 0.01f);
        int charactersPerSFX = Mathf.Max(m_CharactersPerSFX, 1);
        int printedNonWhitespaceCount = 0;
        foreach (char character in fullText)
        {
            m_OutputText.text += character;

            if (!char.IsWhiteSpace(character))
            {
                printedNonWhitespaceCount++;
                if (m_DigitalPromptTextClip != null && printedNonWhitespaceCount % charactersPerSFX == 0)
                {
                    AudioManager.Instance.PlaySFXOneShot(m_DigitalPromptTextClip);
                }
            }

            yield return new WaitForSeconds(secondsPerCharacter);
        }

        m_bIsTypewriting = false;
        m_TypewriterCoroutine = null;
        m_PendingFullText = null;
        m_PendingOnComplete = null;
        onComplete?.Invoke();
    }
}
