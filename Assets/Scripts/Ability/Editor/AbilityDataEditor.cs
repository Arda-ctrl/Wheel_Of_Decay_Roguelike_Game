using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AbilityData))]
public class AbilityDataEditor : Editor
{
    private SerializedProperty abilityNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty iconProp;
    private SerializedProperty damageProp;
    private SerializedProperty cooldownDurationProp;
    private SerializedProperty effectTypeProp;
    private SerializedProperty vfxPrefabProp;
    private SerializedProperty sfxClipProp;
    private SerializedProperty effectDurationProp;
    private SerializedProperty effectRadiusProp;
    
    // Poison Settings
    private SerializedProperty poisonSlowAmountProp;
    private SerializedProperty poisonTickRateProp;
    private SerializedProperty maxPoisonStacksProp;
    private SerializedProperty stackDamageMultiplierProp;
    
    // Fire Settings
    private SerializedProperty initialBurstDamageProp;
    private SerializedProperty igniteDurationProp;
    private SerializedProperty igniteTickRateProp;
    private SerializedProperty hasAreaEffectProp;
    private SerializedProperty areaEffectRadiusProp;
    
    // Ice Settings
    private SerializedProperty freezeChanceProp;
    private SerializedProperty freezeDurationProp;
    private SerializedProperty chillSlowAmountProp;
    private SerializedProperty chillDurationProp;
    
    // Elemental Stack Settings
    private SerializedProperty hasElementalAbilityProp;
    private SerializedProperty hasElementalBuffProp;
    private SerializedProperty maxElementalStacksProp;
    private SerializedProperty elementalStackDecayTimeProp;
    private SerializedProperty elementalDamageMultiplierPerStackProp;
    private SerializedProperty normalElementalDamage1StackProp;
    private SerializedProperty normalElementalDamage2PlusStacksProp;
    private SerializedProperty normalElementalDamagePerAdditionalStackProp;

    private void OnEnable()
    {
        // Basic Settings
        abilityNameProp = serializedObject.FindProperty("abilityName");
        descriptionProp = serializedObject.FindProperty("description");
        iconProp = serializedObject.FindProperty("icon");
        
        // Gameplay Settings
        damageProp = serializedObject.FindProperty("damage");
        cooldownDurationProp = serializedObject.FindProperty("cooldownDuration");
        effectTypeProp = serializedObject.FindProperty("effectType");
        
        // Visual and Audio
        vfxPrefabProp = serializedObject.FindProperty("vfxPrefab");
        sfxClipProp = serializedObject.FindProperty("sfxClip");
        
        // Effect Settings
        effectDurationProp = serializedObject.FindProperty("effectDuration");
        effectRadiusProp = serializedObject.FindProperty("effectRadius");
        
        // Poison Settings
        poisonSlowAmountProp = serializedObject.FindProperty("poisonSlowAmount");
        poisonTickRateProp = serializedObject.FindProperty("poisonTickRate");
        maxPoisonStacksProp = serializedObject.FindProperty("maxPoisonStacks");
        stackDamageMultiplierProp = serializedObject.FindProperty("stackDamageMultiplier");
        
        // Fire Settings
        initialBurstDamageProp = serializedObject.FindProperty("initialBurstDamage");
        igniteDurationProp = serializedObject.FindProperty("igniteDuration");
        igniteTickRateProp = serializedObject.FindProperty("igniteTickRate");
        hasAreaEffectProp = serializedObject.FindProperty("hasAreaEffect");
        areaEffectRadiusProp = serializedObject.FindProperty("areaEffectRadius");
        
        // Ice Settings
        freezeChanceProp = serializedObject.FindProperty("freezeChance");
        freezeDurationProp = serializedObject.FindProperty("freezeDuration");
        chillSlowAmountProp = serializedObject.FindProperty("chillSlowAmount");
        chillDurationProp = serializedObject.FindProperty("chillDuration");
        
        // Elemental Stack Settings
        hasElementalAbilityProp = serializedObject.FindProperty("hasElementalAbility");
        hasElementalBuffProp = serializedObject.FindProperty("hasElementalBuff");
        maxElementalStacksProp = serializedObject.FindProperty("maxElementalStacks");
        elementalStackDecayTimeProp = serializedObject.FindProperty("elementalStackDecayTime");
        elementalDamageMultiplierPerStackProp = serializedObject.FindProperty("elementalDamageMultiplierPerStack");
        normalElementalDamage1StackProp = serializedObject.FindProperty("normalElementalDamage1Stack");
        normalElementalDamage2PlusStacksProp = serializedObject.FindProperty("normalElementalDamage2PlusStacks");
        normalElementalDamagePerAdditionalStackProp = serializedObject.FindProperty("normalElementalDamagePerAdditionalStack");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(abilityNameProp);
        EditorGUILayout.PropertyField(descriptionProp);
        EditorGUILayout.PropertyField(iconProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gameplay Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(damageProp);
        EditorGUILayout.PropertyField(cooldownDurationProp);
        EditorGUILayout.PropertyField(effectTypeProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual and Audio", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(vfxPrefabProp);
        EditorGUILayout.PropertyField(sfxClipProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effect Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(effectDurationProp);
        EditorGUILayout.PropertyField(effectRadiusProp);
        
        // Effect Type'a göre özel ayarları göster
        AbilityEffectType selectedEffectType = (AbilityEffectType)effectTypeProp.enumValueIndex;
        
        switch (selectedEffectType)
        {
            case AbilityEffectType.Poison:
                ShowPoisonSettings();
                break;
            case AbilityEffectType.Fire:
                ShowFireSettings();
                break;
            case AbilityEffectType.Ice:
                ShowIceSettings();
                break;
        }
        
        // Elemental Stack Settings her zaman göster
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Stack Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(hasElementalAbilityProp);
        EditorGUILayout.PropertyField(hasElementalBuffProp);
        EditorGUILayout.PropertyField(maxElementalStacksProp);
        EditorGUILayout.PropertyField(elementalStackDecayTimeProp);
        EditorGUILayout.PropertyField(elementalDamageMultiplierPerStackProp);
        EditorGUILayout.PropertyField(normalElementalDamage1StackProp);
        EditorGUILayout.PropertyField(normalElementalDamage2PlusStacksProp);
        EditorGUILayout.PropertyField(normalElementalDamagePerAdditionalStackProp);

        serializedObject.ApplyModifiedProperties();
    }
    
    private void ShowPoisonSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Poison Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(poisonSlowAmountProp);
        EditorGUILayout.PropertyField(poisonTickRateProp);
        EditorGUILayout.PropertyField(maxPoisonStacksProp);
        EditorGUILayout.PropertyField(stackDamageMultiplierProp);
    }
    
    private void ShowFireSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fire Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(initialBurstDamageProp);
        EditorGUILayout.PropertyField(igniteDurationProp);
        EditorGUILayout.PropertyField(igniteTickRateProp);
        EditorGUILayout.PropertyField(hasAreaEffectProp);
        EditorGUILayout.PropertyField(areaEffectRadiusProp);
    }
    
    private void ShowIceSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ice Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(freezeChanceProp);
        EditorGUILayout.PropertyField(freezeDurationProp);
        EditorGUILayout.PropertyField(chillSlowAmountProp);
        EditorGUILayout.PropertyField(chillDurationProp);
    }
} 