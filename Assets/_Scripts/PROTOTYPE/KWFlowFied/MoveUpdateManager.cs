using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.Grid;
using UnityEngine;

using KWUtils;

namespace KaizerWaldCode
{
    public class MoveUpdateManager : MonoBehaviour
    {
        [SerializeField] private Transform endLocation;
        private Dictionary<GameObject, Grid.FlowField> objectsToMove = new Dictionary<GameObject, Grid.FlowField>();
        //NEED FLOWFIELD VALUE!
        private List<GameObject> LeaderArrived = new List<GameObject>();

        public void AddObjectToMove(GameObject leader, Grid.FlowField flowfield)
        {
            objectsToMove.TryAdd(leader, flowfield);
        }

        private void RemoveObj(GameObject arrivedObj)
        {
            objectsToMove.Remove(arrivedObj);
        }
        
        private void Update()
        {
            if (objectsToMove.Count == 0) return;
            UpdateLeaderPos();

        }

        private void UpdateLeaderPos()
        {
            foreach ((GameObject leader, Grid.FlowField flowfield)in objectsToMove)
            {
                GridSettings settings = flowfield.Settings;
                int indexCurrentlyIn =
                    leader.transform.position.GetIndexFromPosition(settings.MapSize, settings.PointSpacing);

                if (flowfield.CellsBestCost[indexCurrentlyIn] != 0)
                {
                    Vector3 bestDir = new Vector3(flowfield.BestDirection[indexCurrentlyIn].x, 0, flowfield.BestDirection[indexCurrentlyIn].y);
                    bestDir.Normalize();
                    leader.transform.Translate(bestDir * Time.deltaTime * 5, endLocation);
                }
                else
                {
                    LeaderArrived.Add(leader);
                }
            }

            if (LeaderArrived.Count == 0) return;
            foreach (GameObject leader in LeaderArrived)
            {
                RemoveObj(leader);
            }
            LeaderArrived.Clear();
        }
    }
}
