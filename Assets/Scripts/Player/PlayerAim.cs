using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAim : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;


    [Header("Aim Visual - Laser")]
    [SerializeField] private LineRenderer aimLaser;

    [Header("Aim Control")]

    [SerializeField] private Transform aim;

    [SerializeField] private bool isAimingPrecisely;

    [SerializeField] private bool isLockingToTarget;

    [SerializeField] private float offsetChangeRate = 6;
    private float offsetY;

    [Header("Camera control")]
    [Range(0.5f, 1f)]
    [SerializeField] private float minCameraDistance = 1.5f;
    [SerializeField] private Transform cameraTarget;
    [Range(1f, 3f)]
    [SerializeField] private float maxCameraDistance = 4;

    [Range(3f, 5f)]
    [SerializeField] private float cameraSensitivity = 5f;

    [Space]

    [SerializeField] private LayerMask aimLayerMask;
    private Vector2 mouseInput;

    private RaycastHit lastKnownMouseHit;

    private void Start()
    {
        player = GetComponent<Player>();
        AssignInputEvents();

        Cursor.visible = false;
    }
    private void Update()
    {

        if(player.health.isDead)
        {
            return;
        }

        if(player.controlsEnabled == false)
        {
            return;
        }
        
        UpdateAimPosition();
        UpdateCameraPosition();
        UpdateAimVisuals();
    }

    private void EnablePreciseAim(bool enable)
    {
       isAimingPrecisely = enable;
       Cursor.visible = false;
    }

    public Transform GetAimCameraTarget() => cameraTarget;

    public void EnableAimLaser(bool enable)
    {
        aimLaser.enabled = enable;
    }

    private void UpdateAimVisuals()
    {
        aim.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        aimLaser.enabled = player.weapon.WeaponReady();

        if(aimLaser.enabled == false)
        {
            return;
        }

        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();

        weaponModel.transform.LookAt(aim);
        weaponModel.gunPoint.LookAt(aim);


        float laserTipLength = .5f;
        Transform gunPoint = player.weapon.GunPoint();
        Vector3 laserDirection = player.weapon.BulletDirection();
        float gunDistance = player.weapon.CurrentWeapon().gunDistance;

        Vector3 endPoint = gunPoint.position + laserDirection * gunDistance;

        if (Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hit, gunDistance))
        {
            endPoint = hit.point;
            laserTipLength = 0;
        }

        aimLaser.SetPosition(0, gunPoint.position);
        aimLaser.SetPosition(1, endPoint);
        aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLength);

    }

    

    private void UpdateCameraPosition()
    {
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, DesiredCameraPosition(), cameraSensitivity * Time.deltaTime);
    }

    public Transform Aim() => aim;

    public bool CanAimPrecisely() => isAimingPrecisely;

    public RaycastHit GetMouseHitInfo()
    {
        Ray ray = Camera.main.ScreenPointToRay(mouseInput);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
        {
            lastKnownMouseHit = hitInfo;
            return hitInfo;
        }
        return lastKnownMouseHit;
    }

    #region Camera Region
    private Vector3 DesiredCameraPosition()
    {
        // float actualMaxCameraDistance;
        // bool movingDownwards = player.movement.moveInput.y < - 0.5f; 

        // if(movingDownwards)
        // {
        //     actualMaxCameraDistance = minCameraDistance;
        // }
        // else
        // {
        //     actualMaxCameraDistance = maxCameraDistance;
        // }

        float actualMaxCameraDistance = player.movement.moveInput.y < -0.5f ? minCameraDistance : maxCameraDistance;



        Vector3 desiredCameraPosition = GetMouseHitInfo().point;
        Vector3 aimDirection = (desiredCameraPosition - transform.position).normalized;

        float distanceToDesiredPosition = Vector3.Distance(transform.position, desiredCameraPosition);
        float clampedDistance = Mathf.Clamp(distanceToDesiredPosition, minCameraDistance, actualMaxCameraDistance);




        desiredCameraPosition = transform.position + aimDirection * clampedDistance;
        desiredCameraPosition.y = transform.position.y + 1;

        return desiredCameraPosition;
    }

    private void UpdateAimPosition()
    {
        aim.position = GetMouseHitInfo().point;

        Vector3 newAimPosition = isAimingPrecisely ? aim.position : transform.position;

        aim.position = new Vector3(aim.position.x, newAimPosition.y + AdjustedOffsetY(), aim.position.z);

        
    }

    private float AdjustedOffsetY()
    {
        if(isAimingPrecisely)
        {
            offsetY = Mathf.Lerp(offsetY, 0, Time.deltaTime * offsetChangeRate * .5f);
        }
        else
        {
            offsetY = Mathf.Lerp(offsetY, 1, Time.deltaTime * offsetChangeRate);
        }

        return offsetY;
    }

    #endregion
    private void AssignInputEvents()
    {
        controls = player.controls;

        controls.Character.PreciseAim.performed += ctx => EnablePreciseAim(true);
        controls.Character.PreciseAim.canceled += ctx => EnablePreciseAim(false);

        controls.Character.Aim.performed += context => mouseInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => mouseInput = Vector2.zero;

    }
}
