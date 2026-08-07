using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: ScriptableObject describing one "Dating Game" style question asked during the Dating Sim.
/// Exactly one of the four Answer Options is correct.
/// </summary>
[CreateAssetMenu(fileName = "NewDatingSimQuestionSO", menuName = "RPG/Dating Sim/Question SO")]
public class DatingSimQuestionSO : ScriptableObject
{
    [SerializeField, TextArea(2, 5), Tooltip("The question text the Unicorn asks.")]
    private string m_QuestionText;

    [SerializeField, Tooltip("The four answer options the Player can navigate between. Exactly one is correct.")]
    private string[] m_AnswerOptions = new string[4];

    [SerializeField, Range(0, 3), Tooltip("Index into Answer Options that is the correct answer.")]
    private int m_CorrectAnswerIndex;

    public string QuestionText => m_QuestionText;
    public IReadOnlyList<string> AnswerOptions => m_AnswerOptions;
    public int CorrectAnswerIndex => m_CorrectAnswerIndex;

    private void OnValidate()
    {
        if (m_AnswerOptions == null || m_AnswerOptions.Length != 4)
        {
            Debug.LogWarning($"[{name}] must have exactly 4 Answer Options.", this);
        }
    }
}
