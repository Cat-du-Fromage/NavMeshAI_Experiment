using System;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    public class PlacementComponent : MonoBehaviour, IAttachable<Unit>
    {
        public Regiment ParentEntity; //{ get; private set; }
        public List<Transform> PlacementTokens { get; private set; }
        
        public List<Renderer> PlacementRenderers { get; private set; }
        
        public bool IsSelected { get; private set; }
        
        private void Awake() => ParentEntity = GetComponent<Regiment>();

        

        /// <summary>
        /// Create a PositioToken Unit
        /// </summary>
        public void AttachTo(Unit unit) //Index Is a problem!! unit got the index?
        {
            Transform unitTransform = unit.transform;
            
            PlacementTokens ??= new List<Transform>(ParentEntity.GetRegimentType.baseNumUnits);
            PlacementRenderers ??= new List<Renderer>(ParentEntity.GetRegimentType.baseNumUnits);
            
            Vector3 tokenPosition = unitTransform.position;
            
            tokenPosition.y -= 2f * 0.9f - 1; // -1 because terrain height
            
            GameObject newToken = Instantiate(unit.Regiment.GetUnit.positionTokenPrefab, tokenPosition, ParentEntity.transform.rotation);
            
            newToken.name = $"{unit.Regiment.GetUnit.unitPrefab.name}{unit.Index}_{unit.Regiment.GetUnit.positionTokenPrefab.name}";
            
            PlacementTokens.Add(newToken.transform);
            
            PlacementRenderers.Add(newToken.GetComponent<Renderer>());
        }
        
        public void SetVisible(bool state)
        {
            IsSelected = state;
            PlacementRenderers.ForEach(select => select.enabled = state);
        }
    }
    
}