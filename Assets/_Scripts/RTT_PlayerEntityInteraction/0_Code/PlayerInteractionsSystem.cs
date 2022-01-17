using System;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using KWUtils;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public abstract class InteractionSystem<T> : Singleton<InteractionSystem<T>>
        where T : MonoBehaviour
    {
        public event Action<T> OnSingleSelection;
        public event Action OnSelectionClear;
        
        public event Action OnPlaceEntity;
        public event Action OnEndPlaceEntity;
        
        public event Action OnShowEntitiesPlacement;
        public event Action OnHideEntitiesPlacement;

        public void SelectEntity(in T entity) => OnSingleSelection?.Invoke(entity);
        public void ClearSelections() => OnSelectionClear?.Invoke();
        public void StartPlaceEntity() => OnPlaceEntity?.Invoke();
        public void ShowEntitiesPlacement() => OnShowEntitiesPlacement?.Invoke();
        public void HideEntitiesPlacement() => OnHideEntitiesPlacement?.Invoke();
        public void EndPlaceEntity() => OnEndPlaceEntity?.Invoke();
    }
    
    //==================================================================================================================
    
    public class PlayerInteractionsSystem : InteractionSystem<Regiment>
    {
        public static PlayerInteractionsSystem Instance { get; private set; }
        
        private PlayerEntityInteractionInputsManager InteractionsInputs;
        public ref PlayerEntityInteractionInputsManager GetInputs => ref InteractionsInputs;

        protected override void Awake()
        {
            InteractionsInputs = GetComponent<PlayerEntityInteractionInputsManager>();
            if (InteractionsInputs == null) FindObjectOfType<PlayerEntityInteractionInputsManager>();
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}