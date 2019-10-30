using RPG.Attributes;
using UnityEngine;

namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "RPG/Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        /// <summary>
        /// Weapon stats
        /// </summary>
        [SerializeField, Tooltip("Equipped weapon prefab")] private Weapon equippedWeapon = null;
        [SerializeField] private AnimatorOverrideController animatorOverride = null;
        [SerializeField] private float weaponRange = 2f;
        [SerializeField] private float weaponDamage = 1f;
        [SerializeField] private float percentageBonus = 0f;
        [SerializeField] private bool isRightHanded = true;
        [SerializeField] private Projectile projectile = null;

        private const string WeaponName = "Weapon";

        /// <summary>
        /// Getters
        /// </summary>
        public float WeaponRange => weaponRange;
        public float WeaponDamage => weaponDamage;
        public float PercentageBonus => percentageBonus;
        public bool HasProjectile => projectile != null;

        /// <summary>
        /// Spawn a weapon to equip and replace old weapon
        /// </summary>
        /// <param name="leftHand">Left hand transform of the character</param>
        /// <param name="rightHand">Right hand transform of the character</param>
        /// <param name="animator">Animator of the character to override</param>
        public Weapon Spawn(Transform leftHand, Transform rightHand, Animator animator)
        {
            DestroyOldWeapon(leftHand, rightHand);

            Weapon weapon = null;
            // Create weapon to equip from weapon prefab
            if (equippedWeapon != null)
            {
                weapon = Instantiate(equippedWeapon, GetHandTransform(leftHand, rightHand));
                weapon.gameObject.name = WeaponName;
            }

            // Override animator with weapon animations
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
            else if (overrideController != null)
            {
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }

            return weapon;
        }

        /// <summary>
        /// Fire projectile from ranged weapons
        /// </summary>
        /// <param name="leftHand">Left hand transform of the character</param>
        /// <param name="rightHand">Right hand transform of the character</param>
        /// <param name="target">Target to be attacked</param>
        /// <param name="instigator">GameObject that kills this GameObject (the Player)</param>
        public void LaunchProjectile(Transform leftHand, Transform rightHand, Health target, GameObject instigator, float calculatedDamage)
        {
            Projectile projectileInstance = Instantiate(projectile, GetHandTransform(leftHand, rightHand).position, Quaternion.identity);
            projectileInstance.SetTarget(target, instigator, calculatedDamage);
        }

        /// <summary>
        /// Fetch the transform of the hand that wields the weapon
        /// </summary>
        /// <param name="leftHand">Left hand transform of the character</param>
        /// <param name="rightHand">Right hand transform of the character</param>
        /// <returns>Transform of the hand that wields the weapon</returns>
        private Transform GetHandTransform(Transform leftHand, Transform rightHand)
        {
            return (isRightHanded) ? rightHand : leftHand;
        }

        /// <summary>
        /// Remove the old weapon when equipping a new weapon
        /// </summary>
        /// <param name="leftHand">Left hand transform of the character</param>
        /// <param name="rightHand">Right hand transform of the character</param>
        private void DestroyOldWeapon(Transform leftHand, Transform rightHand)
        {
            Transform oldWeapon = leftHand.Find(WeaponName);

            // Not already equipping a weapon
            if (oldWeapon == null)
            {
                oldWeapon = rightHand.Find(WeaponName);
            }

            // Not already equipping a weapon
            if (oldWeapon == null)
            {
                return;
            }

            // Mark as destroying
            oldWeapon.name = "DESTROYING";
            Destroy(oldWeapon.gameObject);
        }
    }
}
