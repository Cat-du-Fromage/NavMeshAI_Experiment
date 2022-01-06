using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTSSelection
{
    public class SelectionComponent : MonoBehaviour
    {
        private bool isSelected = false;

        public bool IsSelected => isSelected;
        
        public void SetSelected(bool state)
        {
            GetComponent<Renderer>().material.color = state ? Color.red : Color.grey;
            isSelected = state;
        }
    }
}
