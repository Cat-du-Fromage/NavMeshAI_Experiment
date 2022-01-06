using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaizerWaldCode.RTSSelection
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
    }
}
