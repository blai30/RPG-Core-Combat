using TMPro;
using UnityEngine;

namespace RPG.Attributes
{
    public class HealthDisplay : MonoBehaviour
    {
        private Health m_health;
        private TextMeshProUGUI m_text;

        private void Awake()
        {
            m_health = GameObject.FindWithTag("Player").GetComponent<Health>();
            m_text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            // Display health percentage and automatically update
            m_text.text = $"{m_health.GetHealthPoints():0}/{m_health.GetMaxHealthPoints():0}";
        }
    }
}
