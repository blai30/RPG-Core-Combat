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
        private GameObject m_player;

        private void Start()
        {
            m_player = GameObject.FindWithTag("Player");
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
            m_player.GetComponent<ActionScheduler>().CancelCurrentAction();
            m_player.GetComponent<PlayerController>().enabled = false;
        }

        /// <summary>
        /// Enable player inputs when cinematic finishes playing
        /// </summary>
        /// <param name="pd">The component that calls this method</param>
        void EnableControl(PlayableDirector pd)
        {
            print("Enable control");
            m_player.GetComponent<PlayerController>().enabled = true;
        }
    }
}
