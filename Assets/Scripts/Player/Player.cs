using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Transform playerBody;

    public PlayerAim aim { get; private set;} //read - only
    public PlayerControls controls { get; private set;}

    public PlayerMovement movement { get; private set;}

    public PlayerWeaponController weapon { get; private set; }

    public WeaponVisualController weaponVisuals { get; private set; }

    public PlayerInteraction interaction { get; private set; }
    public Player_Health health;
    public Enemy_Ragdoll ragdoll{ get; private set; }
    public Animator anim{ get; private set; }

    public bool controlsEnabled{ get; private set; }

    public Player_SFX sound { get; private set; }


    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        ragdoll = GetComponent<Enemy_Ragdoll>();
        health = GetComponent<Player_Health>();
        aim = GetComponent<PlayerAim>();
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponent<PlayerWeaponController>();
        weaponVisuals = GetComponent<WeaponVisualController>();
        interaction = GetComponent<PlayerInteraction>();
        controls = ControlsManager.instance.controls;
        sound = GetComponent<Player_SFX>();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Character.UIMissionToolTipSwitch.performed += ctx => UI.instance.inGameUI.SwitchMissionTooltip();
        controls.Character.UI_Pause.performed += ctx => UI.instance.PauseSwitch();
    }
    private void OnDisable()
    {
        controls.Disable();
    }

    public void SetControlsEnabledTo(bool enabled)
    {
        controlsEnabled = enabled;
        ragdoll.ColliderActive(enabled);
        aim.EnableAimLaser(enabled);
    }
}