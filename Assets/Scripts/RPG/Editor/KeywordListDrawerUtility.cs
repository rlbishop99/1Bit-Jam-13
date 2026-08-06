using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Plasmalot: Shared keyword-list GUI (existing keywords + "Add Keyword" picker sourced from a KeywordsSO
/// word bank) used by both PromptResponseEntryDrawer and TransitionEntryDrawer.
/// </summary>
public static class KeywordListDrawerUtility
{
    private const float m_kLineHeight = 18.0f;
    private const float m_kLineSpacing = 2.0f;
    private const float m_kGroupIndent = 12.0f;
    private const string m_kAddKeywordLabel = "-- Add Keyword --";

    public static float DrawKeywordList(Rect position, float y, SerializedProperty keywordsProp, KeywordsSO bank, KeywordsSO baseBank = null)
    {
        if (bank == null && baseBank == null)
        {
            Rect helpRect = new Rect(position.x, y, position.width, m_kLineHeight * 2.0f);
            EditorGUI.HelpBox(helpRect, "Assign a KeywordsSO on this PromptResponses component to pick keywords.", MessageType.Info);
            return y + (m_kLineHeight * 2.0f);
        }

        for (int i = 0; i < keywordsProp.arraySize; i++)
        {
            SerializedProperty keywordProp = keywordsProp.GetArrayElementAtIndex(i);
            Rect keywordRect = new Rect(position.x, y, position.width - 24.0f, m_kLineHeight);
            Rect removeRect = new Rect(position.x + position.width - 20.0f, y, 20.0f, m_kLineHeight);

            EditorGUI.LabelField(keywordRect, keywordProp.stringValue);
            if (GUI.Button(removeRect, "x"))
            {
                keywordsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            y += m_kLineHeight + m_kLineSpacing;
        }

        List<string> options = new List<string> { m_kAddKeywordLabel };
        HashSet<string> addedOptions = new HashSet<string>();
        if (baseBank != null)
        {
            foreach (string keyword in baseBank.Keywords)
            {
                if (addedOptions.Add(keyword) && !_EntryContainsKeyword(keywordsProp, keyword))
                {
                    options.Add(keyword);
                }
            }
        }
        if (bank != null)
        {
            foreach (string keyword in bank.Keywords)
            {
                if (addedOptions.Add(keyword) && !_EntryContainsKeyword(keywordsProp, keyword))
                {
                    options.Add(keyword);
                }
            }
        }

        Rect popupRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.BeginChangeCheck();
        int selectedIndex = EditorGUI.Popup(popupRect, 0, options.ToArray());
        if (EditorGUI.EndChangeCheck() && selectedIndex > 0)
        {
            keywordsProp.InsertArrayElementAtIndex(keywordsProp.arraySize);
            keywordsProp.GetArrayElementAtIndex(keywordsProp.arraySize - 1).stringValue = options[selectedIndex];
        }
        y += m_kLineHeight;

        return y;
    }

    public static float GetKeywordListHeight(SerializedProperty keywordsProp, KeywordsSO bank, KeywordsSO baseBank = null)
    {
        if (bank == null && baseBank == null) return m_kLineHeight * 2.0f;

        return ((m_kLineHeight + m_kLineSpacing) * keywordsProp.arraySize) + m_kLineHeight;
    }

    public static float DrawKeywordGroupList(Rect position, float y, SerializedProperty keywordGroupsProp, KeywordsSO bank, KeywordsSO baseBank = null)
    {
        if (bank == null && baseBank == null)
        {
            Rect helpRect = new Rect(position.x, y, position.width, m_kLineHeight * 2.0f);
            EditorGUI.HelpBox(helpRect, "Assign a KeywordsSO on this PromptResponses component to pick keywords.", MessageType.Info);
            return y + (m_kLineHeight * 2.0f);
        }

        Rect indentedPosition = new Rect(position.x + m_kGroupIndent, position.y, position.width - m_kGroupIndent, position.height);

        for (int i = 0; i < keywordGroupsProp.arraySize; i++)
        {
            SerializedProperty groupProp = keywordGroupsProp.GetArrayElementAtIndex(i);
            SerializedProperty synonymsProp = groupProp.FindPropertyRelative("m_Synonyms");

            Rect headerLabelRect = new Rect(position.x, y, position.width - 24.0f, m_kLineHeight);
            Rect removeGroupRect = new Rect(position.x + position.width - 20.0f, y, 20.0f, m_kLineHeight);
            EditorGUI.LabelField(headerLabelRect, $"Group {i + 1} (any of)", EditorStyles.boldLabel);
            if (GUI.Button(removeGroupRect, "x"))
            {
                keywordGroupsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            y += m_kLineHeight + m_kLineSpacing;

            y = DrawKeywordList(indentedPosition, y, synonymsProp, bank, baseBank);
            y += m_kLineSpacing;
        }

        Rect addGroupRect = new Rect(position.x, y, position.width, m_kLineHeight);
        if (GUI.Button(addGroupRect, "+ Add Group"))
        {
            keywordGroupsProp.InsertArrayElementAtIndex(keywordGroupsProp.arraySize);
            SerializedProperty newGroupSynonymsProp = keywordGroupsProp.GetArrayElementAtIndex(keywordGroupsProp.arraySize - 1).FindPropertyRelative("m_Synonyms");
            newGroupSynonymsProp.ClearArray();
        }
        y += m_kLineHeight;

        return y;
    }

    public static float GetKeywordGroupListHeight(SerializedProperty keywordGroupsProp, KeywordsSO bank, KeywordsSO baseBank = null)
    {
        if (bank == null && baseBank == null) return m_kLineHeight * 2.0f;

        float height = m_kLineHeight;
        for (int i = 0; i < keywordGroupsProp.arraySize; i++)
        {
            SerializedProperty synonymsProp = keywordGroupsProp.GetArrayElementAtIndex(i).FindPropertyRelative("m_Synonyms");
            height += m_kLineHeight + m_kLineSpacing;
            height += GetKeywordListHeight(synonymsProp, bank, baseBank) + m_kLineSpacing;
        }

        return height;
    }

    /// <summary>Foldout header for a list element, using the built-in per-property-path expand state so no extra serialized field is needed.</summary>
    public static bool DrawEntryFoldoutHeader(Rect position, float y, SerializedProperty entryProperty, string summary)
    {
        Rect headerRect = new Rect(position.x, y, position.width, m_kLineHeight);
        entryProperty.isExpanded = EditorGUI.Foldout(headerRect, entryProperty.isExpanded, summary, true);
        return entryProperty.isExpanded;
    }

    public static float GetFoldoutHeaderHeight() => m_kLineHeight + m_kLineSpacing;

    public static string SummarizeForHeader(string text, int maxLength = 60)
    {
        if (string.IsNullOrWhiteSpace(text)) return "(no response)";

        string singleLine = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
        return singleLine.Length > maxLength ? singleLine.Substring(0, maxLength) + "..." : singleLine;
    }

    public static KeywordsSO ResolveKeywordsSO(SerializedProperty property)
    {
        SerializedProperty bankProp = property.serializedObject.FindProperty("m_KeywordsSO");
        return bankProp != null ? bankProp.objectReferenceValue as KeywordsSO : null;
    }

    public static KeywordsSO ResolveBaseKeywordsSO(SerializedProperty property)
    {
        SerializedProperty bankProp = property.serializedObject.FindProperty("m_BaseKeywordsSO");
        return bankProp != null ? bankProp.objectReferenceValue as KeywordsSO : null;
    }

    private static bool _EntryContainsKeyword(SerializedProperty keywordsProp, string keyword)
    {
        for (int i = 0; i < keywordsProp.arraySize; i++)
        {
            if (keywordsProp.GetArrayElementAtIndex(i).stringValue == keyword) return true;
        }
        return false;
    }
}
