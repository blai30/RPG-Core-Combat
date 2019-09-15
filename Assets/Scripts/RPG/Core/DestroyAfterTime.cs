using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float timeUntilDestroy = 3f;

        void Start()
        {
            Destroy(gameObject, timeUntilDestroy);
        }
    }
}
