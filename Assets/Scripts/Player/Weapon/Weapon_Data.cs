using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "New Weapon_Data", menuName = "ScriptableObjects/Weapon_Data")]
public class Weapon_Data : ScriptableObject
{
    public string weaponName;
    [Header("Bullet Details")]
    public int bulletDamage;


    [Header("Magazine Details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Header("Regular Shooting")]
    public ShootType shootType;
    public int bulletsPerShot = 1;
    public float fireRate;

    [Header("Burst Shooting")]
    public bool burstAvaiable;
    public bool burstActive;
    public int burstBulletsPerShot;
    public float burstFireRate;
    public float burstFireDelay = .1f;

    [Header("Weapon Spread")]
    public float baseSpread;
    public float maxSpread;

    public float spreadIncreaseRate = .15f;

    [Header("Weapon Specifics")]
    public WeaponType weaponType;
    [Range(1, 3)]
    public float reloadSpeed = 1;
    [Range(1, 3)]
    public float equipmentSpeed = 1;
    [Range(4, 25)]
    public float gunDistance = 4;

    [Header("UI Elements")]
    public Sprite weaponIcon;

}
