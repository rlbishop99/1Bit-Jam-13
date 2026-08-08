using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Plasmalot: Title Screen's Play/Quit actions - Play Game fades the screen to black via ScreenFadeManager,
/// then loads the configured Intro Scene by name once the fade has completed. Quit Game exits the application.
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [SerializeField, Tooltip("Name of the Scene loaded by the Play Game button, once the fade-to-black completes.")]
    private string m_IntroSceneName = "Intro";

    [SerializeField, Tooltip("Duration of the Play Game fade-to-black, in seconds.")]
    private float m_FadeOutDuration = 1.0f;

    [Header("Audio")]
    [SerializeField, Tooltip("SFX played when the Play Game button is pressed.")]
    private AudioClip m_PlayGameSFX;

    public void PlayGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXOneShot(m_PlayGameSFX);
        }

        ScreenFadeManager.Instance.FadeOut(m_FadeOutDuration).OnComplete(() => SceneManager.LoadScene(m_IntroSceneName));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
