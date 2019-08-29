using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{

    public class PlayerController : MonoBehaviour
    {

        private Camera _camera;
        private Mover _mover;
        private Fighter _fighter;

        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
            _mover = GetComponent<Mover>();
            _fighter = GetComponent<Fighter>();
        }

        // Update is called once per frame
        void Update()
        {
            InteractWithCombat();
            InteractWithMovement();
        }

        private void InteractWithCombat()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();
                if (target == null)
                {
                    continue;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    _fighter.Attack(target);
                }
            }
        }

        private void InteractWithMovement()
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
            RaycastHit hit;
            if (Physics.Raycast(GetMouseRay(), out hit))
            {
                _mover.MoveTo(hit.point);
            }
        }

        private Ray GetMouseRay()
        {
            return _camera.ScreenPointToRay(Input.mousePosition);
        }
    }

}
