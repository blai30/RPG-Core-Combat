using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mover : MonoBehaviour
{

    /// <summary>
    /// GameObject Components
    /// </summary>
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;

    private static readonly int ForwardSpeed = Animator.StringToHash("forwardSpeed");

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimator();
    }

    public void MoveTo(Vector3 destination)
    {
        // Move navmesh agent to destination (raycast hit point)
        _navMeshAgent.destination = destination;
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
