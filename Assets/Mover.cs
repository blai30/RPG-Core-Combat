using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mover : MonoBehaviour {

    private Camera _camera;
    private NavMeshAgent _navMeshAgent;

    // Start is called before the first frame update
    void Start() {
        _camera = Camera.main;
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButton(0)) {
            MoveToCursor();
        }
    }

    private void MoveToCursor() {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            _navMeshAgent.destination = hit.point;
        }
    }

}
