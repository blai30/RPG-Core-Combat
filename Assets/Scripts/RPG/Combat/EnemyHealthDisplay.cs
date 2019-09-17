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

            // Display health percentage and automatically update
            Health health = _fighter.Target;
            _text.text = string.Format("{0:0.0}%", health.GetPercentage());
        }
    }
}
