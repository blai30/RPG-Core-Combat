using UnityEngine;

namespace RPG.UI.DamageText
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] private DamageText damageTextPrefab = null;

        public void Spawn(float damageDealt)
        {
            DamageText damageText = Instantiate(damageTextPrefab, transform);
            damageText.SetValue(damageDealt);
        }
    }
}
