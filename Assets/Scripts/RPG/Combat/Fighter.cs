using RPG.Core;
using RPG.Movement;
using RPG.Resources;
using RPG.Saving;
using UnityEngine;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable
    {
        /// <summary>
        /// Fighter stats
        /// </summary>
        [SerializeField] private float timeBetweenAttacks = 1f;
        [SerializeField] private Transform leftHandTransform = null;
        [SerializeField] private Transform rightHandTransform = null;
        [SerializeField] private Weapon defaultWeapon = null;
        [SerializeField] private Weapon currentWeapon = null;

        /// <summary>
        /// GameObject components
        /// </summary>
        private ActionScheduler _actionScheduler;
        private Animator _animator;
        private Mover _mover;

        private Health _target;
        private float _timeSinceLastAttack = Mathf.Infinity;

        public Health Target => _target;

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

            if (currentWeapon == null)
            {
                EquipWeapon(defaultWeapon);
            }
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
            if (_target != null && !GetIsInRange(_target.transform, currentWeapon.WeaponRange))
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

            if (currentWeapon.HasProjectile)
            {
                // Fire projectile
                currentWeapon.LaunchProjectile(leftHandTransform, rightHandTransform, _target, gameObject);
            }
            else
            {
                // Take damage at point of impact
                _target.TakeDamage(gameObject, currentWeapon.WeaponDamage);
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

        /// <summary>
        /// Equip the weapon that is passed in
        /// </summary>
        /// <param name="weapon">Weapon to be equipped</param>
        public void EquipWeapon(Weapon weapon)
        {
            currentWeapon = weapon;
            weapon.Spawn(leftHandTransform, rightHandTransform, _animator);
        }

        public object CaptureState()
        {
            return currentWeapon.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string) state;
            Weapon weapon = UnityEngine.Resources.Load<Weapon>(weaponName);
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
            if (currentWeapon != null)
            {
                Gizmos.DrawWireSphere(transform.position, currentWeapon.WeaponRange);
            }
        }
    }
}
