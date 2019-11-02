namespace IL3DN
{
    using UnityEngine;
    [ExecuteInEditMode]

    public class IL3DN_Wind : MonoBehaviour
    {
        public bool Wiggle = true;
        public bool Wind = true;
        [Range(0f, 1f)]
        public float WindStrenght = 1f;
        [Range(0f, 10f)]
        public float WindSpeed = 10f;
        [Range(0f, 1f)]
        public float WindTurbulence = .3f;
        [Range(0f, 1f)]
        public float LeavesWiggle = .01f;
        [Range(0f, 1f)]
        public float GrassWiggle = .1f;
        private float WindGizmo = 0.5f;


        void Update()
        {

            if (Wiggle)
            {
                Shader.EnableKeyword("_WIGGLE_ON");
            }
            else
            {
                Shader.DisableKeyword("_WIGGLE_ON");
            }

            if (Wind)
            {
                Shader.EnableKeyword("_WIND_ON");
            }
            else
            {
                Shader.DisableKeyword("_WIND_ON");
            }

            Shader.SetGlobalVector("WindDirection", transform.rotation * Vector3.back);
            Shader.SetGlobalFloat("WindStrenghtFloat", WindStrenght);
            Shader.SetGlobalFloat("WindSpeedFloat", WindSpeed);
            Shader.SetGlobalFloat("WindTurbulenceFloat", WindTurbulence);
            Shader.SetGlobalFloat("LeavesWiggleFloat", LeavesWiggle);
            Shader.SetGlobalFloat("GrassWiggleFloat", GrassWiggle);
        }

        void OnDrawGizmos()
        {
            Vector3 dir = (transform.position + transform.forward).normalized;

            Gizmos.color = Color.green;
            Vector3 up = transform.up;
            Vector3 side = transform.right;

            Vector3 end = transform.position + transform.forward * (WindGizmo * 10f);
            Gizmos.DrawLine(transform.position, end);

            float s = WindGizmo;
            Vector3 front = transform.forward * WindGizmo;

            Gizmos.DrawLine(end, end - front + up * s);
            Gizmos.DrawLine(end, end - front - up * s);
            Gizmos.DrawLine(end, end - front + side * s);
            Gizmos.DrawLine(end, end - front - side * s);

            Gizmos.DrawLine(end - front - side * s, end - front + up * s);
            Gizmos.DrawLine(end - front + up * s, end - front + side * s);
            Gizmos.DrawLine(end - front + side * s, end - front - up * s);
            Gizmos.DrawLine(end - front - up * s, end - front - side * s);
        }
    }
}