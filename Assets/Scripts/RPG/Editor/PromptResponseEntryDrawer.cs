using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PromptResponses.Entry))]
public class PromptResponseEntryDrawer : PropertyDrawer
{
    private const float m_kLineHeight = 18.0f;
    private const float m_kLineSpacing = 2.0f;
    private const string m_kAddKeywordLabel = "-- Add Keyword --";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty responseProp = property.FindPropertyRelative("m_Response");
        SerializedProperty keywordsProp = property.FindPropertyRelative("m_Keywords");
        SerializedProperty thresholdProp = property.FindPropertyRelative("m_RequiredIntentThreshold");
        KeywordsSO bank = _ResolveKeywordsSO(property);

        float y = position.y;
        float responseHeight = EditorGUI.GetPropertyHeight(responseProp);
        Rect responseRect = new Rect(position.x, y, position.width, responseHeight);
        EditorGUI.PropertyField(responseRect, responseProp);
        y += responseHeight + m_kLineSpacing;

        Rect thresholdRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(thresholdRect, thresholdProp, new GUIContent("Required Intent Threshold"));
        y += m_kLineHeight + m_kLineSpacing;

        if (bank == null)
        {
            Rect helpRect = new Rect(position.x, y, position.width, m_kLineHeight * 2.0f);
            EditorGUI.HelpBox(helpRect, "Assign a KeywordsSO on this PromptResponses component to pick keywords.", MessageType.Info);
            EditorGUI.EndProperty();
            return;
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
        foreach (string keyword in bank.Keywords)
        {
            if (!_EntryContainsKeyword(keywordsProp, keyword))
            {
                options.Add(keyword);
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

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty responseProp = property.FindPropertyRelative("m_Response");
        SerializedProperty keywordsProp = property.FindPropertyRelative("m_Keywords");
        KeywordsSO bank = _ResolveKeywordsSO(property);

        float height = EditorGUI.GetPropertyHeight(responseProp) + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;

        if (bank == null)
        {
            return height + (m_kLineHeight * 2.0f);
        }

        height += (m_kLineHeight + m_kLineSpacing) * keywordsProp.arraySize;
        height += m_kLineHeight;

        return height;
    }

    private KeywordsSO _ResolveKeywordsSO(SerializedProperty property)
    {
        SerializedProperty bankProp = property.serializedObject.FindProperty("m_KeywordsSO");
        return bankProp != null ? bankProp.objectReferenceValue as KeywordsSO : null;
    }

    private bool _EntryContainsKeyword(SerializedProperty keywordsProp, string keyword)
    {
        for (int i = 0; i < keywordsProp.arraySize; i++)
        {
            if (keywordsProp.GetArrayElementAtIndex(i).stringValue == keyword) return true;
        }
        return false;
    }
}
