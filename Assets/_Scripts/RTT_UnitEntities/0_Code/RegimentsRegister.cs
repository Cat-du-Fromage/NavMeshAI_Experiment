using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class VisualIndicatorEvent
    {
        //SELECTION
        //==============================================================================================================
        public void OnRegimentSelected(in Regiment regiment) => regiment.SetSelected(true);

        public void OnClearSelections(in HashSet<Regiment> selections)
        {
            foreach (Regiment regiment in selections)
            {
                regiment.SetSelected(false);
            }
        }
        //PLACEMENT
        //==============================================================================================================
        public void DisplayPositionsTokens(in List<Regiment> allRegiments, bool enable)
        {
            for (int i = 0; i < allRegiments.Count; i++)
            {
                allRegiments[i].DisplayDestination(enable);
            }
        }
    }
    
    public class RegimentsRegister : MonoBehaviour
    {
        public VisualIndicatorEvent VisualIndicatorEvent;
        
        public List<Regiment> Regiments;
        public HashSet<Leader> RegimentsLeader;
        public HashSet<Regiment> SelectedRegiments;
        public HashSet<Regiment> MovingRegiments;

        public event Action<Regiment> OnAddSelection;
        public event Action OnClearSelections;
        public event Action<Regiment> OnMovingRegimentsChanged;
        
        private void Awake()
        {
            VisualIndicatorEvent = new VisualIndicatorEvent();
                
            Regiments = new List<Regiment>();
            RegimentsLeader = new HashSet<Leader>(1);
            SelectedRegiments = new HashSet<Regiment>(1);
            MovingRegiments = new HashSet<Regiment>(1);
        }

        public void DisplayRegimentsPositions(bool enable) => VisualIndicatorEvent.DisplayPositionsTokens(Regiments, enable);

        public void AddSelection(Regiment regiment)
        {
            if (SelectedRegiments.Add(regiment))
            {
                VisualIndicatorEvent.OnRegimentSelected(regiment);
                OnAddSelection?.Invoke(regiment);
            }
        }

        public void ClearSelection()
        {
            VisualIndicatorEvent.OnClearSelections(SelectedRegiments);
            OnClearSelections?.Invoke();
            SelectedRegiments.Clear();
        }

        public void AddMovingRegiments(Regiment regiment)
        {
            OnMovingRegimentsChanged?.Invoke(regiment);
        }

    }
}