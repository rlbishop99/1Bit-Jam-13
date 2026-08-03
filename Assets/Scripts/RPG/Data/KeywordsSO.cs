using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: ScriptableObject that contains a list of keywords for a given Level/Variation.
/// This is used by the DialogueProcessor to determine if a given word is a keyword or not
/// and to determine Intents based on the keywords present in the dialogue.
/// </summary>
[CreateAssetMenu(fileName = "NewKeywordsSO", menuName = "RPG/Keywords SO")]
public class KeywordsSO : ScriptableObject
{
    [SerializeField, Tooltip("The full word bank of keywords available for this Level/Variation.")]
    private List<string> m_Keywords = new List<string>();

    public IReadOnlyList<string> Keywords => m_Keywords;

    private HashSet<string> m_KeywordLookup;

    public bool ContainsKeyword(string word)
    {
        _BuildLookupIfNeeded();
        return m_KeywordLookup.Contains(word.ToLowerInvariant());
    }

    private void OnEnable()
    {
        m_KeywordLookup = null;
    }

    private void _BuildLookupIfNeeded()
    {
        if (m_KeywordLookup != null) return;

        m_KeywordLookup = new HashSet<string>();
        foreach (string keyword in m_Keywords)
        {
            m_KeywordLookup.Add(keyword.ToLowerInvariant());
        }
    }
}
