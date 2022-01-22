using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWGrid;

namespace KaizerWaldCode
{
    public static class GridComponent
    {
        public static Vector3[,] Generate(int numRows,int numCols,int[,] dGrid,Tuple<int,int> b1,CustomGrid cg)
        {

            float cellSize = cg.cellSize;

            int ROW = numRows;
            int COL = numCols;

            int rOff = b1.Item1;
            int cOff = b1.Item2;

            int [,] dijkstra = dGrid;
            Vector3[,] flowfield = new Vector3[ROW, COL];

            for (int j = 0; j < COL; j++)
            {
                for(int i = 0; i < ROW; i++)
                {
                    int min = Int32.MaxValue;
                    int j_dest = -1; //the indices of the cell with the smallest cost value
                    int i_dest = -1; //set these to -1 so we know if they are not set

                    //(i,j+1) EAST
                    if (j + 1 < COL)
                    {
                        if ((dijkstra[i,j+1] < min) && (dijkstra[i, j + 1] != -1))
                        {
                            min = dijkstra[i, j + 1];
                            i_dest = i;
                            j_dest = j + 1;
                        }
                    }

                    //(i-1,j+1) SOUTH-EAST
                    if ((j + 1 < COL) && (i > 0))
                    {
                        if ((dijkstra[i - 1, j + 1] < min) && (dijkstra[i - 1, j + 1] != -1))
                        {
                            min = dijkstra[i - 1, j + 1];
                            i_dest = i - 1;
                            j_dest = j + 1;
                        }
                    }

                    //(i,j+1) SOUTH
                    if (i > 0)
                    {
                        if ((dijkstra[i - 1, j] < min) && (dijkstra[i - 1, j] != -1))
                        {
                            min = dijkstra[i - 1, j];
                            i_dest = i - 1;
                            j_dest = j;
                        }
                    }

                    //(i-1,j-1) SOUTH-WEST
                    if ((j > 0) && (i > 0))
                    {
                        if ((dijkstra[i - 1, j - 1] < min) && (dijkstra[i - 1, j - 1] != -1))
                        {
                            min = dijkstra[i - 1, j - 1];
                            i_dest = i - 1;
                            j_dest = j - 1;
                        }
                    }

                    //(i,j+1) WEST
                    if (j > 0 )
                    {
                        if ((dijkstra[i, j - 1] < min) && (dijkstra[i, j - 1] != -1))
                        {
                            min = dijkstra[i, j - 1];
                            i_dest = i;
                            j_dest = j - 1;
                        }
                    }

                    //(i+1,j-1) NORTH-WEST
                    if ((j > 0) && (i + 1 < ROW))
                    {
                        if ((dijkstra[i + 1, j - 1] < min) && (dijkstra[i + 1, j - 1] != -1))
                        {
                            min = dijkstra[i + 1, j - 1];
                            i_dest = i + 1;
                            j_dest = j - 1;
                        }
                    }

                    //(i,j+1) NORTH
                    if (i + 1 < ROW)
                    {
                        if ((dijkstra[i + 1, j] < min) && (dijkstra[i + 1, j] != -1))
                        {
                            min = dijkstra[i + 1, j];
                            i_dest = i + 1;
                            j_dest = j;
                        }
                    }

                    //(i+1,j-1) NORTH-EAST
                    if ((j + 1 < COL) && (i + 1 < ROW))
                    {
                        if ((dijkstra[i + 1, j + 1] < min) && (dijkstra[i + 1, j + 1] != -1))
                        {
                            min = dijkstra[i + 1, j + 1];
                            i_dest = i + 1;
                            j_dest = j + 1;
                        }
                    }

                    Vector3 field = new Vector3();

                    field.y = 0.0f;
                    field.x = (float)(i_dest - i);
                    field.z = (float)(j_dest - j);

                    if((i_dest == -1) || (j_dest == -1))
                    {
                        field = new Vector3(0, 0, 0);
                    }

                    Color grad = new Vector4(0.01f * dijkstra[i, j], 0.0f,0.0f, 1);

                    flowfield[i, j] = field/(field.magnitude); //normalize vector

                }//end for j
            }//end for i

            return flowfield;

        }//end function

    }
    
        /// <summary>
        /// Process Vertices Positions
        /// </summary>
        public struct JVerticesPosition : IJobFor
        {
            //MapSettings
            [ReadOnly] public int PointPerAxis;
            [ReadOnly] public float Spacing;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<float3> Vertices;
            
            public void Execute(int index)
            {
                (int x, int z) = index.GetXY(PointPerAxis);
                float3 pointPosition = float3(x, 0, z) * float3(Spacing);
                Vertices[index] = pointPosition;
            }
        }

        /// <summary>
        /// Process Vertices Cell Index
        /// </summary>
        public struct JVerticesCellIndex : IJobFor
        {
            [ReadOnly] public int NumCellMap;
            [ReadOnly] public int CellSize;
            [ReadOnly] public NativeArray<float3> Vertices;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<int> VerticesCellGrid;
            
            public void Execute(int index)
            {
                float2 cellGrid = float2(NumCellMap);
                float2 currVertPos = Vertices[index].xz;

                FindCell(ref cellGrid, in currVertPos);
                VerticesCellGrid[index] = (int)mad(cellGrid.y, NumCellMap, cellGrid.x);
            }

            private void FindCell(ref float2 cellGrid, in float2 vertPos)
            {
                for (int i = 0; i < NumCellMap; i++)
                {
                    if ((int)cellGrid.y == NumCellMap) cellGrid.y = select(NumCellMap, i, vertPos.y <= mad(i, CellSize, CellSize));
                    if ((int)cellGrid.x == NumCellMap) cellGrid.x = select(NumCellMap, i, vertPos.x <= mad(i, CellSize, CellSize));
                    if ((int)cellGrid.x != NumCellMap && (int)cellGrid.y != NumCellMap) break;
                }
            }
        }
}
