using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Resources;
using TMPro;
using UnityEngine;

namespace RPG.Combat
{
    public class EnemyHealthDisplay : MonoBehaviour
    {
        private Fighter _fighter;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _fighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();
        }

        private void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (_fighter.Target == null)
            {
                _text.text = "N/A";
                return;
            }

            Health health = _fighter.Target;
            _text.text = String.Format("{0:0.0}%", health.GetPercentage());
        }
    }
}
