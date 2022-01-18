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
        [SerializeField] private int numRegiment, regimentIndex;
        [SerializeField] private GameObject[] regimentPrefabs;

        //"SOLDIER" System-manager-soldier
        private UnitMouvement unitMouvement;

        //ALL REGIMENTS
        private List<Regiment> Regiments;
        public ref readonly List<Regiment> GetRegiments => ref Regiments;
        
        //SELECTED REGIMENT
        private List<Regiment> SelectedRegiments;

        //NESTED PLACEMENTS
        private Dictionary<int, Renderer[]> NestedPlacementTokens;
        public ref readonly Dictionary<int, Renderer[]> GetNestedPlacementTokens => ref NestedPlacementTokens;

        //DESTINATION TOKENS
        //private Dictionary<int, Renderer[]> DestinationTokens;
        //public Renderer[] GetDestinationTokens(int index) => DestinationTokens[index];

        //UNITS
        public int GetTotalUnits => Regiments.Sum(regiment => regiment.Units.Count);
        
        public IMediator<Regiment> Mediator { get; set; }

        private void OnValidate() => regimentIndex = clamp(regimentIndex, 0, regimentPrefabs.Length - 1);

        private void Awake()
        {
            unitMouvement = GetComponent<UnitMouvement>();
            Regiments = new List<Regiment>(numRegiment);
            SelectedRegiments = new List<Regiment>(1);
            CreateRegiment();
        }

        private void Start()
        {
            NestedPlacementTokens = new Dictionary<int, Renderer[]>(numRegiment);
            InitNestedPlacementTokens();
        }

        //Might need to place this into a factory...
        private void CreateRegiment()
        {
            for (int i = 0; i < numRegiment; i++)
            {
                Vector3 position = Vector3.zero + Vector3.forward * (i + 1) * 10;
                Regiments.Add(Instantiate(regimentPrefabs[regimentIndex], position, Quaternion.identity)
                    .GetComponent<Regiment>());
            }
        }

        private void InitNestedPlacementTokens()
        {
            for (int i = 0; i < Regiments.Count; i++)
            {
                NestedPlacementTokens.Add(Regiments[i].Index, Regiments[i].NestedPositionTokenRenderers.ToArray());
            }
        }


//MEDIATOR EVENT : RECIEVER
//======================================================================================================================

        //SELECTION
        //==============================================================================================================
        public void OnDisplaySelectionToken(Regiment regiment, bool enable)
        {
            for (int i = 0; i < regiment.Units.Count; i++)
            {
                regiment.Units[i].GetSelectionRenderer.enabled = enable;
            }
        }
        
        public void OnRegimentSelected(Regiment regiment)
        {
            if (SelectedRegiments.Contains(regiment)) return;
            SelectedRegiments.Add(regiment);
            regiment.SetSelected(true);
            OnDisplaySelectionToken(regiment, true);
        }

        public void OnClearSelections()
        {
            foreach (Regiment regiment in SelectedRegiments)
            {
                regiment.SetSelected(false);
                OnDisplaySelectionToken(regiment, false);
            }
            SelectedRegiments.Clear();
        }
        
        //PLACEMENT
        //==============================================================================================================
        public void OnDestinationSet(Dictionary<int, Transform[]> keys)
        {
            foreach ((int key, Transform[] value) in keys)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    NestedPlacementTokens[key][i].transform.position = value[i].transform.position;
                }
            }
        }

        public void OnDisplayDestinationTokens(bool enable)
        {
            foreach ((int _, Renderer[] value) in NestedPlacementTokens)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i].enabled = enable;
                }
            }
        }
    }
}
