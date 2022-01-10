using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{
    public interface ISelectable
    {
        public Renderer SelectRender { get; }

        public bool IsSelected { get; set; }
        public void SetSelected(bool state)
        {
            SelectRender.enabled = state;
            IsSelected = state;
        }
    }

    public class SelectionComponent : MonoBehaviour, ISelectable
    {
        
        [SerializeField]private Renderer selectRender;
        [SerializeField]private Renderer preSelectRender;
        
        public bool isPreselected = false;
        //private bool isSelected = false;
        Renderer ISelectable.SelectRender => selectRender;
        public bool IsSelected { get; set; }
        public bool IsPreselected => isPreselected;
        
        private void Awake()
        {
            selectRender.enabled = false;
            preSelectRender.enabled = false;
        }
/*
        public void SetSelected(bool state)
        {
            selectRender.enabled = state;
            isSelected = state;
        }
        */
        public void SetPreselected(bool state)
        {
            preSelectRender.enabled = state;
            isPreselected = state;
        }
        
    }
}
