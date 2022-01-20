using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;

namespace KaizerWaldCode
{
    public class FlowFieldFactory
    {
        /*
        // Returns a Dictionary that is queried by location in the grid, and returns a Vector3
        public static Dictionary<int2,Vector3> GenerateFlowField(CustomGrid cg, Dictionary<int2,int> blocked, Vector3 bounds1, Vector3 bounds2, Vector3 destination, int cellsPerFrame)
        {
            // get top right and bottom-left corners
            Tuple<Vector3,Vector3> newBounds = ObstacleGrid.getBounds(bounds1,bounds2);
            Vector3 bottomLeft = newBounds.Item1;
            Vector3 topRight = newBounds.Item2;

            // bounds in grid coordinates
            int2 b1 = cg.worldToCell(bottomLeft);
            int2 b2 = cg.worldToCell(topRight);

            int numCols = b2.Item1 - b1.Item1; // x length
            int numRows = b2.Item2 - b1.Item2; // z length

            int rOff = b1.Item1;
            int cOff = b1.Item2;
        
            // 2. Flood fill grid to get cost values for each square
            int [,] dGrid = DijkstraGrid.GenerateGrid(numCols,numRows,b1,blocked,cg.worldToCell(destination));

            // 3. Generate FlowField array from grid
            Vector3 [,] flowField = FlowField.Generate(numRows,numCols,dGrid,b1,cg);

            // 4. Transfer Array to Dictionary for ease of Querying
            Dictionary<int2,Vector3> vDict = new Dictionary<int2, Vector3>();

            for (int i = 0; i < flowField.GetLength(0); i++){
                for (int j = 0; j < flowField.GetLength(1); j++){
                    int2 index = new int2(i + cOff, j + rOff);
                    vDict[index] = flowField[i,j];
                }
            }

            return vDict;

        }
        */
    }
}