using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTSelection;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class RegimentComponent : MonoBehaviour
    {
        //Properties
        
        //base size
        //current size
        //UnitType : Gameobject
        
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private int baseSize = 1;
        
        private Transform regimentTransform;

        public bool SelectState; //{ get; private set; }
        
        private int CurrentSize => transform.childCount;
        
        //Unity Event
        //==============================================================================================================
        
        private void Awake()
        {
            regimentTransform = transform;
            CreateRegimentMembers();
        }

        //Methods
        //==============================================================================================================
        
        //CreateUnitMembers : create units gameobject as children
        private void CreateRegimentMembers()
        {
            Vector3 startPos = regimentTransform.position;
            
            for (int i = 0; i < baseSize; i++)
            {
                Vector3 pos = startPos + new Vector3(startPos.x + (i * 1.5f), 2f, startPos.z + 3f);
                Instantiate(unitPrefab, pos, regimentTransform.rotation, regimentTransform); //param regTransform set unit as children of the regiment
            }
        }
        
        //SetSelected(bool) : select/deselect all units
        public void SetSelected(bool enable)
        {
            SelectState = enable;
            for (int i = 0; i < regimentTransform.childCount; i++)
            {
                regimentTransform.GetChild(i).GetComponent<SelectionComponent>().SetSelected(enable);
            }
        }

    }
}
