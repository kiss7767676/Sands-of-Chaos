using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum Enemy_RangeWeaponHoldType {Common, LowHold, HighHold}

public class Enemy_RangeWeaponModel : MonoBehaviour
{
   public Transform gunPoint;
   [Space]
   public Enemy_RangeWeaponType weaponType;
   public Enemy_RangeWeaponHoldType weaponHoldType;

   public Transform leftHandTarget;
   public Transform leftElbowTarget;

   
}
