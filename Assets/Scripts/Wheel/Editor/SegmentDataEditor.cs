using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SegmentData))]
public class SegmentDataEditor : Editor
{
    SerializedProperty segmentID;
    SerializedProperty size;
    SerializedProperty type;
    SerializedProperty rarity;
    SerializedProperty description;
    SerializedProperty segmentPrefab;
    SerializedProperty statType;
    SerializedProperty statAmount;
    SerializedProperty segmentColor;

    private SegmentData segmentData;

    void OnEnable()
    {
        segmentData = (SegmentData)target;
        
        // Tüm property'leri al
        segmentID = serializedObject.FindProperty("segmentID");
        size = serializedObject.FindProperty("size");
        type = serializedObject.FindProperty("type");
        rarity = serializedObject.FindProperty("rarity");
        description = serializedObject.FindProperty("description");
        segmentPrefab = serializedObject.FindProperty("segmentPrefab");
        statType = serializedObject.FindProperty("statType");
        statAmount = serializedObject.FindProperty("statAmount");
        segmentColor = serializedObject.FindProperty("segmentColor");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        
        serializedObject.Update();

        // Ana özellikler
        EditorGUILayout.PropertyField(segmentID);
        EditorGUILayout.PropertyField(size);
        EditorGUILayout.PropertyField(type);
        EditorGUILayout.PropertyField(rarity);
        EditorGUILayout.PropertyField(description);
        EditorGUILayout.PropertyField(segmentPrefab);
        EditorGUILayout.PropertyField(statType);
        EditorGUILayout.PropertyField(statAmount);
        EditorGUILayout.PropertyField(segmentColor);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
} 