using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.PlayerEntityInteractions;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    public class RegimentManager : MonoBehaviour, IEntityGroup<Regiment>
    {
        public IMediator<Regiment> Mediator { get; set; }
        
        [SerializeField] private int numRegiment, regimentIndex;
        [SerializeField] private GameObject[] regimentPrefabs;

        //"SOLDIER" System-manager-soldier
        private UnitMouvement unitMouvement;

        //ALL REGIMENTS
        private List<Regiment> Regiments;
        public ref readonly List<Regiment> GetRegiments => ref Regiments;
        
        //SELECTED REGIMENT
        private HashSet<Regiment> SelectedRegiments;
        
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
                Vector3 position = Vector3.zero + (i + 1) * 10 * Vector3.forward;
                Regiments.Add(Instantiate(regimentPrefabs[regimentIndex], position, Quaternion.identity)
                    .GetComponent<Regiment>());
            }
        }

//MEDIATOR EVENT : RECIEVER
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
