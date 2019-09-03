using System;
using RPG.Core;
using UnityEngine;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float travelSpeed = 8f;

        private Health _target = null;
        private float _damage = 0f;

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            transform.LookAt(GetAimLocation());
            transform.Translate(Time.deltaTime * travelSpeed * Vector3.forward);
        }

        public void SetTarget(Health target, float damage)
        {
            _target = target;
            _damage = damage;
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
            return _target.transform.position + (Vector3.up * targetCapsule.height / 2);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != _target)
            {
                return;
            }

            _target.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
