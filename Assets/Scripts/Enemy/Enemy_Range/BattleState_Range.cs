using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleState_Range : EnemyState
{
    private Enemy_Range enemy;

    public float lastTimeShot = -10;
    private int bulletsShot = 0;
    private int bulletsPerAttack;
    private float weaponCooldown;
    private float coverCheckTimer;
    private bool firstTimeAttack = true;


    public BattleState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }

    public override void Enter()
    {
        base.Enter();
        SetupValuesForFirstAttack();

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        enemy.visuals.EnableIK(true, true);
        stateTimer = enemy.attackDelay;
    }


    public override void Update()
    {
        base.Update();

        if (enemy.IsSeeingPlayer())
        {
            enemy.FaceTarget(enemy.aim.position);
        }

        if (enemy.CanThrowGrenade())
        {
            stateMachine.ChangeState(enemy.throwGrenadeState);
        }

        if (MustAdvancePlayer())
        {
            stateMachine.ChangeState(enemy.advancePlayerState_Range);
        }


        ChangeCoverIfShould();

        if (stateTimer > 0)
        {
            return;
        }

        enemy.FaceTarget(enemy.player.position);

        if (WeaponOutOfBullets())
        {
            if (enemy.IsUnstopppable() && UnstoppableWalkReady())
            {
                enemy.advanceDuration = weaponCooldown;
                stateMachine.ChangeState(enemy.advancePlayerState_Range);
            }

            if (WeaponOnCooldown())
                AttemptToResetWeapon();


            return;
        }

        if (CanShoot() && enemy.IsAimOnPlayer())
        {
            Shoot();
        }
    }

    private bool MustAdvancePlayer()
    {
        if (enemy.IsUnstopppable())
            return false;

        return enemy.IsPlayerInAgrresionRange() == false && ReadyToLeaveCover();
    }

    private bool UnstoppableWalkReady()
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        bool outOfStoppingDistance = distanceToPlayer > enemy.advanceStoppingDistance;
        bool unstoppableWalkOnCooldown = Time.time < enemy.weaponData.minWeaponCooldown + enemy.advancePlayerState_Range.lastTimeAdvanced;

        return outOfStoppingDistance && unstoppableWalkOnCooldown == false;
    }

    #region Cover System Region
    private bool ReadyToLeaveCover()
    {
        return Time.time > enemy.minCoverTime + enemy.runToCoverState.lastTimeTookCover;
    }


    private void ChangeCoverIfShould()
    {
        if (enemy.coverPerk != CoverPerk.CanTakeAndChangeCover)
            return;

        coverCheckTimer -= Time.deltaTime;

        if (coverCheckTimer < 0)
        {
            coverCheckTimer = .5f; // We do cover check each .5f seconds

            if (ReadyToChangeCover() && ReadyToLeaveCover())
            {
                if (enemy.CanGetCover())
                    stateMachine.ChangeState(enemy.runToCoverState);
            }
        }
    }

    private bool ReadyToChangeCover()
    {
        bool inDanger = IsPlayerInClearSight() || IsPlayerClose();
        bool advanceTimeIsOver = Time.time > enemy.advancePlayerState_Range.lastTimeAdvanced + enemy.advanceDuration;

        return inDanger && advanceTimeIsOver;
    }




    private bool IsPlayerClose()
    {
        return Vector3.Distance(enemy.transform.position, enemy.player.transform.position) < enemy.safeDistance;
    }

    private bool IsPlayerInClearSight()
    {
        Vector3 directionToPlayer = enemy.player.transform.position - enemy.transform.position;


        if (Physics.Raycast(enemy.transform.position, directionToPlayer, out RaycastHit hit))
        {
            if(hit.transform.root == enemy.player.root)
            return true;
        }

        return false;
    }

    #endregion



    #region Weapon Region
    private void AttemptToResetWeapon()
    {
        bulletsShot = 0;

        bulletsPerAttack = enemy.weaponData.GetBulletsPerAttack();
        weaponCooldown = enemy.weaponData.GetWeaponCooldown();
    }
    private bool WeaponOnCooldown() => Time.time > lastTimeShot + weaponCooldown;
    private bool WeaponOutOfBullets() => bulletsShot >= bulletsPerAttack;
    private bool CanShoot() => Time.time > lastTimeShot + 1 / enemy.weaponData.fireRate;

    private void Shoot()
    {
        enemy.FireSingleBullet();
        lastTimeShot = Time.time;
        bulletsShot++;
    }

    private void SetupValuesForFirstAttack()
    {

        enemy.aggresionRange = enemy.advanceStoppingDistance +2;




        if (firstTimeAttack)
        {
            firstTimeAttack = false;
            bulletsPerAttack = enemy.weaponData.GetBulletsPerAttack();
            weaponCooldown = enemy.weaponData.GetWeaponCooldown();
        }
    }

    #endregion
}
