using UnityEngine;

namespace RPG.Core
{
    public class CameraFacing : MonoBehaviour
    {
        private Camera m_camera;

        private void Awake()
        {
            m_camera = Camera.main;
        }

        void LateUpdate()
        {
            transform.forward = m_camera.transform.forward;
        }
    }
}
