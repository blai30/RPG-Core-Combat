using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// GameObject components
        /// </summary>
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
            // Do combat
            if (InteractWithCombat())
            {
                return;
            }

            // Do movement
            if (InteractWithMovement())
            {
                return;
            }
        }

        /// <summary>
        /// Send raycast from camera to mouse click position
        /// </summary>
        /// <returns>Ray of the mouse click</returns>
        private Ray GetMouseRay()
        {
            return _camera.ScreenPointToRay(Input.mousePosition);
        }

        /// <summary>
        /// Do combat with raycast hit
        /// </summary>
        /// <returns>Initiate combat if combat target is found</returns>
        private bool InteractWithCombat()
        {
            // Get all layers of raycast hits
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                // Find combat target from all raycast hits
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();
                // Continue if cannot attack target
                if (!_fighter.CanAttack(target))
                {
                    continue;
                }

                // Attack the target
                if (Input.GetMouseButtonDown(0))
                {
                    _fighter.Attack(target);
                }

                return true;
            }

            // No target can be attacked
            return false;
        }

        /// <summary>
        /// Do movement with raycast hit
        /// </summary>
        /// <returns></returns>
        private bool InteractWithMovement()
        {
            // Send raycast from camera through screen to terrain
            RaycastHit hit;
            if (Physics.Raycast(GetMouseRay(), out hit))
            {
                // Click to move
                if (Input.GetMouseButton(0))
                {
                    _mover.StartMoveAction(hit.point);
                }

                return true;
            }

            // Cannot do movement
            return false;
        }
    }
}
