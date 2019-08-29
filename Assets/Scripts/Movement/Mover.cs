using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction
    {
        /// <summary>
        /// GameObject Components
        /// </summary>
        private ActionScheduler _actionScheduler;
        private Animator _animator;
        private NavMeshAgent _navMeshAgent;

        private static readonly int ForwardSpeed = Animator.StringToHash("forwardSpeed");

        // Start is called before the first frame update
        void Start()
        {
            _actionScheduler = GetComponent<ActionScheduler>();
            _animator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateAnimator();
        }

        public void Cancel()
        {
            _navMeshAgent.isStopped = true;
        }

        public void StartMoveAction(Vector3 destination)
        {
            _actionScheduler.StartAction(this);
            MoveTo(destination);
        }

        public void MoveTo(Vector3 destination)
        {
            // Move navmesh agent to destination (raycast hit point)
            _navMeshAgent.destination = destination;
            _navMeshAgent.isStopped = false;
        }

        private void UpdateAnimator()
        {
            // Convert global velocity to local space
            Vector3 velocity = _navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            _animator.SetFloat(ForwardSpeed, speed);
        }
    }
}