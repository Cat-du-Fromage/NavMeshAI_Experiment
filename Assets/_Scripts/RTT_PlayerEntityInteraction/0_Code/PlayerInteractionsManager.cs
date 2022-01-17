using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PlayerEntityInteractions.RTTSelection;
using KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement;
using KaizerWaldCode.RTTUnits;
using Unity.Collections;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    /// <summary>
    /// CONCERN : Player Interactions
    /// - Selection / Deselection
    /// - Placement
    /// </summary>
    public class PlayerInteractionsManager : MonoBehaviour //Turn this into a Mediator!! and the other as IManager
    {
        [SerializeField] private RegimentManager regimentManager;
        [SerializeField] private PlacementManager placementManager;

        private SelectionData SelectionData;
        
        //needed by Placement + Selection
        private List<Regiment> Selections;
        public ref readonly List<Regiment> GetSelections => ref Selections;
        
        public List<Renderer> NextPlacementTokens { get; private set; } = new List<Renderer>();

        private void Awake()
        {
            Selections = new List<Regiment>(1);
            SelectionData = new SelectionData();
            regimentManager = FindObjectOfType<RegimentManager>();
            placementManager = FindObjectOfType<PlacementManager>();
        }

        private void Start()
        {
            //Select Deselect
            PlayerInteractionsSystem.Instance.OnSingleSelection += OnSingleRegimentSelected;
            PlayerInteractionsSystem.Instance.OnSelectionClear += OnClearSelection;
            
            //Placement
            PlayerInteractionsSystem.Instance.OnPlaceEntity += OnStartPlacement;
            PlayerInteractionsSystem.Instance.OnEndPlaceEntity += OnEndPlacement;
            
            PlayerInteractionsSystem.Instance.OnShowEntitiesPlacement += ShowNestedPlacements;
            PlayerInteractionsSystem.Instance.OnHideEntitiesPlacement += HideNestedPlacements;
        }

        private void OnDestroy()
        {
            //Select Deselect
            PlayerInteractionsSystem.Instance.OnSingleSelection -= OnSingleRegimentSelected;
            PlayerInteractionsSystem.Instance.OnSelectionClear -= OnClearSelection;
            
            //Placement
            PlayerInteractionsSystem.Instance.OnPlaceEntity -= OnStartPlacement;
            PlayerInteractionsSystem.Instance.OnEndPlaceEntity -= OnEndPlacement;
            
            PlayerInteractionsSystem.Instance.OnShowEntitiesPlacement -= ShowNestedPlacements;
            PlayerInteractionsSystem.Instance.OnHideEntitiesPlacement -= HideNestedPlacements;
        }
        

        //SELECTION / DESELECTION
        //==============================================================================================================
        private void OnSingleRegimentSelected(Regiment regiment)
        {
            if (Selections.Contains(regiment)) return;
            Selections.Add(regiment);
            SelectionData.OnAddRegiment(regiment);
            regiment.SetSelected(true);
            
            placementManager.SetSelectionData(SelectionData);
        }

        private void OnClearSelection()
        {
            foreach (Regiment regiment in Selections)
            {
                regiment.SetSelected(false);
            }
            SelectionData.OnClearRegiment();
            Selections.Clear();
            
            placementManager.SetSelectionData(SelectionData);
        }
        
        //PLACEMENT
        //==============================================================================================================
        
        private void OnStartPlacement()
        {
            for (int i = 0; i < Selections.Count; i++)
            {
                Selections[i].EnablePlacementToken(true);
            }
        }

        private void OnEndPlacement()
        {
            for (int i = 0; i < Selections.Count; i++)
            {
                Selections[i].SetNewDestination();
                Selections[i].EnablePlacementToken(false);
            }
        }

        private void ShowNestedPlacements() => regimentManager.UpdateNestedPlacementTokens(true);

        private void HideNestedPlacements() => regimentManager.UpdateNestedPlacementTokens(false);
    }
}