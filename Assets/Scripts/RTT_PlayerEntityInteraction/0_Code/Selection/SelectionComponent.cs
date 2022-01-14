using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using Unity.Collections;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{

    /// <summary>
    /// Attach To a regiment
    /// Must control whether or not a regiment AND it's units are selected
    /// All Select and deselect will be processed in this class!
    ///
    /// JOB
    /// Give Entity an Selection UI
    /// Enable / Disable Selection UI
    /// </summary>
    public class SelectionComponent : MonoBehaviour, IInteractable, IAttachable<Unit>
    {
        [SerializeField] private GameObject selectPrefab; //may become a list on the future
        
        public List<Renderer> SelectionTokens { get; private set; }
        public bool IsSelected { get; private set; } = false;

        private void Awake() => SelectionTokens = new List<Renderer>();

        

        public void AttachTo(Unit unit)
        {
            Transform unitTransform = unit.transform;
            
            Transform entityTransform = Instantiate(selectPrefab, unitTransform.position, Quaternion.identity, unitTransform).transform;
            entityTransform.localPosition += Vector3.down * (unit.Regiment.GetUnit.unitHeight/2) * 0.9f;
            
            Renderer entityRenderer = entityTransform.GetComponent<Renderer>();
            SelectionTokens.Add(entityRenderer);
            entityRenderer.enabled = false;
        }

        public void SetSelected(bool state)
        {
            IsSelected = state;
            SelectionTokens.ForEach(select => select.enabled = state);
        }
    }
}
