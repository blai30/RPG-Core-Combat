using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction
    {
        [SerializeField] private float maxMoveSpeed = 5.66f;

        /// <summary>
        /// GameObject Components
        /// </summary>
        private ActionScheduler _actionScheduler;
        private Animator _animator;
        private Health _health;
        private NavMeshAgent _navMeshAgent;

        /// <summary>
        /// Animator parameters
        /// </summary>
        private static readonly int ForwardSpeed = Animator.StringToHash("forwardSpeed");

        // Start is called before the first frame update
        void Start()
        {
            _actionScheduler = GetComponent<ActionScheduler>();
            _animator = GetComponent<Animator>();
            _health = GetComponent<Health>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            _navMeshAgent.enabled = !_health.IsDead;

            UpdateAnimator();
        }

        /// <summary>
        /// Start moving
        /// </summary>
        /// <param name="destination">Destination for navmesh agent to move to</param>
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            _actionScheduler.StartAction(this);
            MoveTo(destination, speedFraction);
        }

        /// <summary>
        /// Move navmesh agent to destination
        /// </summary>
        /// <param name="destination">Destination for navmesh agent to move to</param>
        public void MoveTo(Vector3 destination, float speedFraction)
        {
            // Move navmesh agent to destination (raycast hit point)
            _navMeshAgent.destination = destination;
            _navMeshAgent.speed = maxMoveSpeed * Mathf.Clamp01(speedFraction);
            _navMeshAgent.isStopped = false;
        }

        /// <summary>
        /// Stop moving
        /// </summary>
        public void Cancel()
        {
            _navMeshAgent.isStopped = true;
        }

        /// <summary>
        /// Update animator based on velocity
        /// </summary>
        private void UpdateAnimator()
        {
            // Convert global velocity to local space
            Vector3 velocity = _navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            _animator.SetFloat(ForwardSpeed, speed);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            // Draw destination gizmos only when destination is not within range of player
            if (_navMeshAgent != null && Vector3.Distance(transform.position, _navMeshAgent.destination) >= 0.2f)
            {
                Gizmos.DrawLine(transform.position, _navMeshAgent.destination);
                Gizmos.DrawSphere(_navMeshAgent.destination, 0.2f);
            }
        }
    }
}
