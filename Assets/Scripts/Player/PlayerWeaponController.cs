using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsAlly;
    [Space]

    private const float referenceBulletSpeed = 20;
    public Player player;

    [SerializeField] private Weapon_Data defaultWeaponData;
    [SerializeField] private Weapon currentWeapon;
    private bool weaponReady;
    private bool isShooting;


    [Header("Bullet Details")]
    [SerializeField] private float bulletImpactForce = 100;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    //[SerializeField] private Transform gunPoint;
    [SerializeField] private Transform weaponHolder;

    [Header("Inventory")]
    [SerializeField] private List<Weapon> weaponSlots;
    [SerializeField] private int maxSlots = 2;

    [SerializeField] private GameObject weaponPickupPrefab;



    private void Start()
    {
        player = GetComponent<Player>();
        AssignInputEvents();

        Invoke("EquipStartingWeapon", 0.1f);
    }

    private void Update()
    {
        if (isShooting)
        {
            Shoot();
        }

    }

    public void UpdateWeaponUI()
    {
        UI.instance.inGameUI.UpdateWeaponUI(weaponSlots, currentWeapon);
    }



    #region Slots Management - Pickup, Drop, Equip, ready weapon
    private void EquipStartingWeapon()
    {
        weaponSlots[0] = new Weapon(defaultWeaponData);
        EquipWeapon(0);
    }
    private void EquipWeapon(int i)
    {
        if(i >= weaponSlots.Count)
        {
            return;
        }
        
        SetWeaponReady(false);

        currentWeapon = weaponSlots[i];
        player.weaponVisuals.PlayWeaponEquipAnimation();
        UpdateWeaponUI();
    }

    public void PickupWeapon(Weapon newWeapon)
    {

        if(WeaponInSlots(newWeapon.weaponType) != null)
        {
            WeaponInSlots(newWeapon.weaponType).totalReserveAmmo += newWeapon.bulletsInMagazine;
            return;
        }



        if (weaponSlots.Count >= maxSlots && newWeapon.weaponType != currentWeapon.weaponType)
        {
            int weaponIndex = weaponSlots.IndexOf(currentWeapon);

            player.weaponVisuals.SwitchOffWeaponModels();
            weaponSlots[weaponIndex] = newWeapon;

            CreateWeaponOnTheGround();
            EquipWeapon(weaponIndex);

            return;
        }


        weaponSlots.Add(newWeapon);
        player.weaponVisuals.SwitchOnBackupWeaponModel();
        UpdateWeaponUI();
    }

    private void DropWeapon()
    {
        if (HasOnlyOneWeapon())
        {
            return;
        }

        CreateWeaponOnTheGround();

        weaponSlots.Remove(currentWeapon);
        EquipWeapon(0);
    }

    private void CreateWeaponOnTheGround()
    {
        GameObject droppedWeapon = ObjectPool.Instance.GetObject(weaponPickupPrefab, transform);
        droppedWeapon.GetComponent<Pickup_Weapon>()?.SetupPickupWeapon(currentWeapon, transform);
    }

    public void SetWeaponReady(bool ready)
    {
        weaponReady = ready;
        if (ready)
        {
            player.sound.weaponReady.Play();
        }
    }
    
    public bool WeaponReady() => weaponReady;

    #endregion

    private IEnumerator BurstFire()
    {
        SetWeaponReady(false);

        for (int i = 1; i <= currentWeapon.bulletsPerShot; i++)
        {
            FireSingleBullet();
            yield return new WaitForSeconds(currentWeapon.burstFireDelay);

            if (i >= currentWeapon.bulletsPerShot)
            {
                SetWeaponReady(true);
            }
        }


    }

    private void Shoot()
    {
        if (WeaponReady() == false)
        {
            return;
        }

        if (currentWeapon.CanShoot() == false)
        {
            return;
        }

        player.weaponVisuals.PlayFireAnimation();

        if (currentWeapon.shootType == ShootType.Single)
        {
            isShooting = false;
        }

        if (currentWeapon.BurstActivated() == true)
        {
            StartCoroutine(BurstFire());
            return;
        }


        FireSingleBullet();
        TriggerEnemyDodge();
    }

    private void FireSingleBullet()
    {

        currentWeapon.bulletsInMagazine--;
        UpdateWeaponUI();
        player.weaponVisuals.CurrentWeaponModel().fireSFX.Play();


        GameObject newBullet = ObjectPool.Instance.GetObject(bulletPrefab, GunPoint());
       
        newBullet.transform.rotation = Quaternion.LookRotation(GunPoint().forward);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.BulletSetup(whatIsAlly, currentWeapon.bulletDamage, currentWeapon.gunDistance, bulletImpactForce);

        Vector3 bulletsDirection = currentWeapon.ApplySpread(BulletDirection());

        rbNewBullet.mass = referenceBulletSpeed / bulletSpeed;
        rbNewBullet.linearVelocity = bulletsDirection * bulletSpeed;
    }

    private void Reload()
    {// reload happen in player animation events script
        SetWeaponReady(false);
        player.weaponVisuals.PlayReloadAnimation();

        player.weaponVisuals.CurrentWeaponModel().reloadSFX.Play();

        
    }
    public Vector3 BulletDirection()
    {

        Transform aim = player.aim.Aim();
        Vector3 direction = (aim.position - GunPoint().position).normalized;

        if (!player.aim.CanAimPrecisely())
        {
            direction.y = 0;
        }



        return direction;
    }

    public bool HasOnlyOneWeapon() => weaponSlots.Count <= 1;

    public Weapon WeaponInSlots(WeaponType weaponType)
    {
        foreach (Weapon weapon in weaponSlots)
        {
            if (weapon.weaponType == weaponType)
            {
                return weapon;
            }
        }
        return null;
    }
    public Weapon CurrentWeapon() => currentWeapon;

    public Transform GunPoint() => player.weaponVisuals.CurrentWeaponModel().gunPoint;

    private void TriggerEnemyDodge()
    {
        Vector3 rayOrigin = GunPoint().position;
        Vector3 rayDirection = BulletDirection();

        if(Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, Mathf.Infinity))
        {
            Enemy_Melee enemy_Melee = hit.collider.gameObject.GetComponentInParent<Enemy_Melee>();

            if(enemy_Melee != null)
            {
                enemy_Melee.ActivateDodgeRoll();
            }
        }
    }

    #region InputEvents
    private void AssignInputEvents()
    {
        PlayerControls controls = player.controls;

        controls.Character.Fire.performed += ctx => isShooting = true;
        controls.Character.Fire.canceled += ctx => isShooting = false;

        controls.Character.EquipSlot1.performed += ctx => EquipWeapon(0);
        controls.Character.EquipSlot2.performed += ctx => EquipWeapon(1);
        controls.Character.EquipSlot3.performed += ctx => EquipWeapon(2);
        controls.Character.EquipSlot4.performed += ctx => EquipWeapon(3);
        controls.Character.EquipSlot5.performed += ctx => EquipWeapon(4);



        controls.Character.DropCurrentWeapon.performed += ctx => DropWeapon();

        controls.Character.Reload.performed += ctx =>
        {
            if (currentWeapon.CanReload() && WeaponReady())
            {
                Reload();
            }
        };

        controls.Character.ToggleWeaponMode.performed += ctx => currentWeapon.ToggleBurst();
    }


    #endregion
}

