using GameDevTV.Utils;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Resources
{
    public class Health : MonoBehaviour, ISaveable
    {
        /// <summary>
        /// Properties of Health class
        /// </summary>
        [SerializeField, Range(0, 1)] private float regenerationPercentage = 0.7f;
        [SerializeField] private UnityEvent takeDamage;

        /// <summary>
        /// GameObject components
        /// </summary>
        private BaseStats m_baseStats;

        private LazyValue<float> m_healthPoints;
        private bool m_isDead = false;

        /// <summary>
        /// Animator parameters
        /// </summary>
        private static readonly int DieTrigger = Animator.StringToHash("die");

        private void Awake()
        {
            m_baseStats = GetComponent<BaseStats>();

            m_healthPoints = new LazyValue<float>(GetInitialHealth);
        }

        private void Start()
        {
            m_healthPoints.ForceInit();
        }

        private void OnEnable()
        {
            m_baseStats.OnLevelUp += RegenerateHealth;
        }

        private void OnDisable()
        {
            m_baseStats.OnLevelUp -= RegenerateHealth;
        }

        /// <summary>
        /// Check if dead
        /// </summary>
        public bool IsDead => m_isDead;

        /// <summary>
        /// Take damage from an attack
        /// </summary>
        /// <param name="instigator">Attacker dealing damage</param>
        /// <param name="damage">Damage dealt</param>
        public void TakeDamage(GameObject instigator, float damage)
        {
            print(gameObject.name + " took damage: " + damage);

            // Health cannot go below 0
            m_healthPoints.value = Mathf.Max(m_healthPoints.value - damage, 0);
            takeDamage.Invoke();

            // Die when health reaches 0
            if (!m_isDead && m_healthPoints.value <= 0)
            {
                Die();
                AwardExperience(instigator);
            }
        }

        public float GetHealthPoints()
        {
            return m_healthPoints.value;
        }

        public float GetMaxHealthPoints()
        {
            return m_baseStats.GetStat(Stat.Health);
        }

        /// <summary>
        /// Fetch the percentage of current health over max health
        /// </summary>
        /// <returns>Current health over max health</returns>
        public float GetPercentage()
        {
            return 100 * (m_healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health));
        }

        public object CaptureState()
        {
            return m_healthPoints;
        }

        public void RestoreState(object state)
        {
            m_healthPoints.value = (float) state;
            // Die when health reaches 0
            if (!m_isDead && m_healthPoints.value <= 0)
            {
                Die();
            }
        }

        private float GetInitialHealth()
        {
            return m_baseStats.GetStat(Stat.Health);
        }

        /// <summary>
        /// Trigger death animation and mark as dead
        /// </summary>
        private void Die()
        {
            m_isDead = true;
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

        private void RegenerateHealth()
        {
//            float regenHealthPoints = m_baseStats.GetStat(Stat.Health) * regenerationPercentage;
//            m_healthPoints.value = Mathf.Max(m_healthPoints.value, regenHealthPoints);

            m_healthPoints.value = m_baseStats.GetStat(Stat.Health);
        }
    }
}
