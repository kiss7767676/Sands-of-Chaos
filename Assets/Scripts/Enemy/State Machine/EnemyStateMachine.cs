using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyStateMachine
{
    public EnemyState currentState { get; private set; }    

    public void Initialize(EnemyState startState)
    {
        currentState = startState;
        currentState.Enter();
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        if (currentState != null)
        {
            currentState.Enter();
        }
    }
}