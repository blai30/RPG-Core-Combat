using UnityEngine;

namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "RPG/Weapon", order = 0)]
    public class Weapon : ScriptableObject
    {
        [SerializeField, Tooltip("Equipped weapon prefab")] private GameObject equippedWeapon = null;
        [SerializeField] private AnimatorOverrideController animatorOverride = null;
        [SerializeField] private float weaponRange = 2f;
        [SerializeField] private float weaponDamage = 1f;
        [SerializeField] private bool isRightHanded = true;

        public float WeaponRange => weaponRange;
        public float WeaponDamage => weaponDamage;

        public void Spawn(Transform leftHand, Transform rightHand, Animator animator)
        {
            if (equippedWeapon != null)
            {
                Transform handTransform = (isRightHanded) ? rightHand : leftHand;
                Instantiate(equippedWeapon, handTransform);
            }

            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
        }
    }
}
