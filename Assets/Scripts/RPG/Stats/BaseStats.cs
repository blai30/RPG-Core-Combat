using System;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField, Range(1, 99)] private int startingLevel = 1;
        [SerializeField] private CharacterClass characterClass;
        [SerializeField] private Progression progression = null;
        [SerializeField] private GameObject levelUpEffect = null;

        public event Action OnLevelUp;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Experience m_experience;

        private int m_currentLevel = 0;

        private void Start()
        {
            m_experience = GetComponent<Experience>();

            // Set level
            m_currentLevel = CalculateLevel();

            // Add event to listen for experience changes
            if (m_experience != null)
            {
                m_experience.OnExperienceGained += UpdateLevel;
            }
        }

        /// <summary>
        /// Fetch a stat
        /// </summary>
        /// <param name="stat">Stat to fetch</param>
        /// <returns>Value of the stat</returns>
        public float GetStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        /// <summary>
        /// Fetch current level
        /// </summary>
        /// <returns>Current level value</returns>
        public int GetLevel()
        {
            if (m_currentLevel < 1)
            {
                m_currentLevel = CalculateLevel();
            }
            return m_currentLevel;
        }

        /// <summary>
        /// Calculates level based on experience parameters
        /// </summary>
        /// <returns>Level value based on experience</returns>
        public int CalculateLevel()
        {
            // Enemies do not level up
            if (m_experience == null)
            {
                return startingLevel;
            }

            float currentExp = m_experience.GetPoints();
            // Calculate level based on how many experience milestones have been reached
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float expToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (expToLevelUp > currentExp)
                {
                    return level;
                }
            }

            // Level starts at 1 so level will be 1 greater than the array of levels
            return penultimateLevel + 1;
        }

        /// <summary>
        /// Called when experience changes, update level depending on experience parameters
        /// </summary>
        private void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > m_currentLevel)
            {
                m_currentLevel = newLevel;
                LevelUpEffect();
                OnLevelUp();
                print("Levelled up to level " + newLevel + "!");
            }
        }

        /// <summary>
        /// Spawn visual effects upon level up
        /// </summary>
        private void LevelUpEffect()
        {
            Instantiate(levelUpEffect, transform);
        }
    }
}
