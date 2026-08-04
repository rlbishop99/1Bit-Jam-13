using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PromptResponses.TransitionEntry))]
public class TransitionEntryDrawer : PropertyDrawer
{
    private const float m_kLineHeight = 18.0f;
    private const float m_kLineSpacing = 2.0f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty responseProp = property.FindPropertyRelative("m_Response");
        SerializedProperty keywordsProp = property.FindPropertyRelative("m_Keywords");
        SerializedProperty thresholdProp = property.FindPropertyRelative("m_RequiredIntentThreshold");
        SerializedProperty targetLevelProp = property.FindPropertyRelative("m_TargetLevelID");
        KeywordsSO bank = KeywordListDrawerUtility.ResolveKeywordsSO(property);

        float y = position.y;

        Rect targetLevelRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(targetLevelRect, targetLevelProp, new GUIContent("Target Level"));
        y += m_kLineHeight + m_kLineSpacing;

        float responseHeight = EditorGUI.GetPropertyHeight(responseProp);
        Rect responseRect = new Rect(position.x, y, position.width, responseHeight);
        EditorGUI.PropertyField(responseRect, responseProp);
        y += responseHeight + m_kLineSpacing;

        Rect thresholdRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(thresholdRect, thresholdProp, new GUIContent("Required Intent Threshold"));
        y += m_kLineHeight + m_kLineSpacing;

        KeywordListDrawerUtility.DrawKeywordList(position, y, keywordsProp, bank);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty responseProp = property.FindPropertyRelative("m_Response");
        SerializedProperty keywordsProp = property.FindPropertyRelative("m_Keywords");
        KeywordsSO bank = KeywordListDrawerUtility.ResolveKeywordsSO(property);

        float height = m_kLineHeight + m_kLineSpacing;
        height += EditorGUI.GetPropertyHeight(responseProp) + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        height += KeywordListDrawerUtility.GetKeywordListHeight(keywordsProp, bank);

        return height;
    }
}
