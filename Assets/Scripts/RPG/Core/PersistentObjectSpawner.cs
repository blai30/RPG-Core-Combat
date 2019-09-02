using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class PersistentObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject persistentObjectPrefab;

        private static bool _hasSpawned = false;

        private void Awake()
        {
            if (_hasSpawned)
            {
                return;
            }

            // Spawn objects and prevent any new calls
            SpawnPersistentObjects();
            _hasSpawned = true;
        }

        /// <summary>
        /// Spawn the persistent object prefab with all of its nested prefabs
        /// </summary>
        private void SpawnPersistentObjects()
        {
            GameObject persistentObject = Instantiate(persistentObjectPrefab);
            DontDestroyOnLoad(persistentObject);
        }
    }
}
