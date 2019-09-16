using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RPG.Resources
{
    public class ExperienceDisplay : MonoBehaviour
    {
        private Experience _experience;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
        }

        private void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            _text.text = String.Format("{0:0}", _experience.GetPoints());
        }
    }
}
