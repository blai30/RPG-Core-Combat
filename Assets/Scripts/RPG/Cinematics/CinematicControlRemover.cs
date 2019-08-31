using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using RPG.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CinematicControlRemover : MonoBehaviour
    {
        private GameObject _player;

        private void Start()
        {
            _player = GameObject.FindWithTag("Player");
            GetComponent<PlayableDirector>().played += DisableControl;
            GetComponent<PlayableDirector>().stopped += EnableControl;
        }

        /// <summary>
        /// Disable player inputs when cinematic starts playing
        /// </summary>
        /// <param name="pd">The component that calls this method</param>
        void DisableControl(PlayableDirector pd)
        {
            print("Disable control");
            _player.GetComponent<ActionScheduler>().CancelCurrentAction();
            _player.GetComponent<PlayerController>().enabled = false;
        }

        /// <summary>
        /// Enable player inputs when cinematic finishes playing
        /// </summary>
        /// <param name="pd">The component that calls this method</param>
        void EnableControl(PlayableDirector pd)
        {
            print("Enable control");
            _player.GetComponent<PlayerController>().enabled = true;
        }
    }
}
