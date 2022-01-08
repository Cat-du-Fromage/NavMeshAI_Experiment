using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.RTTSelection
{
    public class SelectionRegister : MonoBehaviour
    {
        //CACHED VARIABLES
        private Transform cachedRegiment;
        private int regimentId;
        
        //REGISTER
        private readonly Dictionary<int, Transform> Selections = new Dictionary<int, Transform>();
        
        //ACCESSORS
        public int Count => Selections.Count;
        public Dictionary<int, Transform> GetSelections => Selections;
        
        public void Add(Transform regiment)
        {
            regimentId = regiment.GetInstanceID();
            if (!Selections.ContainsKey(regimentId))
            {
                Selections.Add(regimentId, regiment);
                
                regiment.GetComponent<RegimentComponent>().SetSelected(true);
                Debug.Log($"Add to Register {regiment.TryGetComponent<RegimentComponent>(out _)}");
            }
        }
        
        public void Remove(Transform regiment)
        {
            regimentId = regiment.GetInstanceID();
            Selections[regimentId].GetComponent<RegimentComponent>().SetSelected(false);
            Selections.Remove(regimentId);
        }
        
        public void Clear()
        {
            for (int i = 0; i < Selections.Count; i++)
            {
                (int key, Transform value) = Selections.ElementAt(i);
                if (value == null) continue;
                Selections[key].GetComponent<RegimentComponent>().SetSelected(false);
            }
            Selections.Clear();
        }
        
    }
}
