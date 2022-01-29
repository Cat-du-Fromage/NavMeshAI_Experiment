using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    
    
    public class InteractionSystem : MonoBehaviour, IMainSystem<Regiment>
    {
        [SerializeField] private RegimentsRegister regimentsRegister;
        
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private PlacementManager placementManager;
        
        private void Awake()
        {
            regimentsRegister ??= FindObjectOfType<RegimentsRegister>();
            
            selectionManager ??= GetComponent<SelectionManager>();
            placementManager ??= GetComponent<PlacementManager>();
            
            (selectionManager as ISelector<Regiment>).AttachSubSystemTo(this);
            (placementManager as IPlacement<Regiment>).AttachSubSystemTo(this);
        }

        //SELECTION SUBSYSTEM
        public void NotifyEntitySelected(ISelector<Regiment> sender, Regiment entitySelected)
        {
            placementManager.UpdateSelectionData(entitySelected);
            regimentsRegister.AddSelection(entitySelected);
        }
        
        public void NotifyClearSelections(ISelector<Regiment> sender)
        {
            regimentsRegister.ClearSelection();
            placementManager.ClearSelectionData();
        }
        
        //PLACEMENT SUBSYSTEM
        public void NotifyDestinationSet(IPlacement<Regiment> sender, Dictionary<Regiment,Transform[]> nextDestinations)
        {
            foreach ((Regiment regiment, Transform[] unitsDestinations) in nextDestinations)
            {
                regiment.SetNewDestination(unitsDestinations);
            }
        }

        public void NotifyDisplayTokens(IPlacement<Regiment> sender, bool enable) => regimentsRegister.DisplayRegimentsPositions(enable);
    }
}
