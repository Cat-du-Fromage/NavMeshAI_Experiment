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
            Regiments = new List<Regiment>(numRegiment);
            CreateRegiment();
            
            //List<int> keyList = new List<int>(GetNestedPlacementTokens.Keys);
        }

        private void Start()
        {
            NestedPlacementTokens = new Dictionary<int, Renderer[]>(numRegiment);
            InitFixedPlacementTokens();
        }

        // Start is called before the first frame update

        private void CreateRegiment()
        {
            GameObject newRegiment;
            for (int i = 0; i < numRegiment; i++)
            {
                Vector3 position = Vector3.zero + Vector3.forward * (i+1) * 10;
                newRegiment = Instantiate(regimentPrefabs[regimentIndex], position, Quaternion.identity);
                Regiments.Add(newRegiment.GetComponent<Regiment>());
            }
        }

        private void InitFixedPlacementTokens()
        {
            Unit currentUnit;
            for (int i = 0; i < Regiments.Count; i++)
            {
                Renderer[] tokens = new Renderer[Regiments[i].Units.Count];
                for (int j = 0; j < Regiments[i].Units.Count; j++)
                {
                    currentUnit = Regiments[i].Units[j];
                    tokens[j] = currentUnit.GetPlacementToken.GetComponent<Renderer>();
                }
                NestedPlacementTokens.Add(Regiments[i].Index, tokens);
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
