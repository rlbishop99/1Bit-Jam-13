using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Plasmalot: Drives the Intro Scene's scripted 5 Ws exposition. Prints each Prompt in order via TypewriterDisplay,
/// playing that Prompt's SFX the moment it starts printing, and waits for any key to advance to the next one once
/// printing has finished. Once the last Prompt has finished printing, the next keypress loads the Forest scene instead.
/// The ----- UI ----- prefab's InputPanel expects a KeyboardInputHandler this Scene doesn't have, so it's deactivated in Awake.
/// </summary>
public class IntroController : MonoBehaviour
{
    [Serializable]
    private struct Prompt
    {
        [SerializeField, TextArea(2, 5), Tooltip("The line printed for this Prompt.")]
        private string m_Text;

        [SerializeField, Tooltip("SFX played the moment this Prompt starts printing. Optional.")]
        private AudioClip m_SFX;

        public string Text => m_Text;
        public AudioClip SFX => m_SFX;
    }

    [SerializeField, Tooltip("The 5 Ws prompts, printed in order.")]
    private List<Prompt> m_Prompts = new();

    [SerializeField, Tooltip("The ----- UI ----- prefab's OutputText TypewriterDisplay, used to print each Prompt.")]
    private TypewriterDisplay m_TypewriterDisplay;

    [SerializeField, Tooltip("The ----- UI ----- prefab's InputPanel (Input Carat/InputLineText). Deactivated in Awake since this Scene has no KeyboardInputHandler for it to bind to.")]
    private GameObject m_InputPanel;

    [SerializeField, Tooltip("Name of the Scene loaded once every Prompt has been shown and dismissed.")]
    private string m_ForestSceneName = "Forest";

    private int m_PromptIndex = -1;

    private void Awake()
    {
        if (m_InputPanel != null)
        {
            m_InputPanel.SetActive(false);
        }
    }

    private void Start()
    {
        _ShowNextPrompt();
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.anyKey.wasPressedThisFrame) return;
        if (m_TypewriterDisplay.IsTypewriting) return;

        if (m_PromptIndex >= m_Prompts.Count - 1)
        {
            StartCoroutine(_StartForestScene());
            return;
        }

        _ShowNextPrompt();
    }

    private void _ShowNextPrompt()
    {
        m_PromptIndex++;

        Prompt prompt = m_Prompts[m_PromptIndex];
        AudioManager.Instance.PlaySFXOneShot(prompt.SFX);
        m_TypewriterDisplay.PlayTypewriter(prompt.Text, null);
    }

    private IEnumerator _StartForestScene()
    {
        ScreenFadeManager.Instance.FadeOut(1f).WaitForCompletion();
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(m_ForestSceneName);
    }
}
