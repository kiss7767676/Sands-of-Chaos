using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Enemy_Ragdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollParent;

    [SerializeField] private Collider[] ragdollColliders;
    [SerializeField] private Rigidbody[] ragdollRigidbodies;

    private void Awake()
    {
        ragdollColliders = ragdollParent.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = ragdollParent.GetComponentsInChildren<Rigidbody>();

        RagdollActive(false);

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public void RagdollActive(bool active)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !active;
        }
    }

    public void ColliderActive(bool active)
    {
        foreach(Collider cd in ragdollColliders)
        {
            cd.enabled = active;
        }
    }

}
