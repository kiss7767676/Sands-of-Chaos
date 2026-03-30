using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationEvents : MonoBehaviour
{
    private Enemy enemy;
    private Enemy_Boss enemy_Boss;
    private Enemy_Melee enemy_Melee;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        enemy_Melee = GetComponentInParent<Enemy_Melee>();
        enemy_Boss =  GetComponentInParent<Enemy_Boss>();
    }

    public void AnimationTrigger() => enemy.AnimationTrigger();

    public void StartManualMovement() => enemy.ActivateManualMovement(true);
    public void StopManualMovement() => enemy.ActivateManualMovement(false);

    public void StartManualRotation() => enemy.ActivateManualRotation(true);
    public void StopManualRotation() => enemy.ActivateManualRotation(false);

    public void AbilityEvent() => enemy.AbilityTrigger();

    public void EnableIK() => enemy.visuals.EnableIK(true, true, 1f);

    public void EnableWeaponModel()
    {
        enemy.visuals.EnableWeaponModel(true);
        enemy.visuals.EnableSeconoderyWeaponModel(false);
    }

    public void BossJumpImpact()
    {
        if(enemy_Boss == null)
        {
            enemy_Boss = GetComponentInParent<Enemy_Boss>();
        }

        enemy_Boss?.JumpImpact();
    }

    public void BeginMeleeAttackCheck()
    {
        enemy?.EnableMeleeAttackCheck(true);

        enemy?.audioManager.PlaySFX(enemy_Melee?.meleeSFX.swoosh, true);//, .9f, 1.1f);
    }

    public void FinishMeleeAttackCheck()
    {
        enemy?.EnableMeleeAttackCheck(false);
    }
    

}
