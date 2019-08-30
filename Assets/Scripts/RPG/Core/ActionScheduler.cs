using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        // The currently scheduled action
        private IAction _currentAction;    // = null

        // Start new action and cancel old action
        public void StartAction(IAction action)
        {
            // New action is the same action as old
            if (_currentAction == action)
            {
                return;
            }

            // Cancel old action if there is one
            _currentAction?.Cancel();    // if (currentAction != null)

            // Schedule new action
            _currentAction = action;
        }
    }
}
