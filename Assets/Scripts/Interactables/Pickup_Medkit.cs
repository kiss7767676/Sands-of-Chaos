using UnityEngine;

public class Pickup_Medkit : Interactable
{
    [SerializeField] private int healAmount = 50;

    public override void Interaction()
    {
        weaponController.player.health.Heal(healAmount);
        ObjectPool.Instance.ReturnObject(gameObject);
    }
}