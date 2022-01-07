using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTSelection
{
    public class SelectionComponent : MonoBehaviour
    {
        //[SerializeField] private GameObject selectHighlight;
        //[SerializeField] private GameObject preSelectHighlight;

        [SerializeField]private Renderer selectRender;
        [SerializeField]private Renderer preSelectRender;
        
        public bool isSelected = false;
        public bool isPreSelected = false;
        public bool IsSelected => isSelected;

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
        
        public void SetPreSelected(bool state)
        {
            preSelectRender.enabled = state;
            isSelected = state;
        }

        private void OnMouseOver()
        {
            preSelectRender.enabled = true;
        }

        private void OnMouseExit()
        {
            preSelectRender.enabled = false;
        }
    }
}
