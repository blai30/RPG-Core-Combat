using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RPG.Resources
{
    public class HealthDisplay : MonoBehaviour
    {
        private Health _health;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _health = GameObject.FindWithTag("Player").GetComponent<Health>();
        }

        private void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            _text.text = String.Format("{0:0.0}%", _health.GetPercentage());
        }
    }
}
