using RPG.Core;
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
        [SerializeField] private Projectile projectile = null;

        public float WeaponRange => weaponRange;
        public float WeaponDamage => weaponDamage;
        public bool HasProjectile => projectile != null;

        public void Spawn(Transform leftHand, Transform rightHand, Animator animator)
        {
            if (equippedWeapon != null)
            {
                Instantiate(equippedWeapon, GetHandTransform(leftHand, rightHand));
            }

            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
        }

        public void LaunchProjectile(Transform leftHand, Transform rightHand, Health target)
        {
            Projectile projectileInstance = Instantiate(projectile, GetHandTransform(leftHand, rightHand).position, Quaternion.identity);
            projectileInstance.SetTarget(target, weaponDamage);
        }

        private Transform GetHandTransform(Transform leftHand, Transform rightHand)
        {
            return (isRightHanded) ? rightHand : leftHand;
        }
    }
}
