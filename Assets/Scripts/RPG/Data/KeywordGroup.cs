using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasmalot: One concept slot within an Entry's required keywords. 
/// Any single Synonym present in the Player's input satisfies the whole group.
/// </summary>
[Serializable]
public struct KeywordGroup
{
    [SerializeField, Tooltip("Interchangeable words (from the assigned KeywordsSO word bank) - any one present in the Player's input satisfies this group.")]
    private List<string> m_Synonyms;

    public KeywordGroup(List<string> synonyms)
    {
        m_Synonyms = synonyms;
    }

    public List<string> Synonyms => m_Synonyms;
}
