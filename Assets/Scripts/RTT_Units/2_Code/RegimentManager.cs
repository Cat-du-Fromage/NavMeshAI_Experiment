using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    //Register ALL Regiment InGame!
    //
    public class RegimentManager : MonoBehaviour
    {
        [SerializeField] private int numRegiment, regimentIndex;
        
        [SerializeField] private GameObject[] regimentPrefabs;
        
        public List<Regiment> Regiments;

        private void OnValidate()
        {
            regimentIndex = clamp(regimentIndex, 0, regimentPrefabs.Length-1);
        }

        // Start is called before the first frame update
        void Start()
        {
            CreateRegiment();
        }

        private void CreateRegiment()
        {
            for (int i = 0; i < numRegiment; i++)
            {
                Vector3 position = Vector3.zero + Vector3.forward * (i+1) * 10;
                
                Regiments.Add(Instantiate(regimentPrefabs[regimentIndex], position, Quaternion.identity).GetComponent<Regiment>());
            }
        }
    }
}
