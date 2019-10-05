using System;
using RPG.Saving;
using UnityEngine;

namespace RPG.Stats
{
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] private float experiencePoints = 0f;

        private BaseStats m_baseStats;

        /// <summary>
        /// Action for when experience is gained
        /// </summary>
        public event Action OnExperienceGained;

        private void Awake()
        {
            m_baseStats = GetComponent<BaseStats>();
        }

        /// <summary>
        /// Gain experience from value that is passed in, trigger action
        /// </summary>
        /// <param name="experience">Experience to gain</param>
        public void GainExperience(float experience)
        {
            experiencePoints += experience;
            OnExperienceGained();
        }

        /// <summary>
        /// Fetch experience points
        /// </summary>
        /// <returns>Experience points</returns>
        public float GetPoints()
        {
            return experiencePoints;
        }

        public float GetPointsToLevelUp()
        {
            return m_baseStats.GetStat(Stat.ExperienceToLevelUp);
        }

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void RestoreState(object state)
        {
            experiencePoints = (float) state;
        }
    }
}
