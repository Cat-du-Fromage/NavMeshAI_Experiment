using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTSelection;
using KaizerWaldCode.Utils;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    public class RegimentComponent : MonoBehaviour
    {
        [SerializeField] private RegimentType regimentType;
        [SerializeField] private GameObject unitPrefab;

        public Transform[] Units { get; private set; }

        private Transform regimentTransform;
        public Vector3 UnitSize { get; private set; }
        
        public bool SelectState { get; private set; }
        public bool PreselectState { get; private set; }
        
        public int CurrentSize { get => Units.Length;}
        public RegimentType GetRegimentType { get => regimentType;}

        //Unity Event
        //==============================================================================================================
        
        private void Awake()
        {
            Units = new Transform[regimentType.baseNumUnits];
            regimentTransform = transform;
            UnitSize = unitPrefab.GetComponentInChildren<MeshFilter>().sharedMesh.bounds.size; //z is also valid
            CreateRegimentMembers();
        }

        //Methods
        //==============================================================================================================
        
        //CreateUnitMembers : create units gameobject as children
        private void CreateRegimentMembers()
        {
            Vector3 startPos = regimentTransform.position;
            
            for (int i = 0; i < regimentType.baseNumUnits; i++)
            {
                (int x, int y) = KwGrid.GetXY(i, 10);

                Vector3 newPos = startPos;
                newPos.x = (startPos.x) + (UnitSize.x + regimentType.positionOffset) * (x+1);
                newPos.y = UnitSize.y;
                newPos.z = startPos.z + (y+1);
                //last parameter (regimentTransform) set unit as children of the regiment
                GameObject newUnit = Instantiate(unitPrefab, newPos, regimentTransform.rotation/*, regimentTransform*/);
                newUnit.name = $"{unitPrefab.name} {i}";
                newUnit.GetComponent<UnitComponent>().SetRegiment(transform);
                Units[i] = newUnit.transform;
            }
        }
        
        //SetSelected(bool) : select/deselect all units
        public void SetSelected(bool enable)
        {
            SelectState = enable;
            for (int i = 0; i < CurrentSize; i++)
                Units[i].GetComponent<SelectionComponent>().SetSelected(enable);
        }
        
        //SetSelected(bool) : select/deselect all units
        public void SetPreselected(bool enable)
        {
            PreselectState = enable;
            for (int i = 0; i < CurrentSize; i++)
                Units[i].GetComponent<SelectionComponent>().SetPreselected(enable);
                
        }

    }
}
