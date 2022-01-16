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
        private List<Renderer> NestedPlacementTokens;
        public ref readonly List<Renderer> GetNestedPlacementTokens => ref NestedPlacementTokens;

        public int GetTotalUnits => Regiments.Sum(regiment => regiment.Units.Count);

        private void OnValidate() => regimentIndex = clamp(regimentIndex, 0, regimentPrefabs.Length-1);

        private void Awake()
        {
            Regiments = new List<Regiment>(numRegiment);
            CreateRegiment();
        }

        private void Start()
        {
            NestedPlacementTokens = new List<Renderer>(GetTotalUnits);
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
                for (int j = 0; j < Regiments[i].Units.Count; j++)
                {
                    currentUnit = Regiments[i].Units[j];
                    NestedPlacementTokens.Add(currentUnit.GetPlacementToken.GetComponent<Renderer>());
                }
            }
        }
    }
}
