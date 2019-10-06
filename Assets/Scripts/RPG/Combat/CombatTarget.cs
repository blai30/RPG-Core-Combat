using RPG.Control;
using RPG.Resources;
using UnityEngine;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }

        public bool HandleRaycast(PlayerController playerController)
        {
            // Return false if cannot attack target
            if (!playerController.GetComponent<Fighter>().CanAttack(gameObject))
            {
                return false;
            }

            // Attack the target
            if (Input.GetMouseButton(0))
            {
                playerController.GetComponent<Fighter>().Attack(gameObject);
            }

            return true;
        }
    }
}
