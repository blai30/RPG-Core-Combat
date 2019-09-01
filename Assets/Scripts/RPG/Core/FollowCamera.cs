using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace RPG.Core
{
    public class FollowCamera : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<CinemachineVirtualCamera>().m_Follow = GameObject.FindWithTag("Player").transform;
        }
    }
}
