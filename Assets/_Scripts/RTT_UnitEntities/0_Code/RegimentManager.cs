using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.PlayerEntityInteractions;
using KWUtils;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    public class RegimentManager : MonoBehaviour, IEntityGroup<Regiment>
    {
        public IMainSystem<Regiment> MainSystem { get; set; }
        [SerializeField] private EntitySystem EntitySystem;
        
        [SerializeField] private int numRegiment, regimentIndex;
        [SerializeField] private GameObject[] regimentPrefabs;

        //"SOLDIER" System-manager-soldier
        private UnitMouvement unitMouvement;

        //ALL REGIMENTS
        private List<Regiment> Regiments;
        public List<Regiment> GetRegiments => Regiments;
        
        //SELECTED REGIMENT
        public HashSet<Regiment> SelectedRegiments { get; private set; }
        
        //MovingRegiment?
        
        //UNITS
        public int GetTotalUnits => Regiments.Sum(regiment => regiment.Units.Count);
        
        private void OnValidate() => regimentIndex = clamp(regimentIndex, 0, regimentPrefabs.Length - 1);

        private void Awake()
        {
            unitMouvement = GetComponent<UnitMouvement>();
            Regiments = new List<Regiment>(numRegiment);
            SelectedRegiments = new HashSet<Regiment>(1);
            CreateRegiment();
        }

        //Might need to place this into a factory...
        private void CreateRegiment()
        {
            for (int i = 0; i < numRegiment; i++)
            {
                Vector3 position = Vector3.zero + Vector3.forward * ((i + 1) * 10);
                Regiments.Add(Instantiate(regimentPrefabs[regimentIndex], position, Quaternion.identity)
                    .GetComponent<Regiment>());
            }
        }

//Interaction System EVENT : RECIEVER
//======================================================================================================================

        //SELECTION
        //==============================================================================================================
        public void OnRegimentSelected(Regiment regiment)
        {
            if (!SelectedRegiments.Add(regiment)) return;
            regiment.SetSelected(true);
        }

        public void OnClearSelections()
        {
            foreach (Regiment regiment in SelectedRegiments)
            {
                regiment.SetSelected(false);
            }
            SelectedRegiments.Clear();
        }
        
        //PLACEMENT
        //==============================================================================================================
        public void OnDestinationSet(Dictionary<Regiment, Transform[]> keys)
        {
            foreach ((Regiment regiment, Transform[] unitsDestinations) in keys)
            {
                regiment.SetNewDestination(unitsDestinations);
            }

            
            //ENTITY SYSTEM
            //Regiment[] regimentsToMove = new Regiment[keys.Keys.Count];
            //keys.Keys.CopyTo(regimentsToMove,0);
            //EntitySystem.OnDestinationsSet(keys.GetKeysArray());

            foreach (Regiment regiment in SelectedRegiments)
            {
                unitMouvement.SetUnitToUpdate(regiment);
            }
        }

        public void OnDisplayDestinationTokens(bool enable)
        {
            foreach (Regiment regiment in Regiments)
            {
                regiment.DisplayDestination(enable);
            }
        }
    }
}
