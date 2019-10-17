using TMPro;
using UnityEngine;

namespace RPG.UI.DamageText
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageText = null;

        // Animation event
        public void DestroyText()
        {
            Destroy(gameObject);
        }

        public void SetValue(float damageDealt)
        {
            damageText.text = $"{damageDealt:0.0}";
        }
    }
}
