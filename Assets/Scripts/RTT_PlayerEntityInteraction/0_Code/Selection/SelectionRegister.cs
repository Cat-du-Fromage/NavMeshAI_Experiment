using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public class SelectionRegister : MonoBehaviour
    {
        public List<Regiment> Selections { get; private set; } = new List<Regiment>();

        private const float SpaceBetweenRegiment = 2.5f;

        public float MinRowLength; //{ get; private set; }
        public float MaxRowLength; //{ get; private set; }

        public float StartDragPlaceLength;
        public int SelectionMaxUniPerRow;
        private float GetMaxRowLength(Regiment regiment) => (regiment.GetUnit.unitWidth + regiment.GetRegimentType.offsetInRow) * regiment.GetRegimentType.maxRow;
        private float GetMinRowLength() => Selections.Count == 0 ? 0 : (Selections[0].GetUnit.unitWidth + Selections[0].GetRegimentType.offsetInRow) * (Selections[0].GetRegimentType.minRow);

        public int GetSelectionMaxUniPerRow()
        {
            int num = 0;
            foreach (Regiment r in Selections)
            {
                num += r.GetRegimentType.maxRow;
            }
            return num;
        }
        public float GetStartDragPlaceLength()
        {
            StartDragPlaceLength = 0;
            for (int i = 0; i < Selections.Count; i++)
            {
                RegimentType type = Selections[i].GetRegimentType;
                StartDragPlaceLength += (Selections[i].GetUnit.unitWidth + type.offsetInRow) * (type.minRow - 1);
            }
            StartDragPlaceLength += (Selections.Count - 1) * SpaceBetweenRegiment;
            return StartDragPlaceLength;
        }

        private void OnAddRegiment(Regiment regiment)
        {
            if (Selections.Count == 1)
            {
                MinRowLength = GetMinRowLength();
                MaxRowLength += GetMaxRowLength(regiment);
            }
            else
                MaxRowLength += GetMaxRowLength(regiment) + SpaceBetweenRegiment;
        }
        
        private void OnRemoveRegiment(Regiment regiment)
        {
            MinRowLength = Selections.Count == 0 ? 0 : MinRowLength;
            if (Selections.Count == 0)
                MaxRowLength = MinRowLength = 0;
            else if (Selections.Count == 1)
                MaxRowLength = MinRowLength;
            else
                MaxRowLength -= GetMaxRowLength(regiment) + SpaceBetweenRegiment;
        }

        private void OnClearRegiment() => MinRowLength = MaxRowLength = 0;

        public void Add(Regiment regiment)
        {
            if (!Selections.Contains(regiment))
            {
                Selections.Add(regiment);
                regiment.SetSelected(true);
                OnAddRegiment(regiment);
                Test();
            }
        }
        
        public void Remove(Regiment regiment)
        {
            if (Selections.Contains(regiment))
            {
                regiment.SetSelected(false);
                Selections.Remove(regiment);
                OnRemoveRegiment(regiment);
            }
        }
        
        public void Clear()
        {
            Selections.ForEach(regiment=> regiment.SetSelected(false));
            Selections.Clear();
            OnClearRegiment();
            Test();
        }
        
        
        
        private void Test()
        {
            StartDragPlaceLength = GetStartDragPlaceLength();
            SelectionMaxUniPerRow = GetSelectionMaxUniPerRow();
        }
    }
}
