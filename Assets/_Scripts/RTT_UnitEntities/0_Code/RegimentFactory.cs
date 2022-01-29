using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class RegimentFactory : MonoBehaviour
    {
        [SerializeField] private int numRegiment, regimentIndex;
        [SerializeField] private GameObject[] regimentPrefabs;
    
        private void OnValidate() => regimentIndex = Mathf.Clamp(regimentIndex, 0, regimentPrefabs.Length - 1);

        public List<Regiment> CreateRegiments()
        {
            List<Regiment> regiments = new List<Regiment>(numRegiment);
            for (int i = 0; i < numRegiment; i++)
            {
                Vector3 position = Vector3.zero + Vector3.forward * ((i + 1) * 10);
                regiments.Add(Instantiate(regimentPrefabs[regimentIndex], position, Quaternion.identity)
                    .GetComponent<Regiment>());
            }
            return regiments;
        }
    }
}