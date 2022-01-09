using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public class SelectionRegister : MonoBehaviour
    {
        public List<Transform> Selections { get; } = new List<Transform>();
        
        //CACHED VARIABLES
        private Transform cachedRegiment;
        private int regimentId;
        

        public void Add(Transform regiment)
        {
            if (!Selections.Contains(regiment))
            {
                Selections.Add(regiment);
                regiment.GetComponent<RegimentComponent>().SetSelected(true);
            }
        }
        
        public void Remove(Transform regiment)
        {
            if (Selections.Contains(regiment))
            {
                regiment.GetComponent<RegimentComponent>().SetSelected(false);
                Selections.Remove(regiment);
            }
        }
        
        public void Clear()
        {
            Selections.ForEach(regiment=> regiment.GetComponent<RegimentComponent>().SetSelected(false));
            Selections.Clear();
        }
        
    }
}
