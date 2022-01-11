using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using KaizerWaldCode.RTTUnitPlacement;
using KaizerWaldCode.Utils;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTUnits
{
    public class RegimentComponent : MonoBehaviour
    {
        [SerializeField] private RegimentType regimentType;
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private GameObject positionTokenPrefab;

        public Transform[] Units { get; private set; }
        public Transform[] PositionTokens { get; private set; }

        private Transform regimentTransform;
        public Vector3 UnitSize { get; private set; }
        
        public bool IsSelected { get; private set; }
        public bool IsPreselected { get; private set; }
        
        public int CurrentSize { get => Units.Length;}
        public RegimentType GetRegimentType { get => regimentType;}

        //Unity Event
        //==============================================================================================================
        
        private void Awake()
        {
            Units = new Transform[regimentType.baseNumUnits];
            PositionTokens = new Transform[regimentType.baseNumUnits];
            
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

                Units[i] = CreateUnit(i, newPos);
                PositionTokens[i] = CreatePositionToken(i, newPos);
            }
        }

        /// <summary>
        /// Create a single Unit
        /// </summary>
        /// <param name="index">index of the unit in the regiment</param>
        /// <param name="position">position to spawn</param>
        /// <returns></returns>
        private Transform CreateUnit(int index, Vector3 position)
        {
            GameObject newUnit = Instantiate(unitPrefab, position, regimentTransform.rotation) ;
            newUnit.name = $"{regimentTransform.name}_{unitPrefab.name}{index}";
            newUnit.GetComponent<UnitComponent>().SetRegiment(transform);
            return newUnit.transform;
        }
        
        /// <summary>
        /// Create a single Unit
        /// </summary>
        /// <param name="index">index of the unit in the regiment</param>
        /// <param name="position">position to spawn</param>
        /// <returns></returns>
        private Transform CreatePositionToken(int index, Vector3 position)
        {
            Vector3 tokenPosition = position;
            tokenPosition.y -= UnitSize.y * 0.9f - 1; // -1 because terrain height
            
            GameObject newToken = Instantiate(positionTokenPrefab, tokenPosition, regimentTransform.rotation) ;
            newToken.name = $"{unitPrefab.name}{index}_{positionTokenPrefab.name}";
            newToken.GetComponent<PositionTokenComponent>().AttachToUnit(Units[index]);
            return newToken.transform;
        }
        
        //SetSelected(bool) : select/deselect all units
        public void SetSelected(bool enable)
        {
            IsSelected = enable;
            for (int i = 0; i < CurrentSize; i++)
                Units[i].GetComponent<SelectionComponent>().SetSelected(enable);
        }
        
        //SetPreselected(bool) : select/deselect all units
        public void SetPreselected(bool enable)
        {
            IsPreselected = enable;
            for (int i = 0; i < CurrentSize; i++)
                Units[i].GetComponent<SelectionComponent>().SetPreselected(enable);
        }

    }
}
