using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using UnityEngine;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private float chaseDistance = 5f;

        private GameObject _targetPlayer;

        private Fighter _fighter;
        private Health _health;

        private void Start()
        {
            _targetPlayer = GameObject.FindWithTag("Player");
            _fighter = GetComponent<Fighter>();
            _health = GetComponent<Health>();
        }

        private void Update()
        {
            // No behavior when dead
            if (_health.IsDead)
            {
                return;
            }

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
