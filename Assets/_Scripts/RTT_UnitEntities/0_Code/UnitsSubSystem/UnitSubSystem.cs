using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class UnitSubSystem : MonoBehaviour, IEntitySubSystem<Regiment>
    {
        public IEntitySystem<Regiment> MainSystem { get; set; }

        private Dictionary<Regiment, Unit[]> unitsToMove;

        private Vector3 CachedPos;
        private void Awake()
        {
            unitsToMove = new Dictionary<Regiment, Unit[]>(1);
        }

        // Update is called once per frame
        private void Update()
        {
            if (unitsToMove.Count == 0) return;
            foreach ((Regiment regiment, Unit[] units) in unitsToMove)
            {
                for (int i = 0; i < units.Length; i++)
                {
                    CachedPos = units[i].transform.position;
                    units[i].transform.position = Vector3.MoveTowards(
                        CachedPos, 
                        regiment.Leader.FormationSlotsGhost[i].position,
                        6 * Time.deltaTime);
                    
                }
            }

        }

        public void AddRegimentToMove(Regiment regiment)
        { 
            unitsToMove.Add(regiment, regiment.Units.ToArray());
        }

        public void RemoveRegimentToMove(Regiment regiment)
        {
            unitsToMove.Remove(regiment);
        }
    }
}
