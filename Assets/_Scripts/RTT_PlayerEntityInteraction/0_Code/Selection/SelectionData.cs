using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    //[Serializable]
    public class SelectionData
    {
        private List<Regiment> Selections;
        public ref readonly List<Regiment> GetSelections => ref Selections;
        
        private const float SpaceBetweenRegiment = 2.5f;
        public int NumSelection { get; private set; } = 0;
        public float MinRowLength { get; private set; } = 0;
        public float MaxRowLength { get; private set;} = 0;
        public float StartDragPlaceLength{ get; private set; } = 0;
        public int SelectionMaxUniPerRow{ get; private set; } = 0;

        public SelectionData()
        {
            Selections = new List<Regiment>(1);
        }

        private float GetMaxRowLength(Regiment regiment) => (regiment.GetUnitType.unitWidth + regiment.GetRegimentType.offsetInRow) * regiment.GetRegimentType.maxRow;
        private float GetMinRowLength(in List<Regiment> selections) => selections.Count == 0 ? 0 : (selections[0].GetUnitType.unitWidth + selections[0].GetRegimentType.offsetInRow) * (selections[0].GetRegimentType.minRow);

        public void GetSelectionMaxUniPerRow()
        {
            SelectionMaxUniPerRow = 0;
            foreach (Regiment r in Selections)
            {
                SelectionMaxUniPerRow += r.GetRegimentType.maxRow;
            }
        }
        
        public void GetStartDragPlaceLength()
        {
            StartDragPlaceLength = 0;
            for (int i = 0; i < NumSelection; i++)
            {
                RegimentType type = Selections[i].GetRegimentType;
                StartDragPlaceLength += (Selections[i].GetUnitType.unitWidth + type.offsetInRow) * (type.minRow - 1);
            }
            StartDragPlaceLength += (Selections.Count - 1) * SpaceBetweenRegiment;
        }
        
        //SIDE EFFECT
        public void OnAddRegiment(in Regiment regiment)
        {
            //Selections = selections;
            Selections.Add(regiment);
            NumSelection += 1;
            if (NumSelection == 1)
            {
                MinRowLength = GetMinRowLength(Selections);
                MaxRowLength += GetMaxRowLength(regiment);
            }
            else
                MaxRowLength += GetMaxRowLength(regiment) + SpaceBetweenRegiment;

            GetSelectionMaxUniPerRow();
            GetStartDragPlaceLength();
        }
        /*
        public void OnRemoveRegiment(Regiment regiment)
        {
            MinRowLength = Selections.Count == 0 ? 0 : MinRowLength;
            if (Selections.Count == 0)
                MaxRowLength = MinRowLength = 0;
            else if (Selections.Count == 1)
                MaxRowLength = MinRowLength;
            else
                MaxRowLength -= GetMaxRowLength(regiment) + SpaceBetweenRegiment;
        }
*/
        public void OnClearRegiment()
        {
            Selections.Clear();
            MinRowLength = MaxRowLength = NumSelection = 0;
        }
    }
}
