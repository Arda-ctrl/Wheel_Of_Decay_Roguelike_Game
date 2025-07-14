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
        EditorGUILayout.PropertyField(segmentColor);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("effectType"));

        var effectType = (SegmentEffectType)serializedObject.FindProperty("effectType").enumValueIndex;
        if (effectType == SegmentEffectType.StatBoost)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("statType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("statAmount"));
        }
        else if (effectType == SegmentEffectType.WheelManipulation)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelManipulationType"));
            var wheelType = (WheelManipulationType)serializedObject.FindProperty("wheelManipulationType").enumValueIndex;
            if (wheelType == WheelManipulationType.Redirector)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("redirectDirection"));
            }
            else if (wheelType == WheelManipulationType.BlackHole)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blackHoleRange"));
            }
            // Gerekirse ek parametreler
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
} 