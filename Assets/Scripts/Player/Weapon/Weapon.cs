using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum WeaponType
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Rifle
}

public enum ShootType
{
    Single,
    Auto
}


[System.Serializable]// makes class visible in inspectorq
public class Weapon
{
    public WeaponType weaponType;
    public int bulletDamage;

    #region Regular Fire Variables
    public ShootType shootType;
    public int bulletsPerShot { get; private set; }
    private float defaultFireRate;
    public float fireRate = 1;//bullet per second
    private float lastShootTime;
    #endregion
    #region Burst Fire Variables
    private bool burstAvaiable;
    public bool burstActive;
    private int burstBulletsPerShot;
    private float burstFireRate;
    public float burstFireDelay { get; private set; }
    #endregion

    [Header("Magazine Details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    #region Weapon Generic Info Variables
    public float reloadSpeed { get; private set; }
    public float equipmentSpeed { get; private set; }
    public float gunDistance { get; private set; }
    #endregion
    #region Bullet Spread Variables
    [Header("Bullet Spread")]
    private float baseSpread = 1;
    private float currentSpread = 2;
    private float maximumSpread = 3;
    private float spreadIncreaseRate = .15f;
    private float lastSpreadUpdateTime;
    private float spreadCooldown = 1;
    #endregion

    public Weapon_Data weaponData { get; private set; }// serves as default weapon data
    
    public Weapon(Weapon_Data weaponData)
    {
        bulletDamage = weaponData.bulletDamage;
        bulletsInMagazine = weaponData.bulletsInMagazine;
        magazineCapacity = weaponData.magazineCapacity;
        totalReserveAmmo = weaponData.totalReserveAmmo;

        fireRate = weaponData.fireRate;
        weaponType = weaponData.weaponType;

        bulletsPerShot = weaponData.bulletsPerShot;
        shootType = weaponData.shootType;

        burstAvaiable = weaponData.burstAvaiable;
        burstActive = weaponData.burstActive;
        burstBulletsPerShot = weaponData.burstBulletsPerShot;
        burstFireRate = weaponData.burstFireRate;
        burstFireDelay = weaponData.burstFireDelay;

        baseSpread = weaponData.baseSpread;
        maximumSpread = weaponData.maxSpread;
        spreadIncreaseRate = weaponData.spreadIncreaseRate;

        reloadSpeed = weaponData.reloadSpeed;
        equipmentSpeed = weaponData.equipmentSpeed;
        gunDistance = weaponData.gunDistance;

        defaultFireRate = fireRate;
        this.weaponData = weaponData;

    }

    #region Spread Methods
    public Vector3 ApplySpread(Vector3 originalDirection)
    {
        UpdateSpread();

        float randomizedValue = Random.Range(-currentSpread, currentSpread);

        Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue / 2, randomizedValue);

        return spreadRotation * originalDirection;
    }

    private void UpdateSpread()
    {

        if (Time.time > lastSpreadUpdateTime + spreadCooldown)
        {
            currentSpread = baseSpread;
        }
        else
        {
            IncreaseSpread();
        }

        lastSpreadUpdateTime = Time.time;
    }

    private void IncreaseSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread + spreadIncreaseRate, baseSpread, maximumSpread);
    }
    #endregion

    #region Burst Methods
    public bool BurstActivated()
    {
        if (weaponType == WeaponType.Shotgun)
        {
            burstFireDelay = 0;
            return true;
        }

        return burstActive;
    }

    public void ToggleBurst()
    {
        if (burstAvaiable == false)
        {
            return;
        }
        else
        {
            burstActive = !burstActive;
        }

        if (burstActive)
        {
            bulletsPerShot = burstBulletsPerShot;
            fireRate = burstFireRate;
        }
        else
        {
            //reset to default values
            //these should be stored somewhere else for better flexibility
            bulletsPerShot = 1;
            fireRate = defaultFireRate;
        }
    }


    #endregion

    public bool CanShoot() => HasEnoughAmmo() && ReadyToFire();

    private bool ReadyToFire()
    {
        if (Time.time > lastShootTime + 1 / fireRate)
        {
            lastShootTime = Time.time;
            return true;
        }

        return false;
    }



    #region Reload methods
    private bool HasEnoughAmmo() => bulletsInMagazine > 0;

    public bool CanReload()
    {

        if (bulletsInMagazine == magazineCapacity)
        {
            return false;
        }


        if (totalReserveAmmo > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ReloadBullets()
    {

        int bulletsToReload = magazineCapacity;

        if (bulletsToReload > totalReserveAmmo)
        {
            bulletsToReload = totalReserveAmmo;
        }

        totalReserveAmmo -= bulletsToReload;
        bulletsInMagazine = bulletsToReload;

        if (totalReserveAmmo < 0)
        {
            totalReserveAmmo = 0;
        }
    }
    #endregion



}
