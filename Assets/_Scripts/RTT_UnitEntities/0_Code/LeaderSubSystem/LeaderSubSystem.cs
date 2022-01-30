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
        public IEntitySystem<Regiment> MainSystem { get; set; }

        private void Awake()
        {
            ActiveLeaders = new HashSet<Leader>(1);
            GridData = new GridData(64, 64, 1); //WARNING : Hard coded for now
        }

        public void AddRegimentToMove(Regiment regiment)
        {

            ActiveLeaders.Add(regiment.Leader);
            regiment.Leader.OnDestinationSet(GridData); //even if already in the hashset we want to reset the value
        }
    }
}