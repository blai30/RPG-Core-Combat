using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace RPG.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        // Identifiers to connect portals together
        enum DestinationIdentifier
        {
            A, B, C, D, E
        }

        [SerializeField] private int sceneToLoadIndex = -1;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private DestinationIdentifier destination;
        [SerializeField] private float fadeOutTime = 0.25f;
        [SerializeField] private float fadeInTime = 0.25f;
        [SerializeField] private float fadeWaitTime = 0.1f;

        private void OnTriggerEnter(Collider other)
        {
            // Load new scene when player enters portal trigger
            print("Portal triggered");
            if (other.CompareTag("Player"))
            {
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {
            // Invalid scene index, cannot load new scene
            if (sceneToLoadIndex < 0)
            {
                Debug.LogError("Scene to load is not set");
                yield break;
            }

            // Preserve this portal until new scene finishes loading
            DontDestroyOnLoad(gameObject);

            // Fade out of scene
            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime);

            // Save current level
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            savingWrapper.Save();

            // Load new scene
            yield return SceneManager.LoadSceneAsync(sceneToLoadIndex);

            // Load current level
            savingWrapper.Load();

            // Destination portal in new scene where the player will spawn
            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);

            // Save in the new scene
            savingWrapper.Save();

            // Wait before fading in
            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);

            // Scene loaded successfully, destroy previous portal
            print("Scene loaded");
            Destroy(gameObject);
        }

        /// <summary>
        /// Get the destination portal in the new scene where the player will spawn
        /// </summary>
        /// <returns></returns>
        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                // Skip over this portal
                if (portal == this)
                {
                    continue;
                }

                // Skip over every other portal
                if (portal.destination != destination)
                {
                    continue;
                }

                // Destination portal found
                return portal;
            }

            // No portal found (should not reach this)
            return null;
        }

        /// <summary>
        /// Update the player position and rotation upon entering portal
        /// </summary>
        /// <param name="otherPortal">The destination portal in the new scene where the player will spawn</param>
        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Player");
            NavMeshAgent playerNavMeshAgent = player.GetComponent<NavMeshAgent>();
            // Disable nav mesh agent to avoid issues
            playerNavMeshAgent.enabled = false;
            playerNavMeshAgent.Warp(otherPortal.spawnPoint.position);
            player.transform.rotation = otherPortal.spawnPoint.rotation;
            // Reenable once player transform is updated
            playerNavMeshAgent.enabled = true;
        }
    }
}
