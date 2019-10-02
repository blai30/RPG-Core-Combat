using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CinematicTrigger : MonoBehaviour
    {
        private bool m_triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            // Play cinematic only once when player enters trigger collider
            if (!m_triggered && other.CompareTag("Player"))
            {
                m_triggered = true;
                GetComponent<PlayableDirector>().Play();
            }
        }
    }
}
