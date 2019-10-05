using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Utils;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Resources;
using UnityEngine;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private float chaseDistance = 5f;
        [SerializeField] private float suspicionTime = 5f;
        [SerializeField] private PatrolPath patrolPath = null;
        [SerializeField] private float waypointTolerance = 1f;
        [SerializeField] private float waypointDwellTime = 3f;
        [SerializeField, Range(0, 1)] private float patrolSpeedFraction = 0.2f;

        // Target player to chase
        private GameObject m_targetPlayer;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Fighter m_fighter;
        private Health m_health;
        private Mover m_mover;

        /// <summary>
        /// Internal properties
        /// </summary>
        private LazyValue<Vector3> m_guardPosition;
        private float m_timeSinceLastSawPlayer = Mathf.Infinity;
        private float m_timeSinceArrivedAtWaypoint = Mathf.Infinity;
        private int m_currentWaypointIndex = 0;

        private void Awake()
        {
            m_targetPlayer = GameObject.FindWithTag("Player");
            m_fighter = GetComponent<Fighter>();
            m_health = GetComponent<Health>();
            m_mover = GetComponent<Mover>();

            m_guardPosition = new LazyValue<Vector3>(GetGuardPosition);
        }

        private void Start()
        {
            m_guardPosition.ForceInit();
        }

        private void Update()
        {
            // No behavior when dead
            if (m_health.IsDead)
            {
                return;
            }

            if (m_fighter.GetIsInRange(m_targetPlayer.transform, chaseDistance) && m_fighter.CanAttack(m_targetPlayer))
            {
                // Attack target player when in range
                AttackBehavior();
            }
            else if (m_timeSinceLastSawPlayer <= suspicionTime)
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

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Updates the timers for waiting events
        /// </summary>
        private void UpdateTimers()
        {
            m_timeSinceLastSawPlayer += Time.deltaTime;
            m_timeSinceArrivedAtWaypoint += Time.deltaTime;
        }

        /// <summary>
        /// Behavior for attacking the player
        /// </summary>
        private void AttackBehavior()
        {
            m_timeSinceLastSawPlayer = 0;
            m_fighter.Attack(m_targetPlayer);
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
            Vector3 nextPosition = m_guardPosition.value;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    m_timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }

                nextPosition = GetCurrentWaypoint();
            }

            if (m_timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                m_mover.StartMoveAction(nextPosition, patrolSpeedFraction);
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
            m_currentWaypointIndex = patrolPath.GetNextIndex(m_currentWaypointIndex);
        }

        /// <summary>
        /// Get the position of the current waypoint
        /// </summary>
        /// <returns>Position of current waypoint</returns>
        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypointPosition(m_currentWaypointIndex);
        }

        // Called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
