using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Plasmalot: Read-only Play mode view of GameProgressManager's per-Level Layer state, since the backing
/// Dictionary itself isn't Unity-serializable/visible in the default Inspector.
/// </summary>
[CustomEditor(typeof(GameProgressManager))]
public class GameProgressManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying) return;

        GameProgressManager manager = (GameProgressManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current Layer Per Level", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        foreach (GameEnums.eLevelID levelID in Enum.GetValues(typeof(GameEnums.eLevelID)))
        {
            EditorGUILayout.LabelField(levelID.ToString(), manager.GetCurrentLayer(levelID).ToString());
        }
        EditorGUI.indentLevel--;

        // Layer state can change every frame during Play mode; keep this view live while selected.
        Repaint();
    }
}
