using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        WeaponData weapon = (WeaponData)target;

        // Silah adı
        weapon.weaponName = EditorGUILayout.TextField("Weapon Name", weapon.weaponName);
        // Silah tipi
        weapon.weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weapon.weaponType);

        // Ateşli silahlar için
        if (weapon.weaponType == WeaponType.Pistol || weapon.weaponType == WeaponType.Rifle)
        {
            weapon.fireRate = EditorGUILayout.FloatField("Fire Rate", weapon.fireRate);
            weapon.elementType = (WeaponElement)EditorGUILayout.EnumPopup("Element Type", weapon.elementType);

            // Element None ise sadece damage ve mermi prefabı
            if (weapon.elementType == WeaponElement.None)
            {
                weapon.damage = EditorGUILayout.FloatField("Damage", weapon.damage);
                weapon.bulletPrefab = (GameObject)EditorGUILayout.ObjectField("Bullet Prefab", weapon.bulletPrefab, typeof(GameObject), false);
            }
            else // Elementli silahlar için
            {
                // İleride elemente özel ayarlar eklenebilir
                weapon.bulletPrefab = (GameObject)EditorGUILayout.ObjectField($"{weapon.elementType} Bullet Prefab", weapon.bulletPrefab, typeof(GameObject), false);
                // İleride elemente özel alanlar eklenebilir
            }
        }
        // ElementData SO seçimi (elementli silahlar için)
        if (weapon.weaponType == WeaponType.Pistol || weapon.weaponType == WeaponType.Rifle)
        {
            if (weapon.elementType != WeaponElement.None)
            {
                weapon.elementData = (ElementData)EditorGUILayout.ObjectField($"{weapon.elementType} Element Data", weapon.elementData, typeof(ElementData), false);
                if (weapon.elementData == null)
                {
                    EditorGUILayout.HelpBox($"{weapon.elementType} için bir ElementData SO atayın!", MessageType.Warning);
                }
            }
        }
        // Yakın dövüş silahı için
        else if (weapon.weaponType == WeaponType.Sword)
        {
            weapon.damage = EditorGUILayout.FloatField("Damage", weapon.damage);
            // İleride cooldown, animasyon vs. eklenebilir
        }

        // Sprite alanı (her silah için)
        weapon.weaponSprite = (Sprite)EditorGUILayout.ObjectField("Weapon Sprite", weapon.weaponSprite, typeof(Sprite), false);

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(weapon);
        }
    }
} 