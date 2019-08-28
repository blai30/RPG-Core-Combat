using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Camera _camera;
    private Mover _mover;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _mover = GetComponent<Mover>();
    }

    // Update is called once per frame
    void Update()
    {
        // Click to move
        if (Input.GetMouseButton(0))
        {
            MoveToCursor();
        }
    }

    private void MoveToCursor()
    {
        // Send raycast from camera through screen to terrain
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            _mover.MoveTo(hit.point);
        }
    }

}
