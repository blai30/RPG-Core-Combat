using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        // The currently scheduled action
        private IAction currentAction = null;

        // Start new action and cancel old action
        public void StartAction(IAction action)
        {
            // New action is the same action as old
            if (currentAction == action)
            {
                return;
            }

            // Cancel old action if there is one
            if (currentAction != null)
            {
                currentAction.Cancel();
            }

            // Schedule new action
            currentAction = action;
        }
    }
}
