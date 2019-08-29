using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float healthPoints = 100f;
        [SerializeField] private bool isDead = false;

        private Animator _animator;

        private static readonly int DieTrigger = Animator.StringToHash("die");

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void TakeDamage(float damage)
        {
            healthPoints = Mathf.Max(healthPoints - damage, 0);
            print(healthPoints);

            if (!isDead && healthPoints <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _animator.SetTrigger(DieTrigger);
            isDead = true;
        }

        public bool IsDead => isDead;
    }
}