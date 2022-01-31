using System;
using System.Collections.Generic;
using KaizerWaldCode.FlowField;
using UnityEngine;
using static KaizerWaldCode.FlowField.FlowField;

namespace KaizerWaldCode.RTTUnits
{
    public class LeaderSubSystem : MonoBehaviour, IEntitySubSystem<Regiment>
    {
        private GridData GridData;

        private HashSet<Leader> ActiveLeaders;
        private HashSet<Leader> LeaderToRemove;
        public IEntitySystem<Regiment> MainSystem { get; set; }

        private Vector3 CachedPosition;
        private void Awake()
        {
            ActiveLeaders = new HashSet<Leader>(1);
            LeaderToRemove = new HashSet<Leader>(1);
            GridData = new GridData(64, 64, 1); //WARNING : Hard coded for now
        }

        private void Update()
        {
            if (ActiveLeaders.Count == 0) return;
            foreach (Leader leader in ActiveLeaders)
            {
                CachedPosition = leader.transform.position;
                //Speed is hard coded(5) may want to use unit's speed when refactoring this
                leader.transform.position =
                    Vector3.MoveTowards(CachedPosition, leader.EndDestination,2 * Time.deltaTime);

                if (CachedPosition != leader.EndDestination) continue;
                LeaderToRemove.Add(leader);
            }
        }

        public void LateUpdate()
        {
            if (LeaderToRemove.Count == 0) return;
            foreach (Leader leaderToRemove in LeaderToRemove)
            {
                ActiveLeaders.Remove(leaderToRemove);
                MainSystem.Notify(this, leaderToRemove.AttachRegiment);
            }
            //ActiveLeaders.ExceptWith(LeaderToRemove);
            LeaderToRemove.Clear();
        }

        public void AddRegimentToMove(Regiment regiment)
        {
            regiment.Leader.OnDestinationSet(GridData); //even if already in the hashset we want to reset the value
            ActiveLeaders.Add(regiment.Leader);
        }
    }
}