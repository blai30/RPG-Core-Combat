using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        private const float WaypointGizmoRadius = 0.3f;

        public int GetNextIndex(int index)
        {
            // Returns next index, 0 if next index is out of bounds
            return (index + 1) % transform.childCount;
        }

        public Vector3 GetWaypointPosition(int index)
        {
            return transform.GetChild(index).position;
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                int j = GetNextIndex(i);
                Gizmos.DrawSphere(GetWaypointPosition(i), WaypointGizmoRadius);
                Gizmos.DrawLine(GetWaypointPosition(i), GetWaypointPosition(j));
            }
        }
    }
}
