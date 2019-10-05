using System;
using GameDevTV.Utils;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField, Range(1, 99)] private int startingLevel = 1;
        [SerializeField] private CharacterClass characterClass;
        [SerializeField] private bool useModifiers = false;
        [SerializeField] private Progression progression = null;
        [SerializeField] private GameObject levelUpEffect = null;

        public event Action OnLevelUp;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Experience m_experience;

        private LazyValue<int> m_currentLevel;

        private void Awake()
        {
            m_experience = GetComponent<Experience>();
            // Set level
            m_currentLevel = new LazyValue<int>(CalculateLevel);
        }

        private void Start()
        {
            m_currentLevel.ForceInit();
        }

        private void OnEnable()
        {
            // Add event that listens for experience changes
            if (m_experience != null)
            {
                m_experience.OnExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable()
        {
            // Remove event that listens for experience changes
            if (m_experience != null)
            {
                m_experience.OnExperienceGained -= UpdateLevel;
            }
        }

        /// <summary>
        /// Fetch current level
        /// </summary>
        /// <returns>Current level value</returns>
        public int GetLevel()
        {
            return m_currentLevel.value;
        }

        /// <summary>
        /// Fetch a stat
        /// </summary>
        /// <param name="stat">Stat to fetch</param>
        /// <returns>Value of the stat</returns>
        public float GetStat(Stat stat)
        {
            return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat) / 100);
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        private float GetAdditiveModifier(Stat stat)
        {
            if (!useModifiers)
            {
                return 0;
            }

            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }

            return total;
        }

        private float GetPercentageModifier(Stat stat)
        {
            if (!useModifiers)
            {
                return 0;
            }

            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }

            return total;
        }

        /// <summary>
        /// Calculates level based on experience parameters
        /// </summary>
        /// <returns>Level value based on experience</returns>
        private int CalculateLevel()
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
            if (newLevel > m_currentLevel.value)
            {
                m_currentLevel.value = newLevel;
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
