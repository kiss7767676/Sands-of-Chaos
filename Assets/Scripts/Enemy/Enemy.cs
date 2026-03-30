using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyType {Melee, Range, Boss, Random}

[RequireComponent(typeof(NavMeshAgent))]

public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public LayerMask whatIsAlly;
    public LayerMask whatIsPlayer;
    [Space]


    [Header("Idle data")]
    public float idleTime;
    public float aggresionRange;

    [Header("Attack data")]

    // public float attackRange;

    [Header("Move data")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3;
    public float turnSpeed;
    private bool manualMovement;
    private bool manualRotation;

    [SerializeField] private Transform[] patrolPoints;
    private Vector3[] patrolPointsPosition;
    private int currentPatrolIndex;

    public bool inBattleMode { get; private set; }
    protected bool isMeleeAttackReady;


    public Transform player { get; private set; }
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public EnemyStateMachine stateMachine { get; private set; }
    public Enemy_Visuals visuals { get; private set; }
    public Enemy_Ragdoll ragdoll { get; private set; }
    public Enemy_Health health { get; private set; }
    
    public Enemy_DropController dropController { get; private set; }
    public AudioManager audioManager { get; private set; }


    protected virtual void Awake()
    {

        health = GetComponent<Enemy_Health>();
        stateMachine = new EnemyStateMachine();
        ragdoll = GetComponent<Enemy_Ragdoll>();
        visuals = GetComponent<Enemy_Visuals>();
        dropController = GetComponent<Enemy_DropController>();

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        player = GameObject.Find("Player").GetComponent<Transform>();
    }

    protected virtual void Start()
    {
        InitializePatrolPoints();
        audioManager = AudioManager.instance;
    }



    protected virtual void Update()
    {
        if (ShouldEnterBattleMode())
        {
            EnterBattleMode();
        }
    }

    protected virtual void InitializePerk()
    {

    }

    public virtual void MakeEnemyVIP()
    {
        int additionalHealth = Mathf.RoundToInt(health.currentHealth * 1.5f);

        health.currentHealth += additionalHealth;

        transform.localScale = transform.localScale * 1.15f;
    }

    protected bool ShouldEnterBattleMode()
    {

        if (IsPlayerInAgrresionRange() && !inBattleMode)
        {
            EnterBattleMode();
            return true;
        }

        return false;
    }


    public virtual void EnterBattleMode()
    {
        inBattleMode = true;
    }

    public virtual void GetHit(int damage)
    {
        EnterBattleMode();
        health.ReduceHealth(damage);

        if(health.ShouldDie())
        {
            Die();
        }
    }

    public virtual void Die()
    {
        dropController.DropItems();

        anim.enabled = false;
        agent.isStopped = true;
        agent.enabled = false;

        ragdoll.RagdollActive(true);

        MissionObject_HuntTarget huntTarget = GetComponent<MissionObject_HuntTarget>();
        huntTarget?.InvokeOnTargetKilled();
    }

    public void EnableMeleeAttackCheck(bool enable) => isMeleeAttackReady = enable;

    public virtual void MeleeAttackCheck(Transform[] damagePoints, float attackCheckRadius, GameObject fx, int damage)
    {
        if (isMeleeAttackReady == false)
        {
            return;
        }

        foreach (Transform attackPoints in damagePoints)
        {
            Collider[] detectedHits = Physics.OverlapSphere(attackPoints.position, attackCheckRadius, whatIsPlayer);

            for (int i = 0; i < detectedHits.Length; i++)
            {
                IDamagable damagable = detectedHits[i].GetComponent<IDamagable>();

                if (damagable != null)
                {
                    damagable.TakeDamage(damage);
                    isMeleeAttackReady = false;
                    GameObject newAttackFx = ObjectPool.Instance.GetObject(fx, attackPoints);

                    ObjectPool.Instance.ReturnObject(newAttackFx, 1);
                    return;
                }
            }
        }
    }

    public virtual void BulletImpact(Vector3 force, Vector3 hitPoint, Rigidbody rb)
    {
        if(health.ShouldDie())
        {
            StartCoroutine(DeathImpactCoroutine(force, hitPoint, rb));
        }
    }

    private IEnumerator DeathImpactCoroutine(Vector3 force, Vector3 hitPoint, Rigidbody rb)
    {
        yield return new WaitForSeconds(.1f);

        rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
    }

    public void FaceTarget(Vector3 target, float turnSpeed = 0)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);

        Vector3 currentEulerAngels = transform.rotation.eulerAngles;

        if(turnSpeed == 0)
        {
            turnSpeed = this.turnSpeed;
        }

        float yRotation = Mathf.LerpAngle(currentEulerAngels.y, targetRotation.eulerAngles.y, turnSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(currentEulerAngels.x, yRotation, currentEulerAngels.z);
    }

    #region Animation Events
    public void ActivateManualMovement(bool manualMovement) => this.manualMovement = manualMovement;
    public bool ManualMovementActive() => manualMovement;

    public void ActivateManualRotation(bool manualRotation) => this.manualRotation = manualRotation;
    public bool ManualRotationActive() => manualRotation;
    public void AnimationTrigger() => stateMachine.currentState.AnimationTrigger();

    public virtual void AbilityTrigger()
    {
        stateMachine.currentState.AbilityTrigger();
    }
    #endregion

    #region Patrol Logic
    public Vector3 GetPatrolDestination()
    {
        Vector3 destination = patrolPointsPosition[currentPatrolIndex];

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;

        return destination;
    }
    private void InitializePatrolPoints()
    {
        patrolPointsPosition = new Vector3[patrolPoints.Length];

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            patrolPointsPosition[i] = patrolPoints[i].position;
            patrolPoints[i].gameObject.SetActive(false);
        }
    }
    #endregion

    public bool IsPlayerInAgrresionRange() => Vector3.Distance(transform.position, player.position) < aggresionRange;
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, aggresionRange);
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
