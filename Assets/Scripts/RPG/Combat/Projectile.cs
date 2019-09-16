using System;
using RPG.Resources;
using UnityEngine;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
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
            transform.LookAt(GetAimLocation());
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            if (isHoming && !_target.IsDead)
            {
                transform.LookAt(GetAimLocation());
            }

            transform.Translate(Time.deltaTime * travelSpeed * Vector3.forward);
        }

        public void SetTarget(Health target, GameObject instigator, float damage)
        {
            _target = target;
            _damage = damage;
            _instigator = instigator;

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
            if (other.GetComponent<Health>() != _target)
            {
                return;
            }

            if (_target.IsDead)
            {
                return;
            }

            _target.TakeDamage(_instigator, _damage);

            travelSpeed = 0;

            if (hitEffect != null)
            {
                Instantiate(hitEffect, GetAimLocation(), transform.rotation);
            }

            foreach (GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }

            Destroy(gameObject, lifeAfterImpact);
        }
    }
}
