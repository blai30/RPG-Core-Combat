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
        [SerializeField] private PatrolPath patrolPath;
        [SerializeField] private float waypointTolerance = 1f;
        [SerializeField] private float waypointDwellTime = 3f;
        [SerializeField, Range(0, 1)] private float patrolSpeedFraction = 0.2f;

        // Target player to chase
        private GameObject _targetPlayer;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Fighter _fighter;
        private Health _health;
        private Mover _mover;

        /// <summary>
        /// Internal properties
        /// </summary>
        private Vector3 _guardPosition;
        private float _timeSinceLastSawPlayer = Mathf.Infinity;
        private float _timeSinceArrivedAtWaypoint = Mathf.Infinity;
        private int _currentWaypointIndex = 0;

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
            }
            else if (_timeSinceLastSawPlayer <= suspicionTime)
            {
                // Wait around after target player exits chase distance
                SuspicionBehavior();
            }
            else
            {
                // Go back to patrol path
                PatrolBehavior();
            }

            UpdateTimers();
        }

        /// <summary>
        /// Updates the timers for waiting events
        /// </summary>
        private void UpdateTimers()
        {
            _timeSinceLastSawPlayer += Time.deltaTime;
            _timeSinceArrivedAtWaypoint += Time.deltaTime;
        }

        /// <summary>
        /// Behavior for attacking the player
        /// </summary>
        private void AttackBehavior()
        {
            _timeSinceLastSawPlayer = 0;
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
        private void PatrolBehavior()
        {
            Vector3 nextPosition = _guardPosition;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    _timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }

                nextPosition = GetCurrentWaypoint();
            }

            if (_timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                _mover.StartMoveAction(nextPosition, patrolSpeedFraction);
            }
        }

        /// <summary>
        /// Check if arrived at waypoint within tolerance
        /// </summary>
        /// <returns>If within tolerance range of waypoint</returns>
        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint <= waypointTolerance;
        }

        /// <summary>
        /// Set the next waypoint to move to
        /// </summary>
        private void CycleWaypoint()
        {
            _currentWaypointIndex = patrolPath.GetNextIndex(_currentWaypointIndex);
        }

        /// <summary>
        /// Get the position of the current waypoint
        /// </summary>
        /// <returns>Position of current waypoint</returns>
        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypointPosition(_currentWaypointIndex);
        }

        // Called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
