using RPG.Core;
using RPG.Attributes;
using RPG.Saving;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] private float maxMoveSpeed = 5.66f;
        [SerializeField] private float maxNavMeshPathLength = 60f;
        [SerializeField] private UnityEvent onStart;

        /// <summary>
        /// GameObject Components
        /// </summary>
        private ActionScheduler m_actionScheduler;
        private Animator m_animator;
        private Health m_health;
        private Rigidbody m_rigidbody;
        private NavMeshAgent m_navMeshAgent;

        /// <summary>
        /// Animator parameters
        /// </summary>
        private static readonly int MovementSpeed = Animator.StringToHash("movementSpeed");
        private static readonly int ForwardSpeed = Animator.StringToHash("forwardSpeed");
        private static readonly int HorizontalSpeed = Animator.StringToHash("horizontalSpeed");

        private void Awake()
        {
            m_actionScheduler = GetComponent<ActionScheduler>();
            m_animator = GetComponent<Animator>();
            m_health = GetComponent<Health>();
            m_rigidbody = GetComponent<Rigidbody>();
            m_navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            // Used to disable navmesh rotation for player
            onStart.Invoke();
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
        /// <param name="speedFraction"></param>
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            m_actionScheduler.StartAction(this);
            MoveTo(destination, speedFraction);
        }

        public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path))
            {
                return false;
            }
            // Prevent navmesh from allowing player to walk towards a navmesh that is inaccessible, like the top of houses.
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            if (GetPathLength(path) > maxNavMeshPathLength)
            {
                return false;
            }

            return true;
        }

        public void Move(Vector3 direction, float speedFraction)
        {
            m_navMeshAgent.velocity = Mathf.Clamp01(speedFraction) * maxMoveSpeed * direction;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 20f);
        }

        /// <summary>
        /// Move navmesh agent to destination
        /// </summary>
        /// <param name="destination">Destination for navmesh agent to move to</param>
        /// <param name="speedFraction"></param>
        public void MoveTo(Vector3 destination, float speedFraction)
        {
            // Move navmesh agent to destination (raycast hit point)
            m_navMeshAgent.destination = destination;
            m_navMeshAgent.speed = maxMoveSpeed * Mathf.Clamp01(speedFraction);
            m_navMeshAgent.isStopped = false;
        }

        public void LookTo(Vector3 direction)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 20f);
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

        private float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            if (path.corners.Length >= 2)
            {
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
            }

            return total;
        }

        /// <summary>
        /// Update animator based on velocity
        /// </summary>
        private void UpdateAnimator()
        {
            // Convert global velocity to local space
            Vector3 localVelocity = transform.InverseTransformDirection(m_navMeshAgent.velocity);
            m_animator.SetFloat(MovementSpeed, localVelocity.magnitude);
            m_animator.SetFloat(ForwardSpeed, localVelocity.z);
            m_animator.SetFloat(HorizontalSpeed, localVelocity.x);
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
