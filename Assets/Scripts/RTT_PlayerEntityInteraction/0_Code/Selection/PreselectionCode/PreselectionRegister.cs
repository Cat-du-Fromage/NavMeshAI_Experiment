using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{
    public class PreselectionRegister : MonoBehaviour
    {
        
        public HashSet<Transform> Preselections { get; } = new HashSet<Transform>();
        
        //private Transform CurrentPreselection;
        //REGIMENT SECTION

        //PRESELECTION
        
        public void Add(Transform regiment)
        {
            Preselections.Add(regiment);
            regiment.GetComponent<RegimentComponent>().SetPreselected(true);
        }
        
        public void Remove(Transform regiment)
        {
            Preselections.Remove(regiment);
            regiment.GetComponent<RegimentComponent>().SetPreselected(false);
        }
        
        public void Clear()
        {
            foreach (Transform regiment in Preselections)
            {
                regiment.GetComponent<RegimentComponent>().SetPreselected(false);
            }
            Preselections.Clear();
        }
        
        
        /*
         private bool TestBoxCast()
        {
            Vector3 startM = SelectionInputs.StartMouseClick;
            Vector3 endM = SelectionInputs.EndMouseClick[1];

            RaycastHit[] hits = new RaycastHit[2];
            Ray rayStart = PlayerCamera.ScreenPointToRay(startM);
            if (!Raycast(rayStart, out hits[0], INFINITY, TerrainLayer)) return false;
            Ray rayEnd  = PlayerCamera.ScreenPointToRay(endM);
            if (!Raycast(rayEnd, out hits[1], INFINITY, TerrainLayer)) return false;
            
            float castLen = length(hits[1].point - hits[0].point)/2.5f;
            
            dirCube = PlayerCamera.ScreenPointToRay(SelectionInputs.EndMouseClick[1]).direction;
            HalfExtend = new Vector3(castLen, castLen, castLen);
            rotation = PlayerCamera.transform.rotation;
            
            bool hitUnit= BoxCast(SingleRay.origin, HalfExtend, SingleRay.direction, Quaternion.identity,
                Mathf.Infinity, UnitLayer);

            return hitUnit;
        }
         */
    }
}
