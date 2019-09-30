using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float timeUntilDestroy = 3f;
        [SerializeField] private GameObject targetToDestroy = null;

        void Start()
        {
            if (targetToDestroy != null)
            {
                Destroy(targetToDestroy, timeUntilDestroy);
            }
            Destroy(gameObject, timeUntilDestroy);
        }
    }
}
