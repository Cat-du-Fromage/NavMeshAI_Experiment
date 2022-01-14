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

        public List<Unit> Units;// { get; private set; }
        public List<IAttachable<Unit>> Attachables;

        private Transform regimentTransform;
        
        public bool IsSelected { get; private set; }
        
        public RegimentType GetRegimentType => regimentType;
        public UnitType GetUnit => unitType;
        public int CurrentSize  => Units.Count;
        public List<Transform> GetPlacementTokens => GetComponent<PlacementComponent>().PlacementTokens;
        
        //Unity Event
        //==============================================================================================================
        
        private void Awake()
        {
            Units = new List<Unit>(regimentType.baseNumUnits);
            Attachables = GetComponents<IAttachable<Unit>>().ToList();
            regimentTransform = transform;
        }

        private void Start() => CreateRegimentMembers();

        //Methods
        //==============================================================================================================

        Vector3 GetUnitPosition(Vector3 startPos, int index)
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
                Attachables.ForEach(attach => attach.AttachTo(Units[i]));
            }
        }

        /// <summary>
        /// Create a single Unit
        /// </summary>
        private Unit CreateUnit(int index, Vector3 position)
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
            GetComponent<IInteractable>().SetSelected(enable);
        }

    }
}
