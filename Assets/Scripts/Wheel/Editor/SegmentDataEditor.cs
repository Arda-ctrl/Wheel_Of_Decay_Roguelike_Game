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
    SerializedProperty rewardRarity;
    SerializedProperty rewardType;
    SerializedProperty rewardCount;

    private SegmentData segmentData;

    void OnEnable()
    {
        segmentData = (SegmentData)target;
        segmentID = serializedObject.FindProperty("segmentID");
        size = serializedObject.FindProperty("size");
        type = serializedObject.FindProperty("type");
        rarity = serializedObject.FindProperty("rarity");
        description = serializedObject.FindProperty("description");
        segmentPrefab = serializedObject.FindProperty("segmentPrefab");
        statType = serializedObject.FindProperty("statType");
        statAmount = serializedObject.FindProperty("statAmount");
        segmentColor = serializedObject.FindProperty("segmentColor");
        rewardRarity = serializedObject.FindProperty("rewardRarity");
        rewardType = serializedObject.FindProperty("rewardType");
        rewardCount = serializedObject.FindProperty("rewardCount");
    }

    public override void OnInspectorGUI()
    {
        if (segmentData == null || serializedObject == null)
        {
            EditorGUILayout.HelpBox("SegmentData veya serializedObject null!", MessageType.Error);
            return;
        }

        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

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
            else if (wheelType == WheelManipulationType.Repulsor)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("repulsorRange"));
            }
            // MirrorRedirect için özel parametre yok
            else if (wheelType == WheelManipulationType.CommonRedirector)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("commonRedirectorRange"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("commonRedirectorMinRarity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("commonRedirectorMaxRarity"));
            }
            else if (wheelType == WheelManipulationType.SafeEscape)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("safeEscapeRange"));
            }
            else if (wheelType == WheelManipulationType.ExplosiveEscape)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosiveEscapeRange"));
            }
            else if (wheelType == WheelManipulationType.SegmentSwapper)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("swapperRange"));
            }
        }
        else if (effectType == SegmentEffectType.OnRemoveEffect)
        {
            if (rewardRarity != null) EditorGUILayout.PropertyField(rewardRarity);
            if (rewardType != null) EditorGUILayout.PropertyField(rewardType);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rewardFillMode"));
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
} 