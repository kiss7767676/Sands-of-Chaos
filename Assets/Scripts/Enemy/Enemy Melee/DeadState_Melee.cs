using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeadState_Melee : EnemyState
{

    private Enemy_Melee enemy;
    private bool interactionDisabled;


    public DeadState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Melee;
        
    }

    public override void Enter()
    {
        base.Enter();

        interactionDisabled = false;
    
        stateTimer = 1.5f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // if want to disable interaction
        //DisableInteractionIfShould();
        
    }

    private void DisableInteractionIfShould()
    {
        if (stateTimer < 0 && interactionDisabled == false)
        {
            interactionDisabled = true;
            enemy.ragdoll.RagdollActive(false);
            enemy.ragdoll.ColliderActive(false);
        }
    }

}
