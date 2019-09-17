using System;
using RPG.Saving;
using UnityEngine;

namespace RPG.Stats
{
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] private float experiencePoints = 0f;

        /// <summary>
        /// Action for when experience is gained
        /// </summary>
        public event Action onExperienceGained;

        /// <summary>
        /// Gain experience from value that is passed in, trigger action
        /// </summary>
        /// <param name="experience">Experience to gain</param>
        public void GainExperience(float experience)
        {
            experiencePoints += experience;
            onExperienceGained();
        }

        /// <summary>
        /// Fetch experience points
        /// </summary>
        /// <returns>Experience points</returns>
        public float GetPoints()
        {
            return experiencePoints;
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
