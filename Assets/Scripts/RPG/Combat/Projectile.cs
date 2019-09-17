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

        private Health _target = null;
        private GameObject _instigator;
        private float _damage = 0f;

        private void Start()
        {
            // Face the aim target
            transform.LookAt(GetAimLocation());
        }

        private void Update()
        {
            // No target found
            if (_target == null)
            {
                return;
            }

            // Continuously face the target until they die
            if (isHoming && !_target.IsDead)
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
            _target = target;
            _damage = damage;
            _instigator = instigator;

            // Destroy projectile after some time if not already destroyed
            Destroy(gameObject, maxLifetime);
        }

        /// <summary>
        /// Gets the vertical center of target collider
        /// </summary>
        /// <returns>Vector3 of y-axis center of target</returns>
        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = _target.GetComponent<CapsuleCollider>();
            if (targetCapsule == null)
            {
                return _target.transform.position;
            }
            return _target.transform.position + (Vector3.up * (targetCapsule.height / 2));
        }

        private void OnTriggerEnter(Collider other)
        {
            // Collider is not the target
            if (other.GetComponent<Health>() != _target)
            {
                return;
            }

            // Target already dead
            if (_target.IsDead)
            {
                return;
            }

            // Deal damage and reward experience to instigator
            _target.TakeDamage(_instigator, _damage);

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
