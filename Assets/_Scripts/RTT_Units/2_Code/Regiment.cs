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
        [SerializeField] private RegimentType regimentType; //Immutable DATA
        [SerializeField] private UnitType unitType; //Immutable DATA
        
        private Transform regimentTransform;
        public List<Unit> Units { get; private set; }
        public List<Renderer> SelectionTokensRenderers { get; private set; }
        public List<Renderer> PlacementTokensRenderers{ get; private set; }
        public List<Transform> PlacementTokens { get; private set; }
        public bool IsSelected { get; private set; } = false;
        
        public ref readonly RegimentType GetRegimentType => ref regimentType;
        public ref readonly UnitType GetUnit => ref unitType;
        public int CurrentSize => Units.Count;

        //Unity Event
        //==============================================================================================================
        
        private void Awake()
        {
            Units = new List<Unit>(regimentType.baseNumUnits);
            SelectionTokensRenderers = new List<Renderer>(regimentType.baseNumUnits);
            PlacementTokensRenderers = new List<Renderer>(regimentType.baseNumUnits);
            PlacementTokens = new List<Transform>(regimentType.baseNumUnits);
            regimentTransform = transform;
            
            CreateRegimentMembers();
            InitSelectionRenderers();
            InitPlacementTokens();
            SetSelected(false);
            EnablePlacementToken(false);
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

        private void InitSelectionRenderers()
        {
            for (int i = 0; i < Units.Count; i++)
                SelectionTokensRenderers.Add(Units[i].GetSelectionRenderer);
        }

        private void InitPlacementTokens()
        {
            for (int i = 0; i < Units.Count; i++)
            {
                PlacementTokensRenderers.Add(Instantiate(Units[i].GetPlacementToken));
                PlacementTokens.Add(PlacementTokensRenderers[i].transform);
                PlacementTokensRenderers[i].enabled = true;
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

        //SetSelected(bool) : select/deselect all units
        public void SetSelected(bool enable)
        {
            IsSelected = enable;
            for (int i = 0; i < SelectionTokensRenderers.Count; i++) SelectionTokensRenderers[i].enabled = enable;
        }
        
        public void EnablePlacementToken(bool enable)
        {
            for (int i = 0; i < PlacementTokens.Count; i++) PlacementTokens[i].gameObject.SetActive(enable);
            //for (int i = 0; i < PlacementTokensRenderers.Count; i++) PlacementTokensRenderers[i].enabled = enable;
        }
    }
}
