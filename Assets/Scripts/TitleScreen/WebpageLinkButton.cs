using UnityEngine;

/// <summary>
/// Plasmalot: Opens a configured webpage URL when clicked - used by the Credits panel's per-person icons.
/// </summary>
public class WebpageLinkButton : MonoBehaviour
{
    [SerializeField, Tooltip("URL opened in the system browser when this button is clicked.")]
    private string m_URL;

    public void OpenWebpage()
    {
        Application.OpenURL(m_URL);
    }
}
