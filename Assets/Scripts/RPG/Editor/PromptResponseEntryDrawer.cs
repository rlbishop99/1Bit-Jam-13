using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PromptResponses.Entry))]
public class PromptResponseEntryDrawer : PropertyDrawer
{
    private const float m_kLineHeight = 18.0f;
    private const float m_kLineSpacing = 2.0f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty responseProp = property.FindPropertyRelative("m_Response");
        SerializedProperty keywordsProp = property.FindPropertyRelative("m_Keywords");
        SerializedProperty thresholdProp = property.FindPropertyRelative("m_RequiredIntentThreshold");
        SerializedProperty advancesLayerProp = property.FindPropertyRelative("m_bAdvancesLayer");
        SerializedProperty layerToAdvanceToProp = property.FindPropertyRelative("m_LayerToAdvanceTo");
        SerializedProperty gatingObjectProp = property.FindPropertyRelative("m_GatingObject");
        SerializedProperty presenceRequirementProp = property.FindPropertyRelative("m_PresenceRequirement");
        SerializedProperty entrySFXProp = property.FindPropertyRelative("m_TriggerSFX");
        SerializedProperty rewardItemProp = property.FindPropertyRelative("m_RewardItem");
        KeywordsSO bank = KeywordListDrawerUtility.ResolveKeywordsSO(property);
        KeywordsSO baseBank = KeywordListDrawerUtility.ResolveBaseKeywordsSO(property);

        float y = position.y;
        float responseHeight = EditorGUI.GetPropertyHeight(responseProp);
        Rect responseRect = new Rect(position.x, y, position.width, responseHeight);
        EditorGUI.PropertyField(responseRect, responseProp);
        y += responseHeight + m_kLineSpacing;

        Rect thresholdRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(thresholdRect, thresholdProp, new GUIContent("Required Intent Threshold"));
        y += m_kLineHeight + m_kLineSpacing;

        Rect advancesLayerRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(advancesLayerRect, advancesLayerProp, new GUIContent("Advances Layer"));
        y += m_kLineHeight + m_kLineSpacing;

        if (advancesLayerProp.boolValue)
        {
            Rect layerToAdvanceToRect = new Rect(position.x, y, position.width, m_kLineHeight);
            EditorGUI.PropertyField(layerToAdvanceToRect, layerToAdvanceToProp, new GUIContent("Layer To Advance To"));
            y += m_kLineHeight + m_kLineSpacing;
        }

        Rect gatingObjectRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(gatingObjectRect, gatingObjectProp, new GUIContent("Gating Object"));
        y += m_kLineHeight + m_kLineSpacing;

        if (gatingObjectProp.objectReferenceValue != null)
        {
            Rect presenceRequirementRect = new Rect(position.x, y, position.width, m_kLineHeight);
            EditorGUI.PropertyField(presenceRequirementRect, presenceRequirementProp, new GUIContent("Presence Requirement"));
            y += m_kLineHeight + m_kLineSpacing;
        }

        Rect triggerSFXRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(triggerSFXRect, entrySFXProp, new GUIContent("Trigger SFX"));
        y += m_kLineHeight + m_kLineSpacing;

        Rect rewardItemRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(rewardItemRect, rewardItemProp, new GUIContent("Reward Item"));
        y += m_kLineHeight + m_kLineSpacing;

        KeywordListDrawerUtility.DrawKeywordList(position, y, keywordsProp, bank, baseBank);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty responseProp = property.FindPropertyRelative("m_Response");
        SerializedProperty keywordsProp = property.FindPropertyRelative("m_Keywords");
        SerializedProperty advancesLayerProp = property.FindPropertyRelative("m_bAdvancesLayer");
        SerializedProperty gatingObjectProp = property.FindPropertyRelative("m_GatingObject");
        KeywordsSO bank = KeywordListDrawerUtility.ResolveKeywordsSO(property);
        KeywordsSO baseBank = KeywordListDrawerUtility.ResolveBaseKeywordsSO(property);

        float height = EditorGUI.GetPropertyHeight(responseProp) + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        if (advancesLayerProp.boolValue)
        {
            height += m_kLineHeight + m_kLineSpacing;
        }
        height += m_kLineHeight + m_kLineSpacing;
        if (gatingObjectProp.objectReferenceValue != null)
        {
            height += m_kLineHeight + m_kLineSpacing;
        }
        height += m_kLineHeight + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        height += KeywordListDrawerUtility.GetKeywordListHeight(keywordsProp, bank, baseBank);

        return height;
    }
}
