using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimationEvent : MonoBehaviour
{
    private WeaponVisualController visualController;
    private PlayerWeaponController weaponController;

    private void Start()
    {
        visualController = GetComponentInParent<WeaponVisualController>();
        weaponController = GetComponentInParent<PlayerWeaponController>();
    }

    public void ReloadIsOver()
    {
        visualController.MaximizeRigWeight();
        visualController.CurrentWeaponModel().reloadSFX.Stop();
        weaponController.CurrentWeapon().ReloadBullets();

        weaponController.SetWeaponReady(true);
        weaponController.UpdateWeaponUI();
    }

    public void ReturnRig()
    {
        visualController.MaximizeRigWeight();
        visualController.MaximizeLeftHandIKWeight();
    }

    public void WeaponEquippingIsOver()
    {
        Debug.Log("Weapon equipping finished, setting ready to true");
        weaponController.SetWeaponReady(true);
    }

    public void SwitchOnWeaponModel() => visualController.SwitchOnCurrentWeaponModel();
}
