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
            // Display health percentage and automatically update
            _text.text = string.Format("{0:0.0}%", _health.GetPercentage());
        }
    }
}
