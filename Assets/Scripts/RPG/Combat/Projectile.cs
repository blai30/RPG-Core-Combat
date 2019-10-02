using RPG.Resources;
using UnityEngine;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        /// <summary>
        /// Projectile stats
        /// </summary>
        [SerializeField] private float travelSpeed = 8f;
        [SerializeField] private float maxLifetime = 10f;
        [SerializeField] private float lifeAfterImpact = 2f;
        [SerializeField] private bool isHoming = true;
        [SerializeField] private GameObject hitEffect = null;
        [SerializeField] private GameObject[] destroyOnHit = null;

        private Health m_target = null;
        private GameObject m_instigator;
        private float m_damage = 0f;

        private void Start()
        {
            // Face the aim target
            transform.LookAt(GetAimLocation());
        }

        private void Update()
        {
            // No target found
            if (m_target == null)
            {
                return;
            }

            // Continuously face the target until they die
            if (isHoming && !m_target.IsDead)
            {
                transform.LookAt(GetAimLocation());
            }

            // Continuously travel forward
            transform.Translate(Time.deltaTime * travelSpeed * Vector3.forward);
        }

        /// <summary>
        /// Set the target to shoot at
        /// </summary>
        /// <param name="target">Target to be shot at</param>
        /// <param name="instigator">GameObject that kills this GameObject (the Player)</param>
        /// <param name="damage">Pass the damage dealt</param>
        public void SetTarget(Health target, GameObject instigator, float damage)
        {
            m_target = target;
            m_damage = damage;
            m_instigator = instigator;

            // Destroy projectile after some time if not already destroyed
            Destroy(gameObject, maxLifetime);
        }

        /// <summary>
        /// Gets the vertical center of target collider
        /// </summary>
        /// <returns>Vector3 of y-axis center of target</returns>
        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = m_target.GetComponent<CapsuleCollider>();
            if (targetCapsule == null)
            {
                return m_target.transform.position;
            }
            return m_target.transform.position + (Vector3.up * (targetCapsule.height / 2));
        }

        private void OnTriggerEnter(Collider other)
        {
            // Collider is not the target
            if (other.GetComponent<Health>() != m_target)
            {
                return;
            }

            // Target already dead
            if (m_target.IsDead)
            {
                return;
            }

            // Deal damage and reward experience to instigator
            m_target.TakeDamage(m_instigator, m_damage);

            // Stop the projectile
            travelSpeed = 0;

            // Create effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, GetAimLocation(), transform.rotation);
            }

            // Destroy projectile children on hit if any are set
            foreach (GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }

            // Destroy projectile after some time
            Destroy(gameObject, lifeAfterImpact);
        }
    }
}
