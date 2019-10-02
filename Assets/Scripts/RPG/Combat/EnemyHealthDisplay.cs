using RPG.Resources;
using TMPro;
using UnityEngine;

namespace RPG.Combat
{
    public class EnemyHealthDisplay : MonoBehaviour
    {
        private Fighter m_fighter;
        private TextMeshProUGUI m_text;

        private void Awake()
        {
            m_fighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();
        }

        private void Start()
        {
            m_text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (m_fighter.Target == null)
            {
                m_text.text = "N/A";
                return;
            }

            // Display health percentage and automatically update
            Health health = m_fighter.Target;
            m_text.text = string.Format("{0:0.0}%", health.GetPercentage());
        }
    }
}
