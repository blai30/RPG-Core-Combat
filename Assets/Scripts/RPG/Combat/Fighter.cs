using System;
using System.Collections.Generic;
using GameDevTV.Utils;
using RPG.Core;
using RPG.Movement;
using RPG.Attributes;
using RPG.Saving;
using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        /// <summary>
        /// Fighter stats
        /// </summary>
        [SerializeField] private float timeBetweenAttacks = 1f;
        [SerializeField] private Transform leftHandTransform = null;
        [SerializeField] private Transform rightHandTransform = null;
        [SerializeField] private WeaponConfig defaultWeaponConfig = null;

        /// <summary>
        /// GameObject components
        /// </summary>
        private ActionScheduler m_actionScheduler;
        private Animator m_animator;
        private Mover m_mover;

        private LazyValue<WeaponConfig> m_currentWeapon;
        private Health m_target;
        private float m_timeSinceLastAttack = Mathf.Infinity;

        public Health Target => m_target;

        /// <summary>
        /// Animator parameters
        /// </summary>
        private static readonly int AttackTrigger = Animator.StringToHash("attack");
        private static readonly int StopAttackTrigger = Animator.StringToHash("stopAttack");

        private void Awake()
        {
            m_actionScheduler = GetComponent<ActionScheduler>();
            m_animator = GetComponent<Animator>();
            m_mover = GetComponent<Mover>();

            m_currentWeapon = new LazyValue<WeaponConfig>(SetupDefaultWeapon);
        }

        private WeaponConfig SetupDefaultWeapon()
        {
            AttachWeapon(defaultWeaponConfig);
            return defaultWeaponConfig;
        }

        private void Start()
        {
            m_currentWeapon.ForceInit();
        }

        private void Update()
        {
            m_timeSinceLastAttack += Time.deltaTime;

            // No target exists
            if (m_target == null)
            {
                return;
            }

            // Target is already dead
            if (m_target.IsDead)
            {
                return;
            }

            // Get in range of the target
            if (m_target != null && !GetIsInRange(m_target.transform, m_currentWeapon.value.WeaponRange))
            {
                m_mover.MoveTo(m_target.transform.position, 1f);
            }
            else
            {
                // Attack the target when in range
                m_mover.Cancel();
                AttackBehavior();
            }
        }

        /// <summary>
        /// Animation Event called from the animation
        /// </summary>
        void Hit()
        {
            if (m_target == null)
            {
                return;
            }

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            if (m_currentWeapon.value.HasProjectile)
            {
                // Fire projectile
                m_currentWeapon.value.LaunchProjectile(leftHandTransform, rightHandTransform, m_target, gameObject, damage);
            }
            else
            {
                // Take damage at point of impact
                m_target.TakeDamage(gameObject, damage);
            }
        }

        /// <summary>
        /// Animation Event called from the animation
        /// </summary>
        void Shoot()
        {
            Hit();
        }

        /// <summary>
        /// Checks if this object is in range of the target
        /// </summary>
        /// <returns>If the target is in range for attack</returns>
        public bool GetIsInRange(Transform target, float range)
        {
            return Vector3.Distance(transform.position, target.position) < range;
        }

        /// <summary>
        /// Cancel the attack state
        /// </summary>
        public void Cancel()
        {
            StopAttack();
            m_mover.Cancel();
            m_target = null;
        }

        /// <summary>
        /// Checks if the target can be attacked
        /// </summary>
        /// <param name="combatTarget">The target to be attacked</param>
        /// <returns>If the target can be attacked</returns>
        public bool CanAttack(GameObject combatTarget)
        {
            // No target exists
            if (combatTarget == null)
            {
                return false;
            }

            // Check if the target has the Health component
            Health targetToTest = combatTarget.GetComponent<Health>();

            // If target has Health component and is not already dead
            return targetToTest != null && !targetToTest.IsDead;
        }

        /// <summary>
        /// Schedule attack action to target
        /// </summary>
        /// <param name="combatTarget">The target to be attacked</param>
        public void Attack(GameObject combatTarget)
        {
            m_actionScheduler.StartAction(this);
            m_target = combatTarget.GetComponent<Health>();
        }

        /// <summary>
        /// Equip the weapon that is passed in
        /// </summary>
        /// <param name="weaponConfig">Weapon to be equipped</param>
        public void EquipWeapon(WeaponConfig weaponConfig)
        {
            m_currentWeapon.value = weaponConfig;
            AttachWeapon(weaponConfig);
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return m_currentWeapon.value.WeaponDamage;
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return m_currentWeapon.value.PercentageBonus;
            }
        }

        public object CaptureState()
        {
            return m_currentWeapon.value.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string) state;
            WeaponConfig weaponConfig = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
            EquipWeapon(weaponConfig);
        }

        private void AttachWeapon(WeaponConfig weaponConfig)
        {
            weaponConfig.Spawn(leftHandTransform, rightHandTransform, m_animator);
        }

        /// <summary>
        /// The state of attacking the target
        /// </summary>
        private void AttackBehavior()
        {
            // Rotate to face the target
            transform.LookAt(m_target.transform);

            // Constantly attack over set interval
            if (m_timeSinceLastAttack >= timeBetweenAttacks)
            {
                // This will trigger the Hit() event
                TriggerAttack();
                m_timeSinceLastAttack = 0;
            }
        }

        /// <summary>
        /// Trigger the attack animations
        /// </summary>
        private void TriggerAttack()
        {
            m_animator.ResetTrigger(StopAttackTrigger);
            m_animator.SetTrigger(AttackTrigger);
        }

        /// <summary>
        /// Stop the attack animations
        /// </summary>
        private void StopAttack()
        {
            m_animator.ResetTrigger(AttackTrigger);
            m_animator.SetTrigger(StopAttackTrigger);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (m_currentWeapon != null)
            {
                Gizmos.DrawWireSphere(transform.position, m_currentWeapon.value.WeaponRange);
            }
        }
    }
}
