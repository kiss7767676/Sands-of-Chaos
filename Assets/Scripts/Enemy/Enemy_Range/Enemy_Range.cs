using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public enum CoverPerk { Unavalible, CanTakeCover, CanTakeAndChangeCover }
public enum UnstoppablePerk { Unavalible, Unstoppable }
public enum GrenadePerk { Unavalible, CanThrowGrenade }


public class Enemy_Range : Enemy
{

    [Header("Enemy perks")]
    public CoverPerk coverPerk;
    public UnstoppablePerk unstoppablePerk;
    public GrenadePerk grenadePerk;
    public Enemy_RangeWeaponType weaponType;


    [Header("Grenade perk")]
    public int grenadeDamage;
    public GameObject grenadePrefab;
    public float impactPower;
    public float explosionTimer = .75f;
    public float timeToTarget = 1.2f;
    public float grenadeCooldown;
    private float lastTimeGrenadeThrown = -10;
    [SerializeField] private Transform grenadeStartPoint;


    [Header("Advance perk")]
    public float advanceSpeed;
    public float advanceStoppingDistance;
    public float advanceDuration = 2.5f;




    [Header("Cover System")]
    public float minCoverTime;
    public float safeDistance;
    public CoverPoint lastCover { get; private set; }
    public CoverPoint currentCover { get; private set; }


    [Header("Weapon Details")]
    public float attackDelay;
    public Enemy_RangeWeaponData weaponData;
    [Space]
    public Transform gunPoint;
    public Transform weaponHolder;
    public GameObject bulletPrefab;
    [SerializeField] public float aimHeightOffset = -0.9f;

    [SerializeField] List<Enemy_RangeWeaponData> availableWeaponData;

    [Header("Aim Details")]
    public float slowAim = 4;
    public float fastAim = 20;
    public Transform aim;
    public Transform playersBody;
    public LayerMask whatToIgnore;



    #region States 
    public IdleState_Range idleState { get; private set; }
    public MoveState_Range moveState { get; private set; }
    public BattleState_Range battleState { get; private set; }
    public RunToCoverState_Range runToCoverState { get; private set; }
    public AdvancePlayerState_Range advancePlayerState_Range { get; private set; }
    public ThrowGrenadeState_Range throwGrenadeState { get; private set; }
    public DeadState_Range deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        idleState = new IdleState_Range(this, stateMachine, "Idle");
        moveState = new MoveState_Range(this, stateMachine, "Move");
        battleState = new BattleState_Range(this, stateMachine, "Battle");
        runToCoverState = new RunToCoverState_Range(this, stateMachine, "Run");
        advancePlayerState_Range = new AdvancePlayerState_Range(this, stateMachine, "Advance");
        throwGrenadeState = new ThrowGrenadeState_Range(this, stateMachine, "ThrowGrenade");
        deadState = new DeadState_Range(this, stateMachine, "Idle");

    }


    protected override void Start()
    {
        base.Start();

        playersBody = player.GetComponent<Player>().playerBody;
        aim.parent = null;

        InitializePerk();

        stateMachine.Initialize(idleState);
        visuals.SetupLook();
        SetupWeapon();

    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();
    }

    public override void Die()
    {
        base.Die();

        if (stateMachine.currentState != deadState)
        {
            stateMachine.ChangeState(deadState);
        }
    }


    public void FireSingleBullet()
    {
        anim.SetTrigger("Shoot");
        Vector3 bulletDirection = ((aim.position + Vector3.up * aimHeightOffset) - gunPoint.position).normalized;

        GameObject newBullet = ObjectPool.Instance.GetObject(bulletPrefab, gunPoint);

        newBullet.transform.rotation = Quaternion.LookRotation(gunPoint.forward);

        newBullet.GetComponent<Bullet>().BulletSetup(whatIsAlly, weaponData.bulletDamage);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        Vector3 bulletDirectionWithSpread = weaponData.ApplyWeaponSpread(bulletDirection);

        rbNewBullet.mass = 20 / weaponData.bulletSpeed;
        rbNewBullet.linearVelocity = bulletDirectionWithSpread * weaponData.bulletSpeed;
    }

    #region Grenade Logic
    public bool CanThrowGrenade()
    {
        if (grenadePerk == GrenadePerk.Unavalible)
            return false;

        if (Vector3.Distance(player.transform.position, transform.position) < safeDistance)
            return false;

        if (Time.time > grenadeCooldown + lastTimeGrenadeThrown)
            return true;

        return false;
    }

    public void ThrowGrenade()
    {
        lastTimeGrenadeThrown = Time.time;
        Debug.Log("Throwing");
        visuals.EnableGrenadeModel(false);

        GameObject newGrenade = ObjectPool.Instance.GetObject(grenadePrefab, grenadeStartPoint);
        newGrenade.transform.position = grenadeStartPoint.transform.position;
        Enemy_Grenade newGrenadeScript = newGrenade.GetComponent<Enemy_Grenade>();

        if (stateMachine.currentState == deadState)
        {
            newGrenadeScript.SetupGrenade(whatIsAlly, transform.position, 1, explosionTimer, impactPower, grenadeDamage);
            return;
        }

        newGrenadeScript.SetupGrenade(whatIsAlly, player.transform.position, timeToTarget, explosionTimer, impactPower, grenadeDamage);
    }
    #endregion

    protected override void InitializePerk()
    {
        if (IsUnstopppable())
        {
            advanceSpeed = 1;
            anim.SetFloat("AdvanceAnimIndex", 1);
        }

        if (weaponType == Enemy_RangeWeaponType.Random)
        {
            ChooseRandomWeaponType();
        }

    }

    private void ChooseRandomWeaponType()
    {
        List<Enemy_RangeWeaponType> validTypes = new List<Enemy_RangeWeaponType>();

        foreach (Enemy_RangeWeaponType value in Enum.GetValues(typeof(Enemy_RangeWeaponType)))
        {
            if (value != Enemy_RangeWeaponType.Random && value != Enemy_RangeWeaponType.Rifle)
            {
                validTypes.Add(value);
            }
        }

        int randomIndex = UnityEngine.Random.Range(0, validTypes.Count);

        weaponType = validTypes[randomIndex];
    }

    public override void EnterBattleMode()
    {
        if (inBattleMode)
        {
            return;
        }

        base.EnterBattleMode();

        if (CanGetCover())
        {
            stateMachine.ChangeState(runToCoverState);
        }
        else
        {
            stateMachine.ChangeState(battleState);
        }

    }

    private void SetupWeapon()
    {
        List<Enemy_RangeWeaponData> filteredData = new List<Enemy_RangeWeaponData>();

        foreach (var weaponData in availableWeaponData)
        {
            if (weaponData.weaponType == weaponType)
            {
                filteredData.Add(weaponData);
            }
        }

        if (filteredData.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, filteredData.Count);
            weaponData = filteredData[random];
        }
        else
        {
            Debug.LogWarning("No available data was found");
        }

        gunPoint = visuals.currentWeaponModel.GetComponent<Enemy_RangeWeaponModel>().gunPoint;

    }

    #region Cover System

    public bool CanGetCover()
    {

        if (coverPerk == CoverPerk.Unavalible)
            return false;

        currentCover = AttemptToFindCover()?.GetComponent<CoverPoint>();

        if (lastCover != currentCover && currentCover != null)
            return true;

        Debug.LogWarning("No cover found!");
        return false;
    }

    private Transform AttemptToFindCover()
    {
        List<CoverPoint> collectedCoverPoints = new List<CoverPoint>();

        foreach (Cover cover in CollectNearByCovers())
        {
            collectedCoverPoints.AddRange(cover.GetValidCoverPoints(transform));
        }

        CoverPoint closestCoverPoint = null;
        float shortestDistance = float.MaxValue;

        foreach (CoverPoint coverPoint in collectedCoverPoints)
        {
            float currentDistance = Vector3.Distance(transform.position, coverPoint.transform.position);
            if (currentDistance < shortestDistance)
            {
                closestCoverPoint = coverPoint;
                shortestDistance = currentDistance;
            }
        }

        if (closestCoverPoint != null)
        {
            lastCover?.SetOccupied(false);
            lastCover = currentCover;

            currentCover = closestCoverPoint;
            currentCover.SetOccupied(true);

            return currentCover.transform;
        }


        return null;
    }

    private List<Cover> CollectNearByCovers()
    {
        float coverRadiusCheck = 30;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, coverRadiusCheck);
        List<Cover> collectedCovers = new List<Cover>();

        foreach (Collider collider in hitColliders)
        {
            Cover cover = collider.GetComponent<Cover>();

            if (cover != null && collectedCovers.Contains(cover) == false)
                collectedCovers.Add(cover);
        }

        return collectedCovers;
    }


    #endregion






    #region Enemy Aim Region

    public void UpdateAimPosition()
    {
        float aimSpeed = IsAimOnPlayer() ? fastAim : slowAim;
        aim.position = Vector3.MoveTowards(aim.position, playersBody.position, aimSpeed * Time.deltaTime);
    }


    public bool IsAimOnPlayer()
    {
        float distnaceAimToPlayer = Vector3.Distance(aim.position, player.position);

        return distnaceAimToPlayer < 2;
    }


    public bool IsSeeingPlayer()
    {
        Vector3 myPosition = transform.position + (Vector3.up * aimHeightOffset);
        Vector3 directionToPlayer = playersBody.position - myPosition;

        if (Physics.Raycast(myPosition, directionToPlayer, out RaycastHit hit, Mathf.Infinity, ~whatToIgnore))
        {
            if (hit.transform.root == player.root)
            {
                UpdateAimPosition();
                return true;
            }
        }

        return false;
    }

    #endregion


    public bool IsUnstopppable() => unstoppablePerk == UnstoppablePerk.Unstoppable;

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, advanceStoppingDistance);
    }
}
