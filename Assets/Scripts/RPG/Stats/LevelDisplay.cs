using TMPro;
using UnityEngine;

namespace RPG.Stats
{
    public class LevelDisplay : MonoBehaviour
    {
        private BaseStats m_baseStats;
        private TextMeshProUGUI m_text;

        private void Awake()
        {
            m_baseStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }

        private void Start()
        {
            m_text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            // Display level and automatically update
            m_text.text = string.Format("{0:0}", m_baseStats.GetLevel());
        }
    }
}
