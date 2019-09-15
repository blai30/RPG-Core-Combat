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

        private const string weaponName = "Weapon";

        public float WeaponRange => weaponRange;
        public float WeaponDamage => weaponDamage;
        public bool HasProjectile => projectile != null;

        public void Spawn(Transform leftHand, Transform rightHand, Animator animator)
        {
            DestroyOldWeapon(leftHand, rightHand);

            if (equippedWeapon != null)
            {
                GameObject weapon = Instantiate(equippedWeapon, GetHandTransform(leftHand, rightHand));
                weapon.name = weaponName;
            }

            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
            else if (overrideController != null)
            {
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
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

        private void DestroyOldWeapon(Transform leftHand, Transform rightHand)
        {
            Transform oldWeapon = leftHand.Find(weaponName);

            if (oldWeapon == null)
            {
                oldWeapon = rightHand.Find(weaponName);
            }

            if (oldWeapon == null)
            {
                return;
            }

            oldWeapon.name = "DESTROYING";
            Destroy(oldWeapon.gameObject);
        }
    }
}
