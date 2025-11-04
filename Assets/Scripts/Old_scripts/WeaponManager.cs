using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Objects in Scene")]
    public List<GameObject> weaponObjects;

    private GameObject currentWeapon;
    private int currentWeaponIndex = 0;

    void Start()
    {
        for (int i = 0; i < weaponObjects.Count; i++)
        {
            weaponObjects[i].SetActive(i == 0);
        }

        if (weaponObjects.Count > 0)
            currentWeapon = weaponObjects[0];
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame) SwitchWeapon(0);
        if (Keyboard.current.tKey.wasPressedThisFrame) SwitchWeapon(1);
        if (Keyboard.current.yKey.wasPressedThisFrame) SwitchWeapon(2);
    }

    public void SwitchWeapon(int index)
    {
        if (weaponObjects == null || index < 0 || index >= weaponObjects.Count)
            return;

        if (currentWeapon != null)
            currentWeapon.SetActive(false);

        currentWeapon = weaponObjects[index];
        currentWeapon.SetActive(true);
        currentWeaponIndex = index;
    }
}