using System.Collections;
using System.Collections.Generic;
using RPG.Saving;
using UnityEngine;

namespace RPG.Core
{
    public class Health : MonoBehaviour, ISaveable
    {
        /// <summary>
        /// Properties of Health class
        /// </summary>
        [SerializeField] private float healthPoints = 100f;
        [SerializeField] private bool isDead = false;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// Animator parameters
        /// </summary>
        private static readonly int DieTrigger = Animator.StringToHash("die");

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Check if dead
        /// </summary>
        public bool IsDead => isDead;

        /// <summary>
        /// Take damage from an attack
        /// </summary>
        /// <param name="damage">Damage dealt</param>
        public void TakeDamage(float damage)
        {
            // Health cannot go below 0
            healthPoints = Mathf.Max(healthPoints - damage, 0);
            print(healthPoints);

            CheckHealth();
        }

        /// <summary>
        /// Checks current health and die when 0
        /// </summary>
        private void CheckHealth()
        {
            // Die when health reaches 0
            if (!isDead && healthPoints <= 0)
            {
                Die();
            }
        }

        public object CaptureState()
        {
            return healthPoints;
        }

        public void RestoreState(object state)
        {
            healthPoints = (float) state;
            CheckHealth();
        }

        /// <summary>
        /// Trigger death animation and mark as dead
        /// </summary>
        private void Die()
        {
            isDead = true;
            // Get new animator component because Start is not called when loading
            GetComponent<Animator>().SetTrigger(DieTrigger);
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
    }
}
