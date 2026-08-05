using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Plasmalot: Tags which Level/Variation is active in the scene, and owns transitioning the Player
/// to another Level (playing this Level's Walking SFX, then loading the target Level's Scene).
/// </summary>
public class LevelContext : MonoBehaviour
{
    private static LevelContext m_Instance;
    public static LevelContext Instance => m_Instance;

    [SerializeField, Tooltip("The Level currently active in the scene.")]
    private GameEnums.eLevelID m_CurrentLevelID = GameEnums.eLevelID.Forest;

    [SerializeField, Tooltip("The Variation of the current Level currently active in the scene.")]
    private GameEnums.eVariationID m_CurrentVariationID = GameEnums.eVariationID.Default;

    [SerializeField, Tooltip("SFX played when the Player transitions away from this Level. Unique per Level.")]
    private AudioClip m_WalkingSFXClip;

    public GameEnums.eLevelID CurrentLevelID => m_CurrentLevelID;
    public GameEnums.eVariationID CurrentVariationID => m_CurrentVariationID;

    /// <summary>
    /// Plasmalot: Fired synchronously at the start of TransitionToLevel, before the fade/load routine begins.
    /// </summary>
    public event Action<GameEnums.eLevelID> OnTransitionStarted;

    private void Awake()
    {
        m_Instance = this;
    }

    public void SetLevelAndVariation(GameEnums.eLevelID levelID, GameEnums.eVariationID variationID)
    {
        m_CurrentLevelID = levelID;
        m_CurrentVariationID = variationID;
    }

    public void TransitionToLevel(GameEnums.eLevelID targetLevelID)
    {
        OnTransitionStarted?.Invoke(targetLevelID);
        StartCoroutine(_TransitionRoutine(targetLevelID));
    }

    private IEnumerator _TransitionRoutine(GameEnums.eLevelID targetLevelID)
    {
        ScreenFadeManager.Instance.FadeOut();

        float sfxDuration = 0f;
        if (m_WalkingSFXClip != null)
        {
            AudioManager.Instance.PlaySFXOneShot(m_WalkingSFXClip);
            sfxDuration = m_WalkingSFXClip.length;
        }

        float waitDuration = Mathf.Max(ScreenFadeManager.Instance.FadeDuration, sfxDuration);
        yield return new WaitForSeconds(waitDuration);

        SceneManager.LoadScene(LevelSceneMap.GetSceneName(targetLevelID));
    }
}
