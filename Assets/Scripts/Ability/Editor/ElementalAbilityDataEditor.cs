using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ElementalAbilityData))]
public class ElementalAbilityDataEditor : Editor
{
    // Basic Settings
    private SerializedProperty abilityNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty iconProp;
    
    // Ability Settings
    private SerializedProperty abilityTypeProp;
    private SerializedProperty cooldownDurationProp;
    private SerializedProperty manaCostProp;
    
    // Element Settings
    private SerializedProperty elementTypeProp;
    
    // Visual and Audio
    private SerializedProperty vfxPrefabProp;
    private SerializedProperty sfxClipProp;
    
    // Elemental Strike Settings
    private SerializedProperty stackAmountProp;
    private SerializedProperty strikeDamageProp;
    
    // Elemental Buff Settings
    private SerializedProperty damageMultiplierProp;
    
    // Elemental Projectile Settings
    private SerializedProperty attackCountForProjectileProp;
    private SerializedProperty projectileSpeedProp;
    private SerializedProperty projectileDamageProp;
    private SerializedProperty projectileRangeProp;
    private SerializedProperty projectilePrefabProp;
    
    // Fire Projectile Settings
    private SerializedProperty fireBurnDamageProp;
    private SerializedProperty fireBurnDurationProp;
    private SerializedProperty fireBurnTickRateProp;
    
    // Ice Projectile Settings
    private SerializedProperty iceSlowPercentProjectileProp;
    private SerializedProperty iceSlowDurationProjectileProp;
    private SerializedProperty iceFreezeChanceProp;
    
    // Poison Projectile Settings
    private SerializedProperty poisonDamageProjectileProp;
    private SerializedProperty poisonDurationProjectileProp;
    private SerializedProperty poisonTickRateProjectileProp;
    
    // Elemental Armor Settings
    private SerializedProperty damageReductionPercentProp;
    private SerializedProperty armorDurationProp;
    private SerializedProperty areaDamageRadiusProp;
    
    // Elemental Area Settings
    private SerializedProperty requiredStacksForAreaProp;
    private SerializedProperty areaDamageProp;
    private SerializedProperty areaRadiusProp;
    private SerializedProperty areaDurationProp;
    
    // Elemental Overflow Settings
    private SerializedProperty overflowStackAmountProp;
    private SerializedProperty overflowDamageProp;
    private SerializedProperty requiredEnemyKillsProp;
    
    // Elemental Burst Settings
    private SerializedProperty burstTriggerStacksProp;
    private SerializedProperty burstDamageProp;
    private SerializedProperty burstRadiusProp;
    
    // Elemental Aura Settings
    private SerializedProperty auraRadiusProp;
    private SerializedProperty auraStackTimeProp;
    
    // Elemental Orb Settings
    private SerializedProperty orbDurationProp;
    private SerializedProperty orbSpeedProp;
    private SerializedProperty orbSpriteProp;
    private SerializedProperty orbDetectionRadiusProp;
    private SerializedProperty orbProjectilePrefabProp;

    private void OnEnable()
    {
        // Basic Settings
        abilityNameProp = serializedObject.FindProperty("abilityName");
        descriptionProp = serializedObject.FindProperty("description");
        iconProp = serializedObject.FindProperty("icon");
        
        // Ability Settings
        abilityTypeProp = serializedObject.FindProperty("abilityType");
        cooldownDurationProp = serializedObject.FindProperty("cooldownDuration");
        manaCostProp = serializedObject.FindProperty("manaCost");
        
        // Element Settings
        elementTypeProp = serializedObject.FindProperty("elementType");
        
        // Visual and Audio
        vfxPrefabProp = serializedObject.FindProperty("vfxPrefab");
        sfxClipProp = serializedObject.FindProperty("sfxClip");
        
        // Elemental Strike Settings
        stackAmountProp = serializedObject.FindProperty("stackAmount");
        strikeDamageProp = serializedObject.FindProperty("strikeDamage");
        
        // Elemental Buff Settings
        damageMultiplierProp = serializedObject.FindProperty("damageMultiplier");
        
        // Elemental Projectile Settings
        attackCountForProjectileProp = serializedObject.FindProperty("attackCountForProjectile");
        projectileSpeedProp = serializedObject.FindProperty("projectileSpeed");
        projectileDamageProp = serializedObject.FindProperty("projectileDamage");
        projectileRangeProp = serializedObject.FindProperty("projectileRange");
        projectilePrefabProp = serializedObject.FindProperty("projectilePrefab");
        
        // Fire Projectile Settings
        fireBurnDamageProp = serializedObject.FindProperty("fireBurnDamage");
        fireBurnDurationProp = serializedObject.FindProperty("fireBurnDuration");
        fireBurnTickRateProp = serializedObject.FindProperty("fireBurnTickRate");
        
        // Ice Projectile Settings
        iceSlowPercentProjectileProp = serializedObject.FindProperty("iceSlowPercentProjectile");
        iceSlowDurationProjectileProp = serializedObject.FindProperty("iceSlowDurationProjectile");
        iceFreezeChanceProp = serializedObject.FindProperty("iceFreezeChance");
        
        // Poison Projectile Settings
        poisonDamageProjectileProp = serializedObject.FindProperty("poisonDamageProjectile");
        poisonDurationProjectileProp = serializedObject.FindProperty("poisonDurationProjectile");
        poisonTickRateProjectileProp = serializedObject.FindProperty("poisonTickRateProjectile");
        
        // Elemental Armor Settings
        damageReductionPercentProp = serializedObject.FindProperty("damageReductionPercent");
        armorDurationProp = serializedObject.FindProperty("armorDuration");
        areaDamageRadiusProp = serializedObject.FindProperty("areaDamageRadius");
        
        // Elemental Area Settings
        requiredStacksForAreaProp = serializedObject.FindProperty("requiredStacksForArea");
        areaDamageProp = serializedObject.FindProperty("areaDamage");
        areaRadiusProp = serializedObject.FindProperty("areaRadius");
        areaDurationProp = serializedObject.FindProperty("areaDuration");
        
        // Elemental Overflow Settings
        overflowStackAmountProp = serializedObject.FindProperty("overflowStackAmount");
        overflowDamageProp = serializedObject.FindProperty("overflowDamage");
        requiredEnemyKillsProp = serializedObject.FindProperty("requiredEnemyKills");
        
        // Elemental Burst Settings
        burstTriggerStacksProp = serializedObject.FindProperty("burstTriggerStacks");
        burstDamageProp = serializedObject.FindProperty("burstDamage");
        burstRadiusProp = serializedObject.FindProperty("burstRadius");
        
        // Elemental Aura Settings
        auraRadiusProp = serializedObject.FindProperty("auraRadius");
        auraStackTimeProp = serializedObject.FindProperty("auraStackTime");
        
        // Elemental Orb Settings
        orbDurationProp = serializedObject.FindProperty("orbDuration");
        orbSpeedProp = serializedObject.FindProperty("orbSpeed");
        orbSpriteProp = serializedObject.FindProperty("orbSprite");
        orbDetectionRadiusProp = serializedObject.FindProperty("orbDetectionRadius");
        orbProjectilePrefabProp = serializedObject.FindProperty("orbProjectilePrefab");
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
        EditorGUILayout.LabelField("Ability Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(abilityTypeProp);
        EditorGUILayout.PropertyField(cooldownDurationProp);
        EditorGUILayout.PropertyField(manaCostProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Element Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(elementTypeProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual and Audio", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(vfxPrefabProp);
        EditorGUILayout.PropertyField(sfxClipProp);
        
        // Ability Type'a göre özel ayarları göster
        AbilityType selectedAbilityType = (AbilityType)abilityTypeProp.enumValueIndex;
        
        switch (selectedAbilityType)
        {
            case AbilityType.ElementalStrike:
                ShowElementalStrikeSettings();
                break;
            case AbilityType.ElementalBuff:
                ShowElementalBuffSettings();
                break;
            case AbilityType.ElementalProjectile:
                ShowElementalProjectileSettings();
                break;
            case AbilityType.ElementalArmor:
                ShowElementalArmorSettings();
                break;
            case AbilityType.ElementalArea:
                ShowElementalAreaSettings();
                break;
            case AbilityType.ElementalOverflow:
                ShowElementalOverflowSettings();
                break;
            case AbilityType.ElementalBurst:
                ShowElementalBurstSettings();
                break;
            case AbilityType.ElementalAura:
                ShowElementalAuraSettings();
                break;
            case AbilityType.ElementalOrb:
                ShowElementalOrbSettings();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    private void ShowElementalStrikeSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Strike Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(stackAmountProp);
        EditorGUILayout.PropertyField(strikeDamageProp);

        // Element tipine göre özel alanlar
        ElementType selectedElementType = (ElementType)elementTypeProp.enumValueIndex;
        switch (selectedElementType)
        {
            case ElementType.Fire:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fireStackDamage"), new GUIContent("Fire Stack Damage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("burstDamage"), new GUIContent("Burst Damage (Fire)"));
                break;
            case ElementType.Ice:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("iceSlowPercent"), new GUIContent("Slow Percent (Ice)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("areaDuration"), new GUIContent("Slow Duration (Ice)"));
                break;
            case ElementType.Poison:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("poisonStackDamage"), new GUIContent("Poison Tick Damage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("areaDuration"), new GUIContent("Poison Duration"));
                break;
        }
    }
    
    private void ShowElementalBuffSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Buff Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(damageMultiplierProp);
    }
    
    private void ShowElementalProjectileSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Projectile Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(attackCountForProjectileProp);
        EditorGUILayout.PropertyField(projectileSpeedProp);
        EditorGUILayout.PropertyField(projectileDamageProp);
        EditorGUILayout.PropertyField(projectileRangeProp);
        EditorGUILayout.PropertyField(projectilePrefabProp);

        // Element tipine göre özel alanlar
        ElementType selectedElementType = (ElementType)elementTypeProp.enumValueIndex;
        switch (selectedElementType)
        {
            case ElementType.Fire:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Fire Projectile Effects", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(fireBurnDamageProp, new GUIContent("Burn Damage per Tick"));
                EditorGUILayout.PropertyField(fireBurnDurationProp, new GUIContent("Burn Duration (seconds)"));
                EditorGUILayout.PropertyField(fireBurnTickRateProp, new GUIContent("Burn Tick Rate (seconds)"));
                break;
            case ElementType.Ice:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Ice Projectile Effects", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(iceSlowPercentProjectileProp, new GUIContent("Slow Percent (%)"));
                EditorGUILayout.PropertyField(iceSlowDurationProjectileProp, new GUIContent("Slow Duration (seconds)"));
                EditorGUILayout.PropertyField(iceFreezeChanceProp, new GUIContent("Freeze Chance (0-1)"));
                break;
            case ElementType.Poison:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Poison Projectile Effects", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(poisonDamageProjectileProp, new GUIContent("Poison Damage per Tick"));
                EditorGUILayout.PropertyField(poisonDurationProjectileProp, new GUIContent("Poison Duration (seconds)"));
                EditorGUILayout.PropertyField(poisonTickRateProjectileProp, new GUIContent("Poison Tick Rate (seconds)"));
                break;
        }
    }
    
    private void ShowElementalArmorSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Armor Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(damageReductionPercentProp);
        EditorGUILayout.PropertyField(armorDurationProp);
        EditorGUILayout.PropertyField(areaDamageRadiusProp);
    }
    
    private void ShowElementalAreaSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Area Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(requiredStacksForAreaProp);
        EditorGUILayout.PropertyField(areaDamageProp);
        EditorGUILayout.PropertyField(areaRadiusProp);
        EditorGUILayout.PropertyField(areaDurationProp);
    }
    
    private void ShowElementalOverflowSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Overflow Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(overflowStackAmountProp);
        EditorGUILayout.PropertyField(overflowDamageProp);
        EditorGUILayout.PropertyField(requiredEnemyKillsProp);
    }
    
    private void ShowElementalBurstSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Burst Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(burstTriggerStacksProp);
        EditorGUILayout.PropertyField(burstDamageProp);
        EditorGUILayout.PropertyField(burstRadiusProp);
    }
    
    private void ShowElementalAuraSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Aura Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(auraRadiusProp);
        EditorGUILayout.PropertyField(auraStackTimeProp);
    }
    
    private void ShowElementalOrbSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Orb Settings", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Damage is fixed at 15 and managed by code", MessageType.Info);
        EditorGUILayout.PropertyField(orbDurationProp);
        EditorGUILayout.PropertyField(orbSpeedProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(orbSpriteProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detection & Projectile Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(orbDetectionRadiusProp);
        EditorGUILayout.PropertyField(orbProjectilePrefabProp);
    }
} 