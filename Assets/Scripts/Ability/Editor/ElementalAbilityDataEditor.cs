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
    
    // Elemental Armor Settings
    private SerializedProperty damageReductionPercentProp;
    private SerializedProperty armorDurationProp;
    private SerializedProperty areaDamageRadiusProp;
    
    // Elemental Area Settings
    private SerializedProperty requiredStacksForAreaProp;
    private SerializedProperty areaDamageProp;
    private SerializedProperty areaRadiusProp;
    private SerializedProperty areaDurationProp;
    
    // Elemental Lance Barrage Settings
    private SerializedProperty lanceCountProp;
    private SerializedProperty lanceDamageProp;
    private SerializedProperty lanceRangeProp;
    
    // Elemental Overflow Settings
    private SerializedProperty overflowStackAmountProp;
    private SerializedProperty overflowDamageProp;
    private SerializedProperty requiredEnemyKillsProp;
    
    // Elemental Burst Settings
    private SerializedProperty burstTriggerStacksProp;
    private SerializedProperty burstDamageProp;
    private SerializedProperty burstRadiusProp;
    
    // Elemental Aura Settings
    private SerializedProperty auraDamageProp;
    private SerializedProperty auraRadiusProp;
    private SerializedProperty auraStackTimeProp;
    
    // Elemental Orb Settings
    private SerializedProperty orbDamageProp;
    private SerializedProperty orbDurationProp;
    private SerializedProperty orbSpeedProp;
    private SerializedProperty orbPrefabProp;

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
        
        // Elemental Armor Settings
        damageReductionPercentProp = serializedObject.FindProperty("damageReductionPercent");
        armorDurationProp = serializedObject.FindProperty("armorDuration");
        areaDamageRadiusProp = serializedObject.FindProperty("areaDamageRadius");
        
        // Elemental Area Settings
        requiredStacksForAreaProp = serializedObject.FindProperty("requiredStacksForArea");
        areaDamageProp = serializedObject.FindProperty("areaDamage");
        areaRadiusProp = serializedObject.FindProperty("areaRadius");
        areaDurationProp = serializedObject.FindProperty("areaDuration");
        
        // Elemental Lance Barrage Settings
        lanceCountProp = serializedObject.FindProperty("lanceCount");
        lanceDamageProp = serializedObject.FindProperty("lanceDamage");
        lanceRangeProp = serializedObject.FindProperty("lanceRange");
        
        // Elemental Overflow Settings
        overflowStackAmountProp = serializedObject.FindProperty("overflowStackAmount");
        overflowDamageProp = serializedObject.FindProperty("overflowDamage");
        requiredEnemyKillsProp = serializedObject.FindProperty("requiredEnemyKills");
        
        // Elemental Burst Settings
        burstTriggerStacksProp = serializedObject.FindProperty("burstTriggerStacks");
        burstDamageProp = serializedObject.FindProperty("burstDamage");
        burstRadiusProp = serializedObject.FindProperty("burstRadius");
        
        // Elemental Aura Settings
        auraDamageProp = serializedObject.FindProperty("auraDamage");
        auraRadiusProp = serializedObject.FindProperty("auraRadius");
        auraStackTimeProp = serializedObject.FindProperty("auraStackTime");
        
        // Elemental Orb Settings
        orbDamageProp = serializedObject.FindProperty("orbDamage");
        orbDurationProp = serializedObject.FindProperty("orbDuration");
        orbSpeedProp = serializedObject.FindProperty("orbSpeed");
        orbPrefabProp = serializedObject.FindProperty("orbPrefab");
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
            case AbilityType.ElementalLanceBarrage:
                ShowElementalLanceBarrageSettings();
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
    
    private void ShowElementalLanceBarrageSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Lance Barrage Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(lanceCountProp);
        EditorGUILayout.PropertyField(lanceDamageProp);
        EditorGUILayout.PropertyField(lanceRangeProp);
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
        EditorGUILayout.PropertyField(auraDamageProp);
        EditorGUILayout.PropertyField(auraRadiusProp);
        EditorGUILayout.PropertyField(auraStackTimeProp);
    }
    
    private void ShowElementalOrbSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elemental Orb Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(orbDamageProp);
        EditorGUILayout.PropertyField(orbDurationProp);
        EditorGUILayout.PropertyField(orbSpeedProp);
        EditorGUILayout.PropertyField(orbPrefabProp);
    }
} 