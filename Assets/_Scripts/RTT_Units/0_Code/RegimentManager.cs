using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    //CONCERN : event that affect regiment OUTSIDE of player interaction
    //
    public class RegimentManager : MonoBehaviour
    {
        [SerializeField] private int numRegiment, regimentIndex;
        [SerializeField] private GameObject[] regimentPrefabs;

        //"SOLDIER" System-manager-soldier
        private UnitMouvement unitMouvement;

        //ALL REGIMENTS
        private List<Regiment> Regiments;
        public ref readonly List<Regiment> GetRegiments => ref Regiments;

        //NESTED PLACEMENTS
        private Dictionary<int, Renderer[]> NestedPlacementTokens;
        public ref readonly Dictionary<int, Renderer[]> GetNestedPlacementTokens => ref NestedPlacementTokens;
        
        //DESTINATION TOKENS
        private Dictionary<int, Renderer[]> DestinationTokens;
        public Renderer[] GetDestinationTokens(int index) => DestinationTokens[index];
        
        //UNITS
        public int GetTotalUnits => Regiments.Sum(regiment => regiment.Units.Count);

        private void OnValidate() => regimentIndex = clamp(regimentIndex, 0, regimentPrefabs.Length-1);

        private void Awake()
        {
            unitMouvement = GetComponent<UnitMouvement>();
            Regiments = new List<Regiment>(numRegiment);
            CreateRegiment();
            
            //List<int> keyList = new List<int>(GetNestedPlacementTokens.Keys);
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
                Vector3 position = Vector3.zero + Vector3.forward * (i+1) * 10;
                Regiments.Add(Instantiate(regimentPrefabs[regimentIndex], position, Quaternion.identity).GetComponent<Regiment>());
            }
        }

        private void InitNestedPlacementTokens()
        {
            for (int i = 0; i < Regiments.Count; i++)
            {
                NestedPlacementTokens.Add(Regiments[i].Index, Regiments[i].NestedPositionTokenRenderers.ToArray());
            }
        }

        public void UpdateNestedPlacementTokens(bool state)
        {
            foreach((int _, Renderer[] value) in NestedPlacementTokens)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i].enabled = state;
                }
            }
        }
    }
}
