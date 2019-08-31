using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CinematicControlRemover : MonoBehaviour
    {
        private void Start()
        {
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
        }

        /// <summary>
        /// Enable player inputs when cinematic finishes playing
        /// </summary>
        /// <param name="pd">The component that calls this method</param>
        void EnableControl(PlayableDirector pd)
        {
            print("Enable control");
        }
    }
}
