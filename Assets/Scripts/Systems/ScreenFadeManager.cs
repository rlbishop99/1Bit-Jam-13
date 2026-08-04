using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Plasmalot: Singleton that owns a full-screen CanvasGroup used to fade to/from black across Scene loads
/// (e.g. Level transitions). Automatically fades back in whenever a new Scene finishes loading.
/// </summary>
public class ScreenFadeManager : MonoBehaviour
{
    private static ScreenFadeManager m_Instance;
    public static ScreenFadeManager Instance => m_Instance;

    [SerializeField, Tooltip("Full-screen CanvasGroup faded to/from black.")]
    private CanvasGroup m_FadeCanvasGroup;

    [SerializeField, Tooltip("Default duration (seconds) used for fade in/out.")]
    private float m_DefaultFadeDuration = 1f;

    private Tween m_FadeTween;

    public float FadeDuration => m_DefaultFadeDuration;

    private void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += _HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= _HandleSceneLoaded;
    }

    public Tween FadeOut(float duration = -1f)
    {
        duration = duration >= 0f ? duration : m_DefaultFadeDuration;

        m_FadeCanvasGroup.blocksRaycasts = true;
        m_FadeTween?.Kill();
        m_FadeTween = m_FadeCanvasGroup.DOFade(1f, duration);
        return m_FadeTween;
    }

    public Tween FadeIn(float duration = -1f)
    {
        duration = duration >= 0f ? duration : m_DefaultFadeDuration;

        m_FadeTween?.Kill();
        m_FadeTween = m_FadeCanvasGroup.DOFade(0f, duration)
            .OnComplete(() => m_FadeCanvasGroup.blocksRaycasts = false);
        return m_FadeTween;
    }

    private void _HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FadeIn();
    }
}
