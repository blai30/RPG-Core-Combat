using System;
using RPG.Combat;
using RPG.Movement;
using RPG.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
        [SerializeField] private float raycastRadius = 1f;

        private Camera m_camera;

        /// <summary>
        /// GameObject components
        /// </summary>
        private Fighter m_fighter;
        private Health m_health;
        private Mover m_mover;

        private Vector2 m_moveVector;
        private Vector2 m_lookVector;

        void Awake()
        {
            m_camera = Camera.main;
            m_fighter = GetComponent<Fighter>();
            m_health = GetComponent<Health>();
            m_mover = GetComponent<Mover>();
        }

        void Update()
        {
//            if (InteractWithUI())
//            {
//                return;
//            }

            // No behavior when dead
            if (m_health.IsDead)
            {
                SetCursor(CursorType.None);
                return;
            }

            if (m_moveVector.sqrMagnitude > 0.01)
            {
                m_mover.Move(new Vector3(m_moveVector.x, 0, m_moveVector.y), 1f);
            }

            if (m_lookVector.sqrMagnitude > 0.01)
            {
                m_mover.LookTo(new Vector3(m_lookVector.x, 0, m_lookVector.y));
            }

            SetCursor(CursorType.None);
        }

        public void OnMove(InputValue value)
        {
            m_moveVector = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            m_lookVector = value.Get<Vector2>();
        }

        public void OnAim(InputValue value)
        {
            Vector3 mousePoint = new Vector3(value.Get<Vector2>().x, value.Get<Vector2>().y, 0f);
            m_lookVector = (mousePoint - m_camera.WorldToScreenPoint(transform.position)).normalized;
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
    }
}
