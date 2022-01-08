using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.RTTSelection
{
    public class SelectionRegister : MonoBehaviour
    {
        private readonly Dictionary<int, Transform> Selections = new Dictionary<int, Transform>();
        
        private Transform cachedRegiment;
        private GameObject GetRegimentFromUnit(GameObject unit) => unit.transform.parent.gameObject;

        public void Add(Transform regiment)
        {
            //cachedRegiment = regiment;
            int regimentId = regiment.GetInstanceID();
            Debug.Log(regimentId);
            if (!Selections.ContainsKey(regimentId))
            {
                Selections.Add(regimentId, regiment);
                regiment.GetComponent<RegimentComponent>().SetSelected(true);
            }
        }
        
        public void Remove(Transform regiment)
        {
            //cachedRegiment = regiment;
            int regimentId = regiment.GetInstanceID();
            
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
