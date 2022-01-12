using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{

    public class SelectionComponent : MonoBehaviour, ISelectable
    {
        
        [SerializeField]private Renderer selectRender;
        [SerializeField]private Renderer preSelectRender;
        
        public bool IsPreselected { get; private set; }
        public bool IsSelected { get; private set; }

        private void Awake() => selectRender.enabled = preSelectRender.enabled = IsSelected = false;

        public void SetSelected(bool state)
        {
            selectRender.enabled = state;
            IsSelected = state;
        }
        
        public void SetPreselected(bool state)
        {
            preSelectRender.enabled = state;
            IsPreselected = state;
        }
        
    }
}
