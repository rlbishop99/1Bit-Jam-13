using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plasmalot: Cycles a target Image through whichever Sprite collection is currently assigned, on a serialized
/// timer, while cycling is active. Used to idle-animate the Unicorn while a Dating Sim question is being drawn
/// and while the Player is choosing an answer.
/// </summary>
public class DatingSimSpriteCycler : MonoBehaviour
{
    [SerializeField, Tooltip("Image the cycling Sprite is assigned to.")]
    private Image m_TargetImage;

    [SerializeField, Tooltip("Seconds between each Sprite change while cycling.")]
    private float m_CycleIntervalSeconds = 2.0f;

    [SerializeField, Tooltip("Seconds spent on frame 0 before cycling to the next frame, instead of the normal Cycle Interval Seconds.")]
    private float m_Index0CycleIntervalSeconds = 0.5f;

    private Sprite[] m_CurrentCollection;
    private int m_CurrentIndex;
    private float m_CycleTimer;
    private bool m_bIsCycling;

    /// <summary>
    /// Plasmalot: Assigns a new Sprite collection and immediately shows a frame from it - frame 0.
    /// If bRandomStartFrame is set, a random frame from the collection is shown instead.
    /// </summary>
    public void SetCollection(Sprite[] collection, bool bRandomStartFrame = false)
    {
        m_CurrentCollection = collection;
        m_CurrentIndex = bRandomStartFrame ? Random.Range(0, collection.Length) : 0;
        m_CycleTimer = 0.0f;
        _ShowCurrentFrame();
    }

    public void StartCycling() => m_bIsCycling = true;
    public void StopCycling() => m_bIsCycling = false;

    private void Update()
    {
        if (!m_bIsCycling || m_CurrentCollection == null || m_CurrentCollection.Length <= 1) return;

        float cycleInterval = m_CurrentIndex == 0 ? m_Index0CycleIntervalSeconds : m_CycleIntervalSeconds;

        m_CycleTimer += Time.deltaTime;
        if (m_CycleTimer >= cycleInterval)
        {
            m_CycleTimer = 0.0f;
            m_CurrentIndex = (m_CurrentIndex + 1) % m_CurrentCollection.Length;
            _ShowCurrentFrame();
        }
    }

    private void _ShowCurrentFrame()
    {
        if (m_CurrentCollection == null || m_CurrentCollection.Length == 0) return;

        m_TargetImage.sprite = m_CurrentCollection[m_CurrentIndex];
    }
}
