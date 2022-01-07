using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.RTTSelection
{
    public class SelectionRegister : MonoBehaviour
    {
        private Dictionary<int, GameObject> selectionRegister = new Dictionary<int, GameObject>();
        // Start is called before the first frame update
        public void AddSelection(GameObject unit)
        {
            int unitId = unit.GetInstanceID();
            if (!selectionRegister.ContainsKey(unitId))
            {
                selectionRegister.Add(unitId, unit);
                unit.GetComponent<SelectionComponent>().SetSelected(true);
            }
        }

        public void DeselectSingleUnit(int unitId)
        {
            selectionRegister[unitId].GetComponent<SelectionComponent>().SetSelected(false);
            selectionRegister.Remove(unitId);
        }

        public void DeselectAll()
        {
            
            for (int i = 0; i < selectionRegister.Count; i++)
            {
                (int key, GameObject value) = selectionRegister.ElementAt(i);
                if (value == null) continue;
                selectionRegister[key].GetComponent<SelectionComponent>().SetSelected(false);
            }
            selectionRegister.Clear();
        }
        
        //REGIMENT SECTION

        private GameObject cachedRegiment;
        private GameObject GetRegimentFromUnit(GameObject unit) => unit.transform.parent.gameObject;

        public void AddRegimentSelection(GameObject unit)
        {
            cachedRegiment = GetRegimentFromUnit(unit);
            int regimentId = cachedRegiment.GetInstanceID();
            Debug.Log($"try add {cachedRegiment.name} id : {regimentId}");
            if (!selectionRegister.ContainsKey(regimentId))
            {
                selectionRegister.Add(regimentId, cachedRegiment);
                cachedRegiment.GetComponent<RegimentComponent>().SetSelected(true);
            }
        }
        
        public void DeselectSingleRegiment(GameObject unit)
        {
            cachedRegiment = GetRegimentFromUnit(unit);
            int regimentId = cachedRegiment.GetInstanceID();
            
            selectionRegister[regimentId].GetComponent<RegimentComponent>().SetSelected(false);
            selectionRegister.Remove(regimentId);
        }
        
        public void DeselectAllRegiment()
        {
            
            for (int i = 0; i < selectionRegister.Count; i++)
            {
                (int key, GameObject value) = selectionRegister.ElementAt(i);
                if (value == null) continue;
                Debug.Log(selectionRegister[key].TryGetComponent(out RegimentComponent comp));
                selectionRegister[key].GetComponent<RegimentComponent>().SetSelected(false);
            }
            selectionRegister.Clear();
        }
    }
}
