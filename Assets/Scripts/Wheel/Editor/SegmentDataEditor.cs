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
    SerializedProperty effectID;
    SerializedProperty damageBoostAmount;
    SerializedProperty damagePercentageBoostAmount;
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
        effectID = serializedObject.FindProperty("effectID");
        damageBoostAmount = serializedObject.FindProperty("damageBoostAmount");
        damagePercentageBoostAmount = serializedObject.FindProperty("damagePercentageBoostAmount");
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

        // Effect seçimi
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Effect Settings", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(effectID);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        // Effect'e özel ayarlar
        if (effectID.intValue > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            switch (effectID.intValue)
            {
                case 1: // Damage Boost
                    EditorGUILayout.LabelField("Damage Boost Settings", EditorStyles.boldLabel);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(damageBoostAmount, new GUIContent("Boost Amount"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                    EditorGUILayout.HelpBox("Flat damage boost amount that will be added to base damage.", MessageType.Info);
                    break;

                case 2: // Damage Percentage Boost
                    EditorGUILayout.LabelField("Damage Percentage Boost Settings", EditorStyles.boldLabel);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(damagePercentageBoostAmount, new GUIContent("Boost Percentage"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                    EditorGUILayout.HelpBox("Percentage boost to damage (e.g. 0.1 = 10% increase)", MessageType.Info);
                    break;

                default:
                    EditorGUILayout.HelpBox("Unknown effect ID", MessageType.Warning);
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
} 