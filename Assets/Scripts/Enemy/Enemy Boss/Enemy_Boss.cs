using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BossWeaponType { Flamethrower, Hammer }

public class Enemy_Boss : Enemy
{
    public GameObject BombfxPrefab;

    [Header("Boss details")]
    public BossWeaponType bossWeaponType;
    public float actionCooldown;
    public float attackRange;

    [Header("Ability")]
    public float minAbilityDistance;
    public float abilityCooldown;
    private float lastTimeUsedAbility;

    [Header("Flamethrower")]
    public int flameDamage;
    public float flameDamageCooldown;
    public ParticleSystem flamethrower;
    public float flamethrowDuration;
    public bool flamethrowActive { get; private set; }

    [Header("Hammer")]
    public int hammerActiveDamage;
    public GameObject activationPrefab;
    [SerializeField] private float hammerCheckRadius;


    [Header("Jump attack")]
    public int jumpAttackDamage;
    public float jumpAttackCooldown = 10;
    private float lastTimeJumped;
    public float travelTimeToTarget = 1;
    public float minJumpDistanceRequired;
    [Space]
    public float impactRadius = 2.5f;
    public float impactPower = 5;
    public Transform impactPoint;
    [SerializeField] private float upforceMultiplier = 10;
    [Space]
    [SerializeField] private LayerMask whatToIngore;

    [Header("Attack")]
    [SerializeField] private int meleeAttackDamage;
    [SerializeField] private Transform[] damagePoints;
    [SerializeField] private float attackCheckRadius;
    [SerializeField] private GameObject meleeAttackFx;


    #region States
    public IdleState_Boss idleState { get; private set; }
    public MoveState_Boss moveState { get; private set; }
    public AttackState_Boss attackState { get; private set; }
    public JumpAttackState_Boss jumpAttackState { get; private set; }
    public AbilityState_Boss abilityState { get; private set; }
    public DeadState_Boss deadState { get; private set; }
    #endregion

    public Enemy_BossVisuals bossVisuals { get; private set; }
    [SerializeField] private float offset = 1;

    protected override void Awake()
    {
        base.Awake();

        bossVisuals = GetComponent<Enemy_BossVisuals>();

        idleState = new IdleState_Boss(this, stateMachine, "Idle");
        moveState = new MoveState_Boss(this, stateMachine, "Move");
        attackState = new AttackState_Boss(this, stateMachine, "Attack");
        jumpAttackState = new JumpAttackState_Boss(this, stateMachine, "JumpAttack");
        abilityState = new AbilityState_Boss(this, stateMachine, "Ability");
        deadState = new DeadState_Boss(this, stateMachine, "Idle"); // Idle is just a placeholder we use ragdoll
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();


        stateMachine.currentState.Update();

        if (ShouldEnterBattleMode())
            EnterBattleMode();

        MeleeAttackCheck(damagePoints, attackCheckRadius, meleeAttackFx, meleeAttackDamage);
    }



    public override void Die()
    {
        base.Die();

        if (stateMachine.currentState != deadState)
        {
            stateMachine.ChangeState(deadState);
        }
            
    }

    public override void EnterBattleMode()
    {
        if (inBattleMode)
            return;

        base.EnterBattleMode();
        stateMachine.ChangeState(moveState);
    }


    public void ActivateFlamethrower(bool activate)
    {
        flamethrowActive = activate;

        if (!activate)
        {
            flamethrower.Stop();
            anim.SetTrigger("StopFlameThrower");
            Debug.Log("flame stopped");
            return;
        }

        var mainModule = flamethrower.main;
        var extraModule = flamethrower.transform.GetChild(0).GetComponent<ParticleSystem>().main;

        mainModule.duration = flamethrowDuration;
        extraModule.duration = flamethrowDuration;

        flamethrower.Clear();
        flamethrower.Play();
    }

    public void ActivateHummer()
    {
        GameObject newActivation = ObjectPool.Instance.GetObject(activationPrefab, impactPoint);
        ObjectPool.Instance.ReturnObject(newActivation, 1);

        MassDamage(damagePoints[0].position, hammerCheckRadius, hammerActiveDamage);
    }


    public bool CanDoAbility()
    {
        bool playerWithinDistance = Vector3.Distance(transform.position, player.position) < minAbilityDistance;

        if (playerWithinDistance == false)
            return false;

        if (Time.time > lastTimeUsedAbility + abilityCooldown)
        {
            return true;
        }

        return false;
    }

    public void SetAbilityOnCooldown() => lastTimeUsedAbility = Time.time;

    public void JumpImpact()
    {
        Transform impactPoint = this.impactPoint;

        if (impactPoint == null)
            impactPoint = transform;

        MassDamage(impactPoint.position, impactRadius, jumpAttackDamage);
    }

    private void MassDamage(Vector3 impactPoint, float impactRadius, int damage)
    {
        HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();

        Collider[] colliders = Physics.OverlapSphere(impactPoint, impactRadius, ~whatIsAlly);

        foreach (Collider hit in colliders)
        {

            IDamagable damagable = hit.GetComponent<IDamagable>();

            if(damagable != null)
            {
                
                GameObject rootEntity = hit.transform.root.gameObject;

                if (uniqueEntities.Add(rootEntity) == false)
                    continue;

                Debug.Log(hit.transform.root.name + "boss damaged");

                damagable.TakeDamage(damage);
            }

            ApplyPhysicalForceTo(impactPoint, impactRadius, hit);

        }
    }

    private void ApplyPhysicalForceTo(Vector3 impactPoint, float impactRadius, Collider hit)
    {
        Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                GameObject newbomb = ObjectPool.Instance.GetObject(BombfxPrefab, this.impactPoint);
                ObjectPool.Instance.ReturnObject(newbomb, 2);
                rb.AddExplosionForce(impactPower, impactPoint, impactRadius, upforceMultiplier, ForceMode.Impulse);
            }
    }

    public bool CanDoJumpAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < minJumpDistanceRequired)
            return false;

        if (Time.time > lastTimeJumped + jumpAttackCooldown && IsPlayerInClearSight())
        {
            return true;
        }



        return false;


    }

    public void SetJumpAttackOnCooldown() => lastTimeJumped = Time.time;

    public bool IsPlayerInClearSight()
    {
        Vector3 myPos = transform.position + new Vector3(0, 1.5f, 0);
        Vector3 playerPos = player.position + (Vector3.up * offset);
        Vector3 directionToPlayer = (playerPos - myPos).normalized;

        if (Physics.Raycast(myPos, directionToPlayer, out RaycastHit hit, 100, ~whatToIngore))
        {
            if (hit.transform.root == player.root)
                return true;
        }

        return false;
    }

    public bool PlayerInAttackRange() => Vector3.Distance(transform.position, player.position) < attackRange;


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (player != null)
        {
            Vector3 myPos = transform.position + new Vector3(0, 1.5f, 0);
            Vector3 playerPos = player.position + Vector3.up;

            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(myPos, playerPos);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minAbilityDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minJumpDistanceRequired);

        if(damagePoints.Length > 0)
        {
            foreach( var damagePoint in damagePoints)
            {
                Gizmos.DrawWireSphere(damagePoint.position, attackCheckRadius);
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(damagePoints[0].position, hammerCheckRadius);
        }
        
    }
}
