using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Saving;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        [SerializeField] private float fadeInTime = 0.25f;

        private const string DefaultSaveFile = "save";

        private IEnumerator Start()
        {
            // Fade into the scene when game starts
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();
            yield return GetComponent<SavingSystem>().LoadLastScene(DefaultSaveFile);
            yield return fader.FadeIn(fadeInTime);
        }

        void Update()
        {
            // Save the game state
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }

            // Load the game state
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
        }

        /// <summary>
        /// Saves the state of all saveable entities
        /// </summary>
        public void Save()
        {
            // Call to the saving system save
            GetComponent<SavingSystem>().Save(DefaultSaveFile);
        }

        /// <summary>
        /// Loads the state of all saveable entities
        /// </summary>
        public void Load()
        {
            // Call to the saving system load
            GetComponent<SavingSystem>().Load(DefaultSaveFile);
        }
    }
}
