using System;
using UnityEngine;
using KaizerWaldCode.RTTUnits;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement;

namespace KaizerWaldCode
{
    public class InteractionEvents
    {
        private SelectionManager SelectionManager;
        private PlacementManager PlacementManager;

        public InteractionEvents(SelectionManager selection, PlacementManager placement)
        {
            SelectionManager = selection;
            PlacementManager = placement;
        }
 
        public void Dispatch(SelectionManager selectionManager)
        {
            
        }
        
        public void Dispatch(PlacementManager placementManager)
        {
            
        }
    }
    
    public class Porto_InteractionSystem : MonoBehaviour
    {
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private PlacementManager placementManager;

        private InteractionEvents InteractionEvents;

        private void Awake()
        {
            InteractionEvents = new InteractionEvents(selectionManager, placementManager);
        }

        //SELECTION MANAGER
        public void NotifyEntitySelected(Regiment entitySelected)
        {
            //regimentManager.OnRegimentSelected(entitySelected);
            placementManager.UpdateSelectionData(entitySelected);
        }
        
        public void NotifyClearSelections()
        {
            //regimentManager.OnClearSelections();
            placementManager.ClearSelectionData();
        }
    }
}