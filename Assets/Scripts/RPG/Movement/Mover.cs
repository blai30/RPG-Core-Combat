using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using RPG.Attributes;
using RPG.Saving;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] private float maxMoveSpeed = 5.66f;

        /// <summary>
        /// GameObject Components
        /// </summary>
        private ActionScheduler m_actionScheduler;
        private Animator m_animator;
        private Health m_health;
        private NavMeshAgent m_navMeshAgent;

        /// <summary>
        /// Animator parameters
        /// </summary>
        private static readonly int ForwardSpeed = Animator.StringToHash("forwardSpeed");

        private void Awake()
        {
            m_actionScheduler = GetComponent<ActionScheduler>();
            m_animator = GetComponent<Animator>();
            m_health = GetComponent<Health>();
            m_navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            m_navMeshAgent.enabled = !m_health.IsDead;

            UpdateAnimator();
        }

        /// <summary>
        /// Start moving
        /// </summary>
        /// <param name="destination">Destination for navmesh agent to move to</param>
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            m_actionScheduler.StartAction(this);
            MoveTo(destination, speedFraction);
        }

        /// <summary>
        /// Move navmesh agent to destination
        /// </summary>
        /// <param name="destination">Destination for navmesh agent to move to</param>
        public void MoveTo(Vector3 destination, float speedFraction)
        {
            // Move navmesh agent to destination (raycast hit point)
            m_navMeshAgent.destination = destination;
            m_navMeshAgent.speed = maxMoveSpeed * Mathf.Clamp01(speedFraction);
            m_navMeshAgent.isStopped = false;
        }

        /// <summary>
        /// Stop moving
        /// </summary>
        public void Cancel()
        {
            m_navMeshAgent.isStopped = true;
        }

        public object CaptureState()
        {
            return new SerializableVector3(transform.position);
        }

        public void RestoreState(object state)
        {
            SerializableVector3 position = (SerializableVector3) state;
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = position.ToVector();
            GetComponent<NavMeshAgent>().enabled = true;
        }

        /// <summary>
        /// Update animator based on velocity
        /// </summary>
        private void UpdateAnimator()
        {
            // Convert global velocity to local space
            Vector3 velocity = m_navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            m_animator.SetFloat(ForwardSpeed, speed);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            // Draw destination gizmos only when destination is not within range of player
            if (m_navMeshAgent != null && Vector3.Distance(transform.position, m_navMeshAgent.destination) >= 0.2f)
            {
                Gizmos.DrawLine(transform.position, m_navMeshAgent.destination);
                Gizmos.DrawSphere(m_navMeshAgent.destination, 0.2f);
            }
        }
    }
}
