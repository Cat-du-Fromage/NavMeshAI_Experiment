using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public class SelectionData
    {
        public List<Regiment> Selections { get;}
        
        public int NumRegiment { get; set; }
        public float MinRowLength { get; }
        public float MaxRowLength { get; } //numSelect -1 * spaceRegiment

        public void OnSelectionChange()
        {
            NumRegiment = 4;
        }
    }
}
