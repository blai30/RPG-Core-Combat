using RPG.Combat;
using RPG.Movement;
using RPG.Resources;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        enum CursorType
        {
            None,
            Movement,
            Combat,
            UI
        }

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType cursorType;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] private CursorMapping[] cursorMappings = null;

        private Camera m_camera;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Fighter m_fighter;

        private Health m_health;
        private Mover m_mover;

        void Awake()
        {
            m_camera = Camera.main;
            m_fighter = GetComponent<Fighter>();
            m_health = GetComponent<Health>();
            m_mover = GetComponent<Mover>();
        }

        void Update()
        {
            if (InteractWithUI())
            {
                return;
            }

            // No behavior when dead
            if (m_health.IsDead)
            {
                SetCursor(CursorType.None);
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

            SetCursor(CursorType.None);
        }

        /// <summary>
        /// Send raycast from camera to mouse click position
        /// </summary>
        /// <returns>Ray of the mouse click</returns>
        private Ray GetMouseRay()
        {
            return m_camera.ScreenPointToRay(Input.mousePosition);
        }

        private CursorMapping GetCursorMapping(CursorType cursorType)
        {
            foreach (CursorMapping cursorMapping in cursorMappings)
            {
                if (cursorMapping.cursorType == cursorType)
                {
                    return cursorMapping;
                }
            }

            return cursorMappings[0];
        }

        private void SetCursor(CursorType cursorType)
        {
            CursorMapping cursorMapping = GetCursorMapping(cursorType);
            Cursor.SetCursor(cursorMapping.texture, cursorMapping.hotspot, CursorMode.Auto);
        }

        private bool InteractWithUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }

            return false;
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

                SetCursor(CursorType.Combat);
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

                SetCursor(CursorType.Movement);
                return true;
            }

            // Cannot do movement
            return false;
        }
    }
}
