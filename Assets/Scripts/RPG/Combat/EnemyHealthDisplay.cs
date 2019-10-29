using RPG.Attributes;
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
            m_text.text = $"{health.GetHealthPoints():0}/{health.GetMaxHealthPoints():0}";
        }
    }
}
