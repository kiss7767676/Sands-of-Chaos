using System.Diagnostics;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponVisualController : MonoBehaviour
{

    private Player player;
    private Animator anim;


    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private BackupWeaponModel[] backupWeaponModels;

    [Header("Rig")]
    [SerializeField] private float rigWeightIcreaseRate;
    private bool ShouldIncreaseRig_Weight;
    private Rig rig;

    [Header("Left Hand IK")]
    [SerializeField] private Transform lefthandIK_Target;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private float leftHandIKWeightIncreaseRate;
    private bool shouldIncrease_LeftHandIKWeight;



    private void Start()
    {
        player = GetComponent<Player>();

        anim = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        backupWeaponModels = GetComponentsInChildren<BackupWeaponModel>(true);
    }

    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandIKWeight();
    }

    public void PlayFireAnimation() => anim.SetTrigger("Fire");

    public WeaponModel CurrentWeaponModel()
    {
        WeaponModel weaponModel = null;

        WeaponType weaponType = player.weapon.CurrentWeapon().weaponType;

        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (weaponModels[i].weaponType == weaponType)
            {
                weaponModel = weaponModels[i];

            }
        }
        return weaponModel;
    }
    public void PlayReloadAnimation()
    {

        float reloadSpeed = player.weapon.CurrentWeapon().reloadSpeed;
        anim.SetFloat("ReloadSpeed", reloadSpeed);
        anim.SetTrigger("Reload");
        ReduceRigWeight();
    }

    #region Animation Rigging Methods
    private void UpdateLeftHandIKWeight()
    {
        if (shouldIncrease_LeftHandIKWeight)
        {
            leftHandIK.weight += leftHandIKWeightIncreaseRate * Time.deltaTime;
            if (leftHandIK.weight >= 1)
            {
                shouldIncrease_LeftHandIKWeight = false;
            }
        }
    }

    private void UpdateRigWeight()
    {
        if (ShouldIncreaseRig_Weight)
        {
            rig.weight += rigWeightIcreaseRate * Time.deltaTime;
            if (rig.weight >= 1)
            {
                ShouldIncreaseRig_Weight = false;
            }
        }
    }

    private void ReduceRigWeight()
    {
        rig.weight = .15f;
    }
    public void MaximizeRigWeight() => ShouldIncreaseRig_Weight = true;

    public void MaximizeLeftHandIKWeight() => shouldIncrease_LeftHandIKWeight = true;
    private void AttachLeftHand()
    {
        Transform targetTransform = CurrentWeaponModel().holdPoint;

        lefthandIK_Target.localPosition = targetTransform.localPosition;
        lefthandIK_Target.localRotation = targetTransform.localRotation;

    }
    #endregion
    public void PlayWeaponEquipAnimation()
    {
        GrabType grabType = CurrentWeaponModel().grabType;//equip animation type

        float equipmentSpeed = player.weapon.CurrentWeapon().equipmentSpeed;

        leftHandIK.weight = 0;
        ReduceRigWeight();
        anim.SetFloat("WeaponGrabType", (float)grabType);
        anim.SetTrigger("WeaponGrab");
        anim.SetFloat("EquipSpeed", equipmentSpeed);

    }

    public void SwitchOnCurrentWeaponModel()
    {
        int animationIndex = (int)CurrentWeaponModel().holdType;

        SwitchOffWeaponModels();

        SwitchOffBackupWeaponModels();

        if (player.weapon.HasOnlyOneWeapon() == false)
        {
            SwitchOnBackupWeaponModel();
        }

        SwitchAnimationLayer(animationIndex);
        CurrentWeaponModel().gameObject.SetActive(true);
        AttachLeftHand();
    }
    public void SwitchOffWeaponModels()// switch off weapon models
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            weaponModels[i].gameObject.SetActive(false);
        }

    }

    private void SwitchOffBackupWeaponModels()
    {
        foreach (BackupWeaponModel backupModel in backupWeaponModels)
        {
            backupModel.Activate(false);
        }

    }

    public void SwitchOnBackupWeaponModel()
    {
        SwitchOffBackupWeaponModels();

        BackupWeaponModel lowHangWeapon = null; 
        BackupWeaponModel backHangWeapon = null;
        BackupWeaponModel sideHangWeapon = null;

        foreach (BackupWeaponModel backupModel in backupWeaponModels)
        {

            if(backupModel.weaponType == player.weapon.CurrentWeapon().weaponType)
            {
                continue;
            }


            if(player.weapon.WeaponInSlots(backupModel.weaponType) != null)
            {
               if(backupModel.HangTypeIs(HangType.LowBackHang))
               {
                   lowHangWeapon = backupModel;
               }
               else if(backupModel.HangTypeIs(HangType.BackHang))
               {
                   backHangWeapon = backupModel;
               }
               else if(backupModel.HangTypeIs(HangType.SideHang))
               {
                   sideHangWeapon = backupModel;
               }
            }
        }

        lowHangWeapon?.Activate(true);
        backHangWeapon?.Activate(true);
        sideHangWeapon?.Activate(true);

    }
    private void SwitchAnimationLayer(int layerIndex)
    {
        for (int i = 1; i < anim.layerCount; i++)
        {
            anim.SetLayerWeight(i, 0);
        }
        anim.SetLayerWeight(layerIndex, 1);
    }



}


