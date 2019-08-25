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

    private Camera _camera;

    private static readonly int ForwardSpeed = Animator.StringToHash("forwardSpeed");

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Click to move
        if (Input.GetMouseButton(0))
        {
            MoveToCursor();
        }

        UpdateAnimator();
    }

    private void MoveToCursor()
    {
        // Send raycast from camera through screen to terrain
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Move navmesh agent to raycast hit point
            _navMeshAgent.destination = hit.point;
        }
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
