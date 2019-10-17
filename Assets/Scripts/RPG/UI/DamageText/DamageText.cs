using UnityEngine;

namespace RPG.UI.DamageText
{
    public class DamageText : MonoBehaviour
    {
        // Animation event
        public void DestroyText()
        {
            Destroy(gameObject);
        }
    }
}
