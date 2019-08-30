using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using UnityEngine;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private float chaseDistance = 5f;

        private GameObject _targetPlayer;

        private Fighter _fighter;

        private void Start()
        {
            _targetPlayer = GameObject.FindWithTag("Player");
            _fighter = GetComponent<Fighter>();
        }

        private void Update()
        {
            if (_fighter.GetIsInRange(_targetPlayer.transform, chaseDistance) && _fighter.CanAttack(_targetPlayer))
            {
                _fighter.Attack(_targetPlayer);
            }
            else
            {
                _fighter.Cancel();
            }
        }
    }
}
