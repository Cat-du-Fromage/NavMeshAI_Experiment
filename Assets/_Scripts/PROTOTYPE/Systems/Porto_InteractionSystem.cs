using System;
using KaizerWaldCode.PlayerEntityInteractions;
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

        public void Dispatch(PlacementManager placementManager, Regiment entitySelected = null)
        {
            if (entitySelected is null)
            {
                //placementManager.ClearSelectionData();
            }
            else
            {
                //placementManager.UpdateSelectionData(entitySelected);
            }
        }
        
        public void Dispatch(SelectionManager selectionManager, Regiment entitySelected = null)
        {
            if (entitySelected is null)
            {
                //selectionManager.;
            }
            else
            {
                //selectionManager.UpdateSelectionData(entitySelected);
            }
        }
    }
    
    public class Porto_InteractionSystem : MonoBehaviour
    {
        [SerializeField] private Proto_EntitySystem entitySystem;
        
        //SubSystems
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private PlacementManager placementManager;

        private InteractionEvents InteractionEvents;

        private void Awake()
        {
            selectionManager ??= GetComponent<SelectionManager>();
            placementManager ??= GetComponent<PlacementManager>();
            
            InteractionEvents = new InteractionEvents(selectionManager, placementManager);
        }

        //SELECTION MANAGER
        public void Notify(ISelector<Regiment> selection, Regiment entitySelected = null)
        {
            InteractionEvents.Dispatch(selectionManager, entitySelected);
        }
        
        public void Notify(IPlacement<Regiment> placement, Regiment entitySelected = null)
        {
            InteractionEvents.Dispatch(placementManager, entitySelected);
        }
    }
}