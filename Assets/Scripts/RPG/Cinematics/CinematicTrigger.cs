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
        private bool _triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            // Play cinematic only once when player enters trigger collider
            if (!_triggered && other.CompareTag("Player"))
            {
                _triggered = true;
                GetComponent<PlayableDirector>().Play();
            }
        }
    }
}
