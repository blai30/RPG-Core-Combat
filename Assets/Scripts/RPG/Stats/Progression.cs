using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "RPG/Stats/Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        [System.Serializable]
        class ProgressionCharacterClass
        {
            public CharacterClass characterClass;
            public ProgressionStat[] stats;
//            public float[] health;
        }

        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public float[] levels;
        }

        [SerializeField] private ProgressionCharacterClass[] characterClasses = null;

        private Dictionary<CharacterClass, Dictionary<Stat, float[]>> _lookupTable = null;

        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            BuildLookup();
            float[] levels = _lookupTable[characterClass][stat];
            if (levels.Length < level)
            {
                return 0;
            }

            return levels[level - 1];
        }

        public int GetLevels(Stat stat, CharacterClass characterClass)
        {
            BuildLookup();
            float[] levels = _lookupTable[characterClass][stat];
            return levels.Length;
        }

        private void BuildLookup()
        {
            if (_lookupTable != null)
            {
                return;
            }

            _lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();
            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, float[]>();
                foreach (ProgressionStat progressionStat in progressionClass.stats)
                {
                    statLookupTable[progressionStat.stat] = progressionStat.levels;
                }

                _lookupTable[progressionClass.characterClass] = statLookupTable;
            }
        }
    }
}
