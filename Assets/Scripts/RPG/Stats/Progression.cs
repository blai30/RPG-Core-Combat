using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "RPG/Stats/Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        /// <summary>
        /// A character class with progression stats
        /// </summary>
        [System.Serializable]
        class ProgressionCharacterClass
        {
            public CharacterClass characterClass;
            public ProgressionStat[] stats;
        }

        /// <summary>
        /// A progression stat
        /// </summary>
        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public float[] levels;
        }

        /// <summary>
        /// Array of character classes with progression
        /// </summary>
        [SerializeField] private ProgressionCharacterClass[] characterClasses = null;

        /// <summary>
        /// Dictionary of character classes and their stats
        /// </summary>
        private Dictionary<CharacterClass, Dictionary<Stat, float[]>> m_lookupTable = null;

        /// <summary>
        /// Fetch the stat of a character class given level
        /// </summary>
        /// <param name="stat">The stat to fetch</param>
        /// <param name="characterClass">The character class to fetch from</param>
        /// <param name="level">The level of stat to fetch</param>
        /// <returns></returns>
        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            BuildLookup();
            float[] levels = m_lookupTable[characterClass][stat];
            if (levels.Length < level)
            {
                return 0;
            }

            return levels[level - 1];
        }

        /// <summary>
        /// Fetch the levels of a stat from a character class
        /// </summary>
        /// <param name="stat">The stat to fetch the levels for</param>
        /// <param name="characterClass">The character class to fetch stat levels from</param>
        /// <returns></returns>
        public int GetLevels(Stat stat, CharacterClass characterClass)
        {
            BuildLookup();
            float[] levels = m_lookupTable[characterClass][stat];
            return levels.Length;
        }

        /// <summary>
        /// Populate dictionary with character classes, stats, and stat levels
        /// </summary>
        private void BuildLookup()
        {
            // Dictionary already populated
            if (m_lookupTable != null)
            {
                return;
            }

            // Initialize
            m_lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();
            // For every character class with progression
            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, float[]>();
                // For every stat with progression in the character class
                foreach (ProgressionStat progressionStat in progressionClass.stats)
                {
                    // Set the stat levels
                    statLookupTable[progressionStat.stat] = progressionStat.levels;
                }

                m_lookupTable[progressionClass.characterClass] = statLookupTable;
            }
        }
    }
}
