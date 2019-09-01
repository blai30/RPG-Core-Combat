using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private int sceneIndexToLoad = -1;

        private void OnTriggerEnter(Collider other)
        {
            // Load new scene when player enters portal trigger
            print("Portal triggered");
            if (other.CompareTag("Player"))
            {
                SceneManager.LoadScene(sceneIndexToLoad);
            }
        }
    }
}
