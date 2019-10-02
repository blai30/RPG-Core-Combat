using TMPro;
using UnityEngine;

namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        private Experience m_experience;
        private TextMeshProUGUI m_text;

        private void Awake()
        {
            m_experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
        }

        private void Start()
        {
            m_text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            // Display experience points and automatically update
            m_text.text = string.Format("{0:0}", m_experience.GetPoints());
        }
    }
}
