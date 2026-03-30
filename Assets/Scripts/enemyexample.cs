using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class enemyexample : MonoBehaviour
{
   public NavMeshAgent agent;
   public Transform target;

   private void Update()
   {
       agent.destination = target.position;
   }
}
