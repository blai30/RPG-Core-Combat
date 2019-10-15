using System;
using RPG.Combat;
using RPG.Movement;
using RPG.Resources;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        [Serializable]
        struct CursorMapping
        {
            public CursorType cursorType;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] private CursorMapping[] cursorMappings = null;
        [SerializeField] private float maxNavMeshProjectionDistance = 1f;
        [SerializeField] private float maxNavMeshPathLength = 60f;

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

            if (InteractWithComponent())
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

        private bool InteractWithComponent()
        {
            // Get all layers of raycast hits
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Do movement with raycast hit
        /// </summary>
        /// <returns></returns>
        private bool InteractWithMovement()
        {
            // Send raycast from camera through screen to terrain
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                // Click to move
                if (Input.GetMouseButton(0))
                {
                    m_mover.StartMoveAction(target, 1f);
                }

                SetCursor(CursorType.Movement);
                return true;
            }

            // Cannot do movement
            return false;
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();

            // Raycast to terrain
            RaycastHit hit;
            if (!Physics.Raycast(GetMouseRay(), out hit))
            {
                return false;
            }

            // Find nearest navmesh point
            NavMeshHit navMeshHit;
            if (!NavMesh.SamplePosition(hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas))
            {
                return false;
            }

            target = navMeshHit.position;

            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path))
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

        private RaycastHit[] RaycastAllSorted()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            float[] distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);

            return hits;
        }
    }
}
