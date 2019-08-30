using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private float chaseDistance = 5f;
        [SerializeField] private float suspicionTime = 5f;

        // Target player to chase
        private GameObject _targetPlayer;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Fighter _fighter;
        private Health _health;
        private Mover _mover;

        // Initial position
        private Vector3 _guardPosition;
        private float timeSinceLastSawPlayer = Mathf.Infinity;

        private void Start()
        {
            _targetPlayer = GameObject.FindWithTag("Player");
            _fighter = GetComponent<Fighter>();
            _health = GetComponent<Health>();
            _mover = GetComponent<Mover>();

            _guardPosition = transform.position;
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
                // Attack target player when in range
                AttackBehavior();
                timeSinceLastSawPlayer = 0;
            }
            else if (timeSinceLastSawPlayer <= suspicionTime)
            {
                SuspicionBehavior();
            }
            else
            {
                // Go back to initial guarding position
                GuardBehavior();
            }

            timeSinceLastSawPlayer += Time.deltaTime;
        }

        /// <summary>
        /// Behavior for attacking the player
        /// </summary>
        private void AttackBehavior()
        {
            _fighter.Attack(_targetPlayer);
        }

        /// <summary>
        /// Behavior for player exiting chase distance
        /// </summary>
        private void SuspicionBehavior()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        /// <summary>
        /// Behavior for guarding their guard position
        /// </summary>
        private void GuardBehavior()
        {
            _mover.StartMoveAction(_guardPosition);
        }

        // Called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
