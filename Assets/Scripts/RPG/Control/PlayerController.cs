using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        private Camera _camera;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Fighter _fighter;
        private Health _health;
        private Mover _mover;

        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
            _fighter = GetComponent<Fighter>();
            _health = GetComponent<Health>();
            _mover = GetComponent<Mover>();
        }

        // Update is called once per frame
        void Update()
        {
            // No behavior when dead
            if (_health.IsDead)
            {
                return;
            }

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

                if (target == null)
                {
                    continue;
                }

                // Continue if cannot attack target
                if (!_fighter.CanAttack(target.gameObject))
                {
                    continue;
                }

                // Attack the target
                if (Input.GetMouseButton(0))
                {
                    _fighter.Attack(target.gameObject);
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
