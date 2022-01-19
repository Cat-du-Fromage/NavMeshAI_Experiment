using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public class InteractionSystem : MonoBehaviour, IMediator<Regiment>
    {
        [SerializeField] private RegimentManager regimentManager;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private PlacementManager placementManager;

        private void Awake()
        {
            regimentManager ??= GetComponent<RegimentManager>();
            selectionManager ??= GetComponent<SelectionManager>();
            placementManager ??= GetComponent<PlacementManager>();
            
            IEntityGroup<Regiment> entityGroup = regimentManager;
            entityGroup.SetMediator(this);
            ISelector<Regiment> selector = selectionManager;
            selector.SetMediator(this);
            IPlacement<Regiment> placement = placementManager;
            placement.SetMediator(this);
        }

        //SELECTION MANAGER
        public void NotifyEntitySelected(ISelector<Regiment> sender, Regiment entitySelected)
        {
            regimentManager.OnRegimentSelected(entitySelected);
            placementManager.UpdateSelectionData(entitySelected);
        }
        
        public void NotifyClearSelections(ISelector<Regiment> sender)
        {
            regimentManager.OnClearSelections();
            placementManager.ClearSelectionData();
        }
        
        //PLACEMENT NOTIFIER

        public void NotifyDestinationSet(IPlacement<Regiment> sender, Dictionary<Regiment,Transform[]> nextDestinations)
        {
            regimentManager.OnDestinationSet(nextDestinations);
        }

        public void NotifyDisplayTokens(IPlacement<Regiment> sender, bool enable)
        {
            regimentManager.OnDisplayDestinationTokens(enable);
        }
    }
}
