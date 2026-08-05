using DG.Tweening;
using UnityEngine;

/// <summary>
/// Plasmalot: Singleton that owns the one Music AudioSource and one SFX AudioSource used across the game.
/// Exposes play/stop/pause/volume controls, with DOTween used to fade Music and SFX volume changes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager m_Instance;
    public static AudioManager Instance => m_Instance;

    [SerializeField, Tooltip("Plays looping background Music. Volume is faded via DOTween.")]
    private AudioSource m_MusicSource;

    [SerializeField, Tooltip("Plays looping SFX and one-shot SFX. Volume is faded via DOTween.")]
    private AudioSource m_SFXSource;

    [SerializeField, Tooltip("Plays looping SFX.")]
    private AudioSource m_LoopingSFXSource;

    [SerializeField, Tooltip("Default duration (seconds) used for Play/Stop fades when no duration is specified.")]
    private float m_DefaultFadeDuration = 1f;

    private float m_MusicVolume = 1f;
    private float m_SFXVolume = 1f;

    private Tween m_MusicFadeTween;
    private Tween m_SFXFadeTween;

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

    #region Music

    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = -1f)
    {
        if (clip == null) return;

        fadeDuration = fadeDuration >= 0f ? fadeDuration : m_DefaultFadeDuration;

        m_MusicFadeTween?.Kill();
        m_MusicSource.clip = clip;
        m_MusicSource.loop = loop;
        m_MusicSource.volume = 0f;
        m_MusicSource.Play();
        m_MusicFadeTween = m_MusicSource.DOFade(m_MusicVolume, fadeDuration);
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        fadeDuration = fadeDuration >= 0f ? fadeDuration : m_DefaultFadeDuration;

        m_MusicFadeTween?.Kill();
        m_MusicFadeTween = m_MusicSource.DOFade(0f, fadeDuration).OnComplete(m_MusicSource.Stop);
    }

    public void PauseMusic()
    {
        m_MusicSource.Pause();
    }

    public void ResumeMusic()
    {
        m_MusicSource.UnPause();
    }

    public void SetMusicVolume(float volume, float fadeDuration = 0f)
    {
        m_MusicVolume = Mathf.Clamp01(volume);

        if (fadeDuration <= 0f)
        {
            m_MusicFadeTween?.Kill();
            m_MusicSource.volume = m_MusicVolume;
            return;
        }

        m_MusicFadeTween?.Kill();
        m_MusicFadeTween = m_MusicSource.DOFade(m_MusicVolume, fadeDuration);
    }

    #endregion

    #region SFX

    public void PlaySFXLoop(AudioClip clip, float fadeDuration = -1f)
    {
        if (clip == null) return;

        fadeDuration = fadeDuration >= 0f ? fadeDuration : m_DefaultFadeDuration;

        m_SFXFadeTween?.Kill();
        m_SFXSource.clip = clip;
        m_SFXSource.loop = true;
        m_SFXSource.volume = 0f;
        m_SFXSource.Play();
        m_SFXFadeTween = m_SFXSource.DOFade(m_SFXVolume, fadeDuration);
    }

    public void PlaySFXOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        m_SFXSource.PlayOneShot(clip, m_SFXVolume * volumeScale);
    }

    public void StopSFX(float fadeDuration = -1f)
    {
        fadeDuration = fadeDuration >= 0f ? fadeDuration : m_DefaultFadeDuration;

        m_SFXFadeTween?.Kill();
        m_SFXFadeTween = m_SFXSource.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            m_SFXSource.Stop();
            m_SFXSource.volume = m_SFXVolume;
        });
    }

    public void PauseSFX()
    {
        m_SFXSource.Pause();
    }

    public void ResumeSFX()
    {
        m_SFXSource.UnPause();
    }

    public void SetSFXVolume(float volume, float fadeDuration = 0f)
    {
        m_SFXVolume = Mathf.Clamp01(volume);

        if (fadeDuration <= 0f)
        {
            m_SFXFadeTween?.Kill();
            m_SFXSource.volume = m_SFXVolume;
            return;
        }

        m_SFXFadeTween?.Kill();
        m_SFXFadeTween = m_SFXSource.DOFade(m_SFXVolume, fadeDuration);
    }

    #endregion
}
