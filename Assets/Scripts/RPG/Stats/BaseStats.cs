using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField, Range(1, 99)] private int startingLevel = 1;
        [SerializeField] private CharacterClass characterClass;
        [SerializeField] private Progression progression = null;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Experience _experience;

        private int _currentLevel = 0;

        private void Start()
        {
            _experience = GetComponent<Experience>();

            // Set level
            _currentLevel = CalculateLevel();

            // Add event to listen for experience changes
            if (_experience != null)
            {
                _experience.onExperienceGained += UpdateLevel;
            }
        }

        /// <summary>
        /// Called when experience changes, update level depending on experience parameters
        /// </summary>
        private void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > _currentLevel)
            {
                _currentLevel = newLevel;
                print("Levelled up to level " + newLevel + "!");
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
            if (_currentLevel < 1)
            {
                _currentLevel = CalculateLevel();
            }
            return _currentLevel;
        }

        /// <summary>
        /// Calculates level based on experience parameters
        /// </summary>
        /// <returns>Level value based on experience</returns>
        public int CalculateLevel()
        {
            // Enemies do not level up
            if (_experience == null)
            {
                return startingLevel;
            }

            float currentExp = _experience.GetPoints();
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
    }
}
