using Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KaizerWaldCode.PlayerEntityInteractions;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement;
//using KaizerWaldCode.RTTUnitPlacement;
using KWUtils;
using Unity.VisualScripting;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    //Regiment?
    //Data Holder?
    //
    
    //SUR! : Reference Units!
    //SUR! : Reference Units Immutable DATA!
    
    public class Regiment : MonoBehaviour
    {
        public int Index { get; private set; }
        
        [SerializeField] private RegimentType regimentType; //Immutable DATA
        [SerializeField] private UnitType unitType; //Immutable DATA
        
        private Transform regimentTransform;
        public List<Unit> Units { get; private set; }
        //public List<Renderer> SelectionTokensRenderers { get; private set; }
        //public List<Renderer> DestinationTokensRenderers{ get; private set; }
        //public List<Transform> DestinationTokens { get; private set; }
        
        public List<Transform> NestedPositionTokens { get; private set; }
        public List<Renderer> NestedPositionTokenRenderers { get; private set; }
        public bool IsSelected { get; private set; } = false;
        
        public ref readonly RegimentType GetRegimentType => ref regimentType;
        public ref readonly UnitType GetUnit => ref unitType;
        public int CurrentSize => Units.Count;
        
        //SetSelected(bool) : select/deselect all units
        public void SetSelected(bool enable) => IsSelected = enable;
        
        //TEST FEATURE
        private bool UnitMustMove = false;

        //Unity Event
        //==============================================================================================================
        
        private void Awake()
        {
            Index = GetInstanceID();
            Units = new List<Unit>(regimentType.baseNumUnits);
            //SelectionTokensRenderers = new List<Renderer>(regimentType.baseNumUnits);
            //DestinationTokensRenderers = new List<Renderer>(regimentType.baseNumUnits);
            //DestinationTokens = new List<Transform>(regimentType.baseNumUnits);
            NestedPositionTokens = new List<Transform>(regimentType.baseNumUnits);
            NestedPositionTokenRenderers = new List<Renderer>(regimentType.baseNumUnits);
            regimentTransform = transform;
            
            CreateRegimentMembers();
            //InitSelectionRenderers();
            //InitDestinationTokens();
            InitPlacementTokens();
            SetSelected(false);
            //EnablePlacementToken(false);
        }

        //Methods
        //==============================================================================================================

        Vector3 GetUnitPosition(in Vector3 startPos, int index)
        {
            (int x, int y) = index.GetXY(regimentType.maxRow/2);
            Vector3 newPos = startPos;
            newPos.x = (startPos.x) + (unitType.unitWidth + regimentType.offsetInRow) * (x+1);
            newPos.y = 2f; //real unit size not the token
            newPos.z = startPos.z + (y+1);
            return newPos;
        }
        
        //CreateUnitMembers : create units gameobject as children
        private void CreateRegimentMembers() // Make a builder AND a factory!!
        {
            Vector3 startPos = regimentTransform.position;
            
            for (int i = 0; i < regimentType.baseNumUnits; i++)
            {
                Vector3 newPos = GetUnitPosition(startPos, i);
                Units.Add(CreateUnit(i, newPos));
                Units[i].SetIndex(i);
            }
        }
/*
        private void InitSelectionRenderers()
        {
            for (int i = 0; i < Units.Count; i++)
                SelectionTokensRenderers.Add(Units[i].GetSelectionRenderer);
        }
        */
        private void InitPlacementTokens()
        {
            for (int i = 0; i < Units.Count; i++)
            {
                //"nested"
                NestedPositionTokenRenderers.Add(Instantiate(unitType.positionTokenPrefab).GetComponent<Renderer>());
                NestedPositionTokenRenderers[i].AddComponent<PositionTokenComponent>().AttachToUnit(Units[i].transform);
                NestedPositionTokenRenderers[i].name = "NestedPositionToken";
                NestedPositionTokenRenderers[i].enabled = false;
                NestedPositionTokens.Add(NestedPositionTokenRenderers[i].transform);
            }
        }
        
        /// <summary>
        /// Create a single Unit
        /// </summary>
        private Unit CreateUnit(int index, in Vector3 position)
        {
            GameObject newUnit = Instantiate(unitType.unitPrefab, position, regimentTransform.rotation) ;
            Unit unit = newUnit.GetComponent<Unit>();
            newUnit.name = $"{regimentTransform.name}_{unitType.unitPrefab.name}{index}";
            unit.SetRegiment(this);
            return unit;
        }
    }
}
