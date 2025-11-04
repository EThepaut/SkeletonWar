using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public float damage = 10f;
    public float bulletSpeed = 15f;
    public int manaCost = 10;
    public float fireRate = 1f;
    public int penetration = 0;
    public float zone = 0f;
}