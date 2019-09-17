using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using UnityEngine;

namespace RPG.Resources
{
    public class Health : MonoBehaviour, ISaveable
    {
        /// <summary>
        /// Properties of Health class
        /// </summary>
        [SerializeField] private float healthPoints = -1f;
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

            if (healthPoints < 0)
            {
                healthPoints = GetComponent<BaseStats>().GetStat(Stat.Health);
            }
        }

        /// <summary>
        /// Check if dead
        /// </summary>
        public bool IsDead => isDead;

        /// <summary>
        /// Take damage from an attack
        /// </summary>
        /// <param name="damage">Damage dealt</param>
        public void TakeDamage(GameObject instigator, float damage)
        {
            // Health cannot go below 0
            healthPoints = Mathf.Max(healthPoints - damage, 0);
            print(healthPoints);

            // Die when health reaches 0
            if (!isDead && healthPoints <= 0)
            {
                Die();
                AwardExperience(instigator);
            }
        }

        /// <summary>
        /// Fetch the percentage of current health over max health
        /// </summary>
        /// <returns>Current health over max health</returns>
        public float GetPercentage()
        {
            return 100 * (healthPoints / GetComponent<BaseStats>().GetStat(Stat.Health));
        }

        public object CaptureState()
        {
            return healthPoints;
        }

        public void RestoreState(object state)
        {
            healthPoints = (float) state;
            // Die when health reaches 0
            if (!isDead && healthPoints <= 0)
            {
                Die();
            }
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

        /// <summary>
        /// Give experience points to instigator
        /// </summary>
        /// <param name="instigator">GameObject that kills this GameObject (the Player)</param>
        private void AwardExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();

            if (experience == null)
            {
                return;
            }

            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
        }
    }
}
