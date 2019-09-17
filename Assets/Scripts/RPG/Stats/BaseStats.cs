using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField, Range(1, 99)] private int startingLevel = 1;
        [SerializeField] private CharacterClass characterClass;
        [SerializeField] private Progression progression = null;

        private Experience _experience;

        private void Start()
        {
            _experience = GetComponent<Experience>();
        }

        private void Update()
        {
            if (gameObject.CompareTag("Player"))
            {
                print(GetLevel());
            }
        }

        public float GetStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        public int GetLevel()
        {
            if (_experience == null)
            {
                return startingLevel;
            }

            float currentExp = _experience.GetPoints();

            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float expToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (expToLevelUp > currentExp)
                {
                    return level;
                }
            }

            return penultimateLevel + 1;
        }
    }
}
