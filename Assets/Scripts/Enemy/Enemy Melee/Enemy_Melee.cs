using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AttackData_EnemyMelee
{
    public int attackDamage;
    public string attackName;
    public float attackRange;
    public float moveSpeed;
    public float attackIndex;
    [Range(1, 2)]
    public float animationSpeed;
    public AttackType_Melee attackType;
}
public enum AttackType_Melee { Close, Charge }
public enum EnemyMelee_Type { Regular, Shield, Dodge, AxeThrow }

public class Enemy_Melee : Enemy
{
    public Enemy_MeleeSFX meleeSFX { get; private set; }

    #region States
    public IdleState_Melee idleState { get; private set; }
    public MoveState_Melee moveState { get; private set; }
    public RecoveryState_Melee recoveryState { get; private set; }
    public ChaseState_Melee chaseState { get; private set; }
    public AttackState_Melee attackState { get; private set; }
    public DeadState_Melee deadState { get; private set; }
    public AbilityState_Melee abilityState { get; private set; }
    #endregion

    [Header("Enemy Data")]
    public EnemyMelee_Type meleeType;
    public Enemy_MeleeWeaponType weaponType;

    [Header("Shield")]
    public int shieldDurability;
    public Transform shieldTransform;
    [Space]
    [Header("Dodge")]
    public float dodgeCooldown;
    private float lastTimeDodge = -10;

    [Header("Axe Data")]
    public int axeDamage;
    public GameObject axePrefab;
    public float axeFlySpeed;
    public float axeAimTimer;
    public float axeThrowCooldown;
    public Transform axeStartPoint;
    private float lastTimeAxeThrown;

    [Header("Attack Data")]
    public AttackData_EnemyMelee attackData;
    public List<AttackData_EnemyMelee> attackList;
    private Enemy_WeaponModel currentWeapon;
    private bool isAttackReady;
    [Space]
    [SerializeField] private GameObject meleeAttackFX;



    protected override void Awake()
    {
        base.Awake();



        idleState = new IdleState_Melee(this, stateMachine, "Idle");
        moveState = new MoveState_Melee(this, stateMachine, "Move");
        recoveryState = new RecoveryState_Melee(this, stateMachine, "Recovery");
        chaseState = new ChaseState_Melee(this, stateMachine, "Chase");
        attackState = new AttackState_Melee(this, stateMachine, "Attack");
        deadState = new DeadState_Melee(this, stateMachine, "Idle"); // using ragdoll, Idle anim is placeholder
        abilityState = new AbilityState_Melee(this, stateMachine, "AxeThrow");
        meleeSFX = GetComponent<Enemy_MeleeSFX>();
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);

        InitializePerk();
        visuals.SetupLook();
        UpdateAttackData();
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();

        MeleeAttackCheck(currentWeapon.damagePoints, currentWeapon.attackRadius, meleeAttackFX, attackData.attackDamage);
    }

    public override void EnterBattleMode()
    {
        if (inBattleMode)
        {
            return;
        }

        base.EnterBattleMode();

        stateMachine.ChangeState(recoveryState);
    }

    protected override void InitializePerk()
    {
        if (meleeType == EnemyMelee_Type.AxeThrow)
        {
            weaponType = Enemy_MeleeWeaponType.Throw;
        }


        if (meleeType == EnemyMelee_Type.Shield)
        {
            if (shieldTransform != null)
            {
                anim.SetFloat("ChaseIndex", 1);
                shieldTransform.gameObject.SetActive(true);
                
                // Ensure shield renderers are visible
                Renderer[] shieldRenderers = shieldTransform.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in shieldRenderers)
                {
                    renderer.enabled = true;
                }
            }
            else
            {
                Debug.LogWarning("Shield melee type selected but shieldTransform not assigned!");
            }
            
            weaponType = Enemy_MeleeWeaponType.OneHand;
        }

        if (meleeType == EnemyMelee_Type.Dodge)
        {
            weaponType = Enemy_MeleeWeaponType.Unarmed;
        }
    }


    public override void AbilityTrigger()
    {
        base.AbilityTrigger();

        walkSpeed = walkSpeed * .6f;
        visuals.EnableWeaponModel(false);

    }
    public void UpdateAttackData()
    {
        currentWeapon = visuals.currentWeaponModel.GetComponent<Enemy_WeaponModel>();

        if (currentWeapon.weaponData != null)
        {
            attackList = new List<AttackData_EnemyMelee>(currentWeapon.weaponData.attackData);
            turnSpeed = currentWeapon.weaponData.turnSpeed;
        }
    }

    public override void Die()
    {
        base.Die();

        if (stateMachine.currentState != deadState)
        {
            stateMachine.ChangeState(deadState);
        }
    }



    public void ActivateDodgeRoll()
    {

        if (meleeType != EnemyMelee_Type.Dodge)
        {
            return;
        }

        if (stateMachine.currentState != chaseState)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) < 1f)
        {
            return;
        }

        float dodgeAnimationDuration = 1.167f;


        if (Time.time > dodgeCooldown + dodgeAnimationDuration + lastTimeDodge)
        {
            lastTimeDodge = Time.time;
            anim.SetTrigger("Dodge");
        }

    }

    public void ThrowAxe()
    {
        GameObject newAxe = ObjectPool.Instance.GetObject(axePrefab, axeStartPoint);

        newAxe.GetComponent<Enemy_Axe>().AxeSetup(axeFlySpeed, player, axeAimTimer, axeDamage);
    }
    public bool CanThrowAxe()
    {
        if (meleeType != EnemyMelee_Type.AxeThrow)
        {
            return false;
        }

        if (Time.time > lastTimeAxeThrown + axeThrowCooldown)
        {
            lastTimeAxeThrown = Time.time;
            return true;
        }

        return false;
    }

    private void ResetCooldown()
    {
        lastTimeDodge -= dodgeCooldown;
        lastTimeAxeThrown -= axeThrowCooldown;
    }

    public bool PlayerInAttackRange() => Vector3.Distance(transform.position, player.position) < attackData.attackRange;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);

    }
}
