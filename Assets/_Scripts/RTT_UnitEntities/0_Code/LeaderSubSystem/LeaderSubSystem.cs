using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class LeaderSubSystem : MonoBehaviour, IEntitySubSystem<Regiment>
    {
        private HashSet<Leader> ActiveLeaders;
        public IEntitySystem<Regiment> MainSystem { get; set; }

        private void Awake()
        {
            ActiveLeaders = new HashSet<Leader>(1);
        }

        public void AddRegimentToMove(Regiment regiment)
        {
            ActiveLeaders.Add(regiment.Leader);
            regiment.Leader.OnDestinationSet(); //even if already in the hashset we want to reset the value
        }
    }
}