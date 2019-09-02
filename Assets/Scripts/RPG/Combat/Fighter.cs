using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {
        /// <summary>
        /// Fighter stats
        /// </summary>
        [SerializeField] private float timeBetweenAttacks = 1f;
        [SerializeField] private Transform handTransform = null;
        [SerializeField] private Weapon defaultWeapon = null;

        /// <summary>
        /// GameObject components
        /// </summary>
        private ActionScheduler _actionScheduler;
        private Animator _animator;
        private Mover _mover;

        private Health _target;
        private float _timeSinceLastAttack = Mathf.Infinity;
        private Weapon _currentWeapon;

        /// <summary>
        /// Animator parameters
        /// </summary>
        private static readonly int AttackTrigger = Animator.StringToHash("attack");
        private static readonly int StopAttackTrigger = Animator.StringToHash("stopAttack");

        private void Start()
        {
            _actionScheduler = GetComponent<ActionScheduler>();
            _animator = GetComponent<Animator>();
            _mover = GetComponent<Mover>();

            EquipWeapon(defaultWeapon);
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;

            // No target exists
            if (_target == null)
            {
                return;
            }

            // Target is already dead
            if (_target.IsDead)
            {
                return;
            }

            // Get in range of the target
            if (_target != null && !GetIsInRange(_target.transform, defaultWeapon.WeaponRange))
            {
                _mover.MoveTo(_target.transform.position, 1f);
            }
            else
            {
                // Attack the target when in range
                _mover.Cancel();
                AttackBehavior();
            }
        }

        /// <summary>
        /// Animation Event called from the animation
        /// </summary>
        void Hit()
        {
            if (_target == null)
            {
                return;
            }

            // Take damage at point of impact
            _target.TakeDamage(_currentWeapon.WeaponDamage);
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
            _mover.Cancel();
            _target = null;
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
            _actionScheduler.StartAction(this);
            _target = combatTarget.GetComponent<Health>();
        }

        public void EquipWeapon(Weapon weapon)
        {
            _currentWeapon = weapon;
            weapon.Spawn(handTransform, _animator);
        }

        /// <summary>
        /// The state of attacking the target
        /// </summary>
        private void AttackBehavior()
        {
            // Rotate to face the target
            transform.LookAt(_target.transform);

            // Constantly attack over set interval
            if (_timeSinceLastAttack >= timeBetweenAttacks)
            {
                // This will trigger the Hit() event
                TriggerAttack();
                _timeSinceLastAttack = 0;
            }
        }

        /// <summary>
        /// Trigger the attack animations
        /// </summary>
        private void TriggerAttack()
        {
            _animator.ResetTrigger(StopAttackTrigger);
            _animator.SetTrigger(AttackTrigger);
        }

        /// <summary>
        /// Stop the attack animations
        /// </summary>
        private void StopAttack()
        {
            _animator.ResetTrigger(AttackTrigger);
            _animator.SetTrigger(StopAttackTrigger);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, defaultWeapon.WeaponRange);
        }
    }
}
