using Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KaizerWaldCode.PlayerEntityInteractions;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement;
using KWUtils;
using Unity.VisualScripting;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    public class Regiment : MonoBehaviour
    {
        public int Index { get; private set; }
        public bool IsSelected { get; private set; } = false;
        
        [SerializeField] private RegimentType regimentType; //Immutable DATA
        [SerializeField] private UnitType unitType; //Immutable DATA
        
        private Transform regimentTransform;

        public Leader Leader { get; private set; }
        public List<Unit> Units { get; private set; }
        public List<Transform> DestinationTokens { get; private set; }
        
        public RegimentType GetRegimentType => regimentType;
        public UnitType GetUnitType => unitType;
        public int CurrentSize => Units.Count;
        
        //TEST FEATURE
        public int PreviousRowFormation = 0;
        public int CurrentRowFormation = 0;
        //private bool UnitMustMove = false;

        //Unity Event
        //==============================================================================================================
        
        private void Awake()
        {
            Index = GetInstanceID();
            Units = new List<Unit>(regimentType.baseNumUnits);
            DestinationTokens = new List<Transform>(regimentType.baseNumUnits);
            regimentTransform = transform;
            
            CreateRegimentMembers();
            InitPlacementTokens();
            SetSelected(false);
        }

        //Methods
        //==============================================================================================================

        public int GetCurrentRowFormation()
        {
            int numRow = 0;
            Vector3 normalDirection = (DestinationTokens[1].position - DestinationTokens[0].position).normalized;
            for (int i = 1; i < DestinationTokens.Count; i++)
            {
                if ( (DestinationTokens[i + 1].position - DestinationTokens[i].position).normalized != normalDirection )
                {
                    numRow = i; 
                    break;
                }
            }
            return numRow + 1; //+1 because [0] is not included
        }

        public void SetLeader(Leader leader) => Leader = leader;

        private Vector3 GetUnitPosition(in Vector3 startPos, int index)
        {
            (int x, int y) = index.GetXY(regimentType.maxRow/2);
            Vector3 newPos = startPos;
            newPos.x = (startPos.x) + (unitType.unitWidth + regimentType.offsetInRow) * (x+1);
            newPos.y = 1f; //real unit size not the token
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
        
        private void InitPlacementTokens()
        {
            for (int i = 0; i < Units.Count; i++)
            {
                //"nested"
                DestinationTokens.Add(Instantiate(unitType.positionTokenPrefab).transform);
                DestinationTokens[i].AddComponent<PositionTokenComponent>().AttachToUnit(Units[i].transform);
                DestinationTokens[i].name = "NestedPositionToken";
                DestinationTokens[i].gameObject.SetActive(false);
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

        public void SetNewDestination(in Transform[] newDestination)
        {
            PreviousRowFormation = GetCurrentRowFormation();
            for (int i = 0; i < DestinationTokens.Count; i++)
            {
                DestinationTokens[i].position = newDestination[i].position;
                //CAREFUL NEED TO SET TO FALSE WHEN ARRIVED!
                DestinationTokens[i].GetComponent<PositionTokenComponent>().SetDestination(true);
            }
            CurrentRowFormation = GetCurrentRowFormation();
            Debug.Log($"current numRow = {CurrentRowFormation}");
        }

        /// <summary>
        /// DESTINATION : Enable/Disable
        /// </summary>
        /// <param name="enable"></param>
        public void DisplayDestination(bool enable)
        {
            for (int i = 0; i < DestinationTokens.Count; i++)
            {
                DestinationTokens[i].gameObject.SetActive(enable);
            }
        }

        /// <summary>
        /// SELECTION : Enable/Disable
        /// </summary>
        /// <param name="enable"></param>
        public void SetSelected(bool enable)
        {
            IsSelected = enable;
            for (int i = 0; i < Units.Count; i++)
            {
                Units[i].GetSelectionRenderer.enabled = enable;
            }
        }
    }
}
