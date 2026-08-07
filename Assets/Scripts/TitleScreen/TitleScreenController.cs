using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Plasmalot: Title Screen's Play/Quit actions - Play Game loads the configured Intro Scene by name,
/// Quit Game exits the application.
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [SerializeField, Tooltip("Name of the Scene loaded by the Play Game button.")]
    private string m_IntroSceneName = "Intro";

    public void PlayGame()
    {
        SceneManager.LoadScene(m_IntroSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
