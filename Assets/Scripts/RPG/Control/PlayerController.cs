using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Movement;
using RPG.Resources;
using UnityEngine;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        private Camera m_camera;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Fighter m_fighter;
        private Health m_health;
        private Mover m_mover;

        // Start is called before the first frame update
        void Start()
        {
            m_camera = Camera.main;
            m_fighter = GetComponent<Fighter>();
            m_health = GetComponent<Health>();
            m_mover = GetComponent<Mover>();
        }

        // Update is called once per frame
        void Update()
        {
            // No behavior when dead
            if (m_health.IsDead)
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
            return m_camera.ScreenPointToRay(Input.mousePosition);
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
                if (!m_fighter.CanAttack(target.gameObject))
                {
                    continue;
                }

                // Attack the target
                if (Input.GetMouseButton(0))
                {
                    m_fighter.Attack(target.gameObject);
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
                    m_mover.StartMoveAction(hit.point, 1f);
                }

                return true;
            }

            // Cannot do movement
            return false;
        }
    }
}
