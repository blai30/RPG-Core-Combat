using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        private const float WaypointGizmoRadius = 0.3f;

        private void OnDrawGizmos()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                int j = GetNextIndex(i);
                Gizmos.DrawSphere(GetWaypointPosition(i), WaypointGizmoRadius);
                Gizmos.DrawLine(GetWaypointPosition(i), GetWaypointPosition(j));
            }
        }

        private int GetNextIndex(int index)
        {
            // Returns next index, 0 if next index is out of bounds
            return (index + 1) % transform.childCount;
        }

        private Vector3 GetWaypointPosition(int index)
        {
            return transform.GetChild(index).position;
        }
    }
}
