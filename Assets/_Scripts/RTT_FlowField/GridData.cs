using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using static KWUtils.KWmath;

namespace KaizerWaldCode.FlowField
{
    public readonly struct GridData
    {
        //GRID DATA
        public readonly int MapWidth;
        public readonly int MapHeight;

        public readonly int NumCellsX;
        public readonly int NumCellsY;
        
        public readonly float CellSize;
        public readonly int TotalCells => NumCellsX * NumCellsY;
        
        public GridData(int mapWidth, int mapHeight, float cellSize)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            NumCellsX = mapWidth;
            NumCellsY = mapHeight;
            CellSize = mapWidth / (float)NumCellsX;
        }
    }
}
