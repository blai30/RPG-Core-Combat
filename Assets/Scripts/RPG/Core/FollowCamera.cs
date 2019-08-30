using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using UnityEngine;

namespace RPG.Core
{
    public class FollowCamera : MonoBehaviour
    {
        /// <summary>
        /// Transform of the target object to follow
        /// </summary>
        [SerializeField] private Transform target;

        private void Start()
        {
            target = FindObjectOfType<PlayerController>().transform;
        }

        void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position;
            }
        }
    }
}
