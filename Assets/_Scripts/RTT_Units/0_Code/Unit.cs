using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PlayerEntityInteractions;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    [Serializable]
    public class UnitData
    {
        private int Health;
        private int MeleeAttack;
        private int RangeAttack;
        private int Range;
    }
    
    //Reference to a regiment
    //Contain Only Mutable Data
    public class Unit : MonoBehaviour
    {
        [SerializeField] private Renderer selectionRenderer;
        public int Index { get; private set; }
        public Regiment Regiment { get; private set; }
        
        public ref readonly Renderer GetSelectionRenderer => ref selectionRenderer;
        public void SetRegiment(in Regiment regiment) => Regiment = regiment;
        public void SetIndex(int index) => Index = index;

        private void Awake()
        {
            selectionRenderer ??= GetComponentInChildren<SelectionTag>().transform.GetComponent<Renderer>();
        }
    }
}
