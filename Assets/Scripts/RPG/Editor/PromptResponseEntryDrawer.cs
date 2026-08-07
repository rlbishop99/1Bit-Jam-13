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
        SerializedProperty keywordGroupsProp = property.FindPropertyRelative("m_KeywordGroups");
        SerializedProperty thresholdProp = property.FindPropertyRelative("m_RequiredIntentThreshold");
        SerializedProperty advancesLayerProp = property.FindPropertyRelative("m_bAdvancesLayer");
        SerializedProperty layerToAdvanceToProp = property.FindPropertyRelative("m_LayerToAdvanceTo");
        SerializedProperty startsDatingSimProp = property.FindPropertyRelative("m_bStartsDatingSim");
        SerializedProperty gatingConditionsProp = property.FindPropertyRelative("m_GatingConditions");
        SerializedProperty entrySFXProp = property.FindPropertyRelative("m_TriggerSFX");
        SerializedProperty rewardItemProp = property.FindPropertyRelative("m_RewardItem");
        SerializedProperty markerToActivateProp = property.FindPropertyRelative("m_MarkerToActivate");
        KeywordsSO bank = KeywordListDrawerUtility.ResolveKeywordsSO(property);
        KeywordsSO baseBank = KeywordListDrawerUtility.ResolveBaseKeywordsSO(property);

        float y = position.y;
        bool bExpanded = KeywordListDrawerUtility.DrawEntryFoldoutHeader(position, y, property, KeywordListDrawerUtility.SummarizeForHeader(responseProp.stringValue));
        y += KeywordListDrawerUtility.GetFoldoutHeaderHeight();

        if (!bExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

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

        Rect startsDatingSimRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(startsDatingSimRect, startsDatingSimProp, new GUIContent("Starts Dating Sim"));
        y += m_kLineHeight + m_kLineSpacing;

        float gatingConditionsHeight = EditorGUI.GetPropertyHeight(gatingConditionsProp, true);
        Rect gatingConditionsRect = new Rect(position.x, y, position.width, gatingConditionsHeight);
        EditorGUI.PropertyField(gatingConditionsRect, gatingConditionsProp, new GUIContent("Gating Conditions"), true);
        y += gatingConditionsHeight + m_kLineSpacing;

        Rect triggerSFXRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(triggerSFXRect, entrySFXProp, new GUIContent("Trigger SFX"));
        y += m_kLineHeight + m_kLineSpacing;

        Rect rewardItemRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(rewardItemRect, rewardItemProp, new GUIContent("Reward Item"));
        y += m_kLineHeight + m_kLineSpacing;

        Rect markerToActivateRect = new Rect(position.x, y, position.width, m_kLineHeight);
        EditorGUI.PropertyField(markerToActivateRect, markerToActivateProp, new GUIContent("Marker To Activate"));
        y += m_kLineHeight + m_kLineSpacing;

        KeywordListDrawerUtility.DrawKeywordGroupList(position, y, keywordGroupsProp, bank, baseBank);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = KeywordListDrawerUtility.GetFoldoutHeaderHeight();
        if (!property.isExpanded) return height;

        SerializedProperty responseProp = property.FindPropertyRelative("m_Response");
        SerializedProperty keywordGroupsProp = property.FindPropertyRelative("m_KeywordGroups");
        SerializedProperty advancesLayerProp = property.FindPropertyRelative("m_bAdvancesLayer");
        SerializedProperty gatingConditionsProp = property.FindPropertyRelative("m_GatingConditions");
        KeywordsSO bank = KeywordListDrawerUtility.ResolveKeywordsSO(property);
        KeywordsSO baseBank = KeywordListDrawerUtility.ResolveBaseKeywordsSO(property);

        height += EditorGUI.GetPropertyHeight(responseProp) + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        if (advancesLayerProp.boolValue)
        {
            height += m_kLineHeight + m_kLineSpacing;
        }
        height += m_kLineHeight + m_kLineSpacing;
        height += EditorGUI.GetPropertyHeight(gatingConditionsProp, true) + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        height += m_kLineHeight + m_kLineSpacing;
        height += KeywordListDrawerUtility.GetKeywordGroupListHeight(keywordGroupsProp, bank, baseBank);

        return height;
    }
}
