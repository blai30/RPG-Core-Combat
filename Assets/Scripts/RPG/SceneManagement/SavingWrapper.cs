using System.Collections;
using System.Collections.Generic;
using RPG.Saving;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        private const string defaultSaveFile = "save";

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
        }

        void Save()
        {
            // Call to the saving system save
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }

        private void Load()
        {
            // Call to the saving system load
            GetComponent<SavingSystem>().Load(defaultSaveFile);
        }
    }
}
