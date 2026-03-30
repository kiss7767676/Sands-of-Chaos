using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Interaction : Interactable
{
    private Car_HealthController carHealthController;
    private Car_Controller carController;
    private Transform player;

    private float defaultPlayerScale;
    public PlayerAim aim{ get; private set;}
    private GameObject carInteraction;


    [Header("Exit details")]
    [SerializeField] private float exitCheckRadius = .2f;
    [SerializeField] private Transform[] exitPoints;
    [SerializeField] private LayerMask whatToIngoreForExit;

    private void Awake()
    {
        aim = GetComponent<PlayerAim>();
    }

    private void Start()
    {
        carHealthController = GetComponent<Car_HealthController>();
        carController = GetComponent<Car_Controller>();
        player = GameManager.instance.player.transform;
        
        

        foreach (var point in exitPoints)
        {
            point.GetComponent<MeshRenderer>().enabled = false;
            point.GetComponent<SphereCollider>().enabled = false;
        }

    }

    public override void Interaction()
    {
        base.Interaction();
        GetIntoTheCar();
    }

    private void GetIntoTheCar()
    {
        ControlsManager.instance.SwitchToCarControls();
        carHealthController.UpdateCarHealthUI();
        carController.ActivateCar(true);

        defaultPlayerScale = player.localScale.x;

        player.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        Collider[] playerColliders = player.GetComponentsInChildren<Collider>();

        if (playerRb != null)
            playerRb.isKinematic = true;

        foreach (var col in playerColliders)
            col.enabled = false;

        player.SetParent(transform);
        player.localPosition = Vector3.up * 0.5f;
    }


    public void GetOutOfTheCar()
    {
        if (!carController.carActive)
            return;

        carController.ActivateCar(false);

        // Unparent player
        player.SetParent(null);

        player.transform.localScale = new Vector3(defaultPlayerScale,defaultPlayerScale,defaultPlayerScale);

        // Move player to exit point
        player.position = GetExitPoint();

        // Restore scale
        player.localScale = Vector3.one * defaultPlayerScale;

        // Restore player physics
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        Collider[] playerColliders = player.GetComponentsInChildren<Collider>();

        if (playerRb != null)
            playerRb.isKinematic = false;

        foreach (var col in playerColliders)
            col.enabled = true;

        // Switch controls back to character
        ControlsManager.instance.SwitchToCharacterControls();
        aim.GetAimCameraTarget();
    }


        private Vector3 GetExitPoint()
        {
           for (int i = 0; i < exitPoints.Length; i++)
            {
                if (IsExitClear(exitPoints[i].position))
                    return exitPoints[i].position;
            }

            return exitPoints[0].position;
        }

        private bool IsExitClear(Vector3 point)
        {
            Collider[] colliders = Physics.OverlapSphere(point, exitCheckRadius, ~whatToIngoreForExit);
            return colliders.Length == 0;
        }

        private void OnDrawGizmos()
    {
        if (exitPoints.Length > 0)
        {
            foreach (var point in exitPoints)
            {
                Gizmos.DrawWireSphere(point.position, exitCheckRadius);
            }
        }
    }
}
