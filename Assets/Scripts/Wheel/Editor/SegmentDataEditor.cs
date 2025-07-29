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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("statBonusMode"));
            var statType = (StatType)serializedObject.FindProperty("statType").enumValueIndex;
            var statBonusMode = (StatBonusMode)serializedObject.FindProperty("statBonusMode").enumValueIndex;
            if (statBonusMode == StatBonusMode.EmptySlotCount)
            {
                EditorGUILayout.HelpBox("Her boş slot için statAmount kadar bonus verir.", MessageType.None);
            }
            else if (statBonusMode == StatBonusMode.FilledSlotCount)
            {
                EditorGUILayout.HelpBox("Her dolu slot için statAmount kadar bonus verir.", MessageType.None);
            }
            else if (statBonusMode == StatBonusMode.SmallSegmentCount)
            {
                EditorGUILayout.HelpBox("Çarktaki küçük segment (size=1) sayısına göre bonus verir.", MessageType.None);
            }
            else if (statBonusMode == StatBonusMode.LargeSegmentCount)
            {
                EditorGUILayout.HelpBox("Çarktaki büyük segment (size>1) sayısına göre bonus verir.", MessageType.None);
            }
            else if (statBonusMode == StatBonusMode.SiblingAdjacency)
            {
                EditorGUILayout.HelpBox("Aynı tip segmentler yan yana ise bonus iki katına çıkar.", MessageType.None);
            }
            else if (statBonusMode == StatBonusMode.Persistent)
            {
                EditorGUILayout.HelpBox("Bu segment iğneye gelince silinmez, statı güçlenir.", MessageType.None);
            }
            else if (statBonusMode == StatBonusMode.Isolated)
            {
                EditorGUILayout.HelpBox("Yanında hiç segment olmayan (boş slotlarla çevrili) segmentler isolatedBonusAmount kadar bonus alır. Yanı doluysa adjacentBonusAmount kadar bonus alır.", MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isolatedBonusAmount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("adjacentBonusAmount"));
            }
            else if (statBonusMode == StatBonusMode.DecayOverTime)
            {
                EditorGUILayout.HelpBox("Segment her spin sonrası bonusunu yavaşça kaybeder (her spin sonrası -X stat). 0'a inince silinsin mi? (toggle)", MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("decayStartValue"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("decayAmountPerSpin"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("decayRemoveAtZero"));
            }
            else if (statBonusMode == StatBonusMode.GrowthOverTime)
            {
                EditorGUILayout.HelpBox("Segment her spin sonrası bonusunu artırır (her spin sonrası +X stat).", MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("growthStartValue"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("growthAmountPerSpin"));
            }
            
            // Random stat ayarları (sadece statType Random seçiliyse göster)
            if (statType == StatType.Random)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Random Stat Ayarları:", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Hangi statların rastgele seçilebileceğini belirleyin.", MessageType.None);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("includeAttack"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("includeDefence"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("includeAttackSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("includeMovementSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("includeCriticalChance"));
            }
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