using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTSelection
{
    public class SelectionComponent : MonoBehaviour
    {
        public bool isSelected = false;

        public bool IsSelected => isSelected;
        
        public void SetSelected(bool state)
        {
            GetComponent<Renderer>().material.color = state ? Color.red : Color.grey;
            isSelected = state;
        }
    }
}
