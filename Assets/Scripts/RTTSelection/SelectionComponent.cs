using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTSelection
{
    public class SelectionComponent : MonoBehaviour
    {
        [SerializeField]private Renderer selectRender;
        [SerializeField]private Renderer preSelectRender;
        
        public bool isSelected = false;
        public bool isPreselected = false;
        public bool IsSelected => isSelected;
        public bool IsPreselected => isPreselected;
        
        private void Awake()
        {
            selectRender.enabled = false;
            preSelectRender.enabled = false;
        }

        public void SetSelected(bool state)
        {
            selectRender.enabled = state;
            isSelected = state;
        }
        
        public void SetPreselected(bool state)
        {
            preSelectRender.enabled = state;
            isPreselected = state;
        }
    }
}
