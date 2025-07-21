using UnityEngine;

public enum WeaponType { None, Pistol, Sword, Rifle }
public enum WeaponElement { None, Fire, Ice, Poison }

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Pistol";
    public WeaponType weaponType = WeaponType.Pistol;
    public WeaponElement elementType = WeaponElement.None;
    public float damage = 10f;
    public float fireRate = 0.5f; // Only for ranged weapons
    public Sprite weaponSprite;
    [Header("Projectile Settings")]
    public GameObject bulletPrefab; // Only for ranged weapons
    [Header("Element Settings")]
    public ElementData elementData; // Assign FireElementData, IceElementData, PoisonElementData, etc.
    // Future: anim, special fx, etc.
} 