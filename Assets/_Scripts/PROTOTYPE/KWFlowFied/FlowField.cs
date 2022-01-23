using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWaldCode.Globals.StaticDatas;
using static Unity.Mathematics.math;
using static KWUtils.KWGrid;
using static KWUtils.KWmath;
using static KWUtils.NativeCollectionUtils;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.Grid
{
    public class FlowField
    {
        //Center Cell
        public Vector3[] CellsCenterPosition;
        
        //Cost Value Cell
        public int[] CellsBestCost;
        public int[] CellsCost;
        
        public int2 PositioninGrid;


        public void InitGrid(float3 targetPosition, GridSettings gc)
        {
            //Init Arrays
            CellsCenterPosition = new Vector3[sq(gc.MapSize)];
            CellsCost = new int[sq(gc.MapSize)];
            CellsBestCost = new int[sq(gc.MapSize)];
            
            //INIT POSITIONS
            GridCellPositions(gc);

            //float2 test = float2(0 - (gc.MapSize / 2f));
            float3 offset = new float3((gc.MapSize / 2f),0, (gc.MapSize / 2f));
            int index1 = targetPosition.Get2DCellID(gc.MapSize, gc.PointSpacing, offset);
            //int2 index = targetPosition.GetGridCoordFromPosition(gc.MapSize, gc.PointSpacing);
            
            PositioninGrid = index1.GetXY2(gc.MapSize);
            
            GetObstaclesGrid(gc);

            NativeArray<int> cellsBestCost = AllocFillNtvAry<int>(CellsBestCost.Length, ushort.MaxValue);
            NativeArray<int> cellsCost = CellsCost.ToNativeArray();
            
            Queue<int> cellsToCheck = new Queue<int>();

            /*
            JCellCost job = new JCellCost
            {
                DestinationCellIndex = index1,
                PointPerAxis = gc.MapSize,
                CellsCost = cellCost,
                CellsBestCost = cellsBestCost,
                cellsToCheck = cellsToCheck,
            };
            JobHandle jh = job.Schedule();
            jh.Complete();
            cellsBestCost.CopyTo(CellsBestCost);
            cellCost.CopyTo(CellsCost);
            
            */
            
            cellsCost[index1] = 0;
            cellsBestCost[index1] = 0;
            
            //NativeQueue<int> cellsToCheck = new NativeQueue<int>(Allocator.Temp);
            cellsToCheck.Enqueue(index1);

            for (int k = 0; k < 100; k++)
            {
                if (k != 0 && cellsToCheck.Count == 0)
                {
                    Debug.Log($"Queue left {cellsToCheck.Count}");
                    return;
                }
                //Debug.Log($"Queue left {cellsToCheck.Count}");
                
            //while(cellsToCheck.Count > 0)
            //{
                int currentCellIndex = cellsToCheck.Dequeue();
                //Debug.Log(currentCellIndex);
                //Debug.Log($"DestCell = {cellsCost[currentCellIndex]} AND {cellsBestCost[currentCellIndex]}");
                /*
                if (cellsCost[currentCellIndex] == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int adjacentIndex = index1.AdjCellFromIndex((1 << i), currentCellIndex.GetXY2(gc.MapSize), gc.MapSize);
                        Debug.Log($"Adj = {Enum.GetName(typeof(AdjacentCell), (1 << i))} = {adjacentIndex}");
                    }
                }
                */
                List<int> curNeighbors = GetNeighborCells(currentCellIndex, gc, GridDirection.CardinalDirections);
                //using NativeList<int> curNeighbors = GetNeighborCells(currentCellIndex, gc);

                foreach (int neighborIndex in curNeighbors)
                {
                    Debug.Log($"COST? {cellsCost[neighborIndex]}");
                    if (cellsCost[neighborIndex] >= byte.MaxValue)
                    {
                        Debug.Log($"is happens");
                        continue;
                    }
                    if (cellsCost[neighborIndex] + cellsCost[currentCellIndex] < cellsBestCost[neighborIndex])
                    {
                        cellsBestCost[neighborIndex] = cellsCost[neighborIndex] + cellsBestCost[currentCellIndex];
                        cellsToCheck.Enqueue(neighborIndex);
                    }
                }
                //curNeighbors.Clear();
            }

            cellsBestCost.CopyTo(CellsBestCost);

            cellsBestCost.Dispose();
            cellsCost.Dispose();

        }
        
        private int GetCellAtRelativePos(int2 orignPos, int2 relativePos, in GridSettings settings)
        {
            int2 finalPos = orignPos + relativePos;
 
            if (finalPos.x < 0 || finalPos.x >= settings.MapSize || finalPos.y < 0 || finalPos.y >= settings.MapSize)
            {
                return -1;
            }
            return mad(finalPos.y, settings.MapSize,finalPos.x) ;
        }
        
        private List<int> GetNeighborCells(int index, GridSettings settings, List<GridDirection> directions)
        {
            List<int> curNeighbors = new List<int>();
            int2 coord = index.GetXY2(settings.MapSize);
            

            foreach (GridDirection gd in directions)
            {
                int test = GetCellAtRelativePos(coord, gd, settings);
                if (test != -1)
                {
                    curNeighbors.Add(test);
                }
            }
            
            /*
            for (int i = 0; i < 4; i++)
            {
                int adjacentIndex = index.AdjCellFromIndex((1 << i), coord, settings.MapSize);
                //Debug.Log($"{index}adj index {adjacentIndex}");
                if (adjacentIndex == -1) continue;
                //Debug.Log($"{index}adj index {adjacentIndex}");
                curNeighbors.Add(adjacentIndex);
            }
*/
            return curNeighbors;
        }

        //INIT CELLS POSITION IN GRID
        public void GridCellPositions(GridSettings settings)
        {
            using NativeArray<float3> cellPos = AllocNtvAry<float3>(CellsCenterPosition.Length);

            JCellsPosition job = new JCellsPosition
            {
                HalfMapOffset = settings.MapSize/2,
                PointPerAxis = settings.MapSize,
                Spacing = settings.PointSpacing,
                Vertices = cellPos
            };
            JobHandle jh = job.ScheduleParallel(CellsCenterPosition.Length, JobsUtility.JobWorkerCount - 1, default);
            jh.Complete();
            cellPos.Reinterpret<Vector3>().CopyTo(CellsCenterPosition);
        }

        //Retrieve all obstacle and attribute them +255
        public void GetObstaclesGrid(GridSettings settings)
        {
            Vector3 cellHalfExtents = Vector3.one * (settings.PointSpacing/2f);
            //float radius = settings.PointSpacing / 2f;
            for (int i = 0; i < CellsCenterPosition.Length; i++)
            {
                //if (!Physics.CheckSphere(CellsCenterPosition[i], radius, ObstacleLayer)) continue;
                if (!Physics.CheckBox(CellsCenterPosition[i], cellHalfExtents, Quaternion.identity, ObstacleLayer))
                {
                    CellsCost[i] = 1;
                }
                else
                {
                    CellsCost[i] = byte.MaxValue; 
                }
            }
        }
    }

    public struct JCellCost : IJob
    {
        [ReadOnly] public int DestinationCellIndex;
        //[ReadOnly] public int2 DestinationCellCoord;
        [ReadOnly] public int PointPerAxis;
        
        public NativeArray<int> CellsCost;
        public NativeArray<int> CellsBestCost;
        
        public NativeQueue<int> cellsToCheck;
        public void Execute()
        {
            CellsCost[DestinationCellIndex] = 0;
            CellsBestCost[DestinationCellIndex] = 0;
            
            //NativeQueue<int> cellsToCheck = new NativeQueue<int>(Allocator.Temp);
            cellsToCheck.Enqueue(DestinationCellIndex);
            
            while(cellsToCheck.Count > 0)
            {
                int currentCellIndex = cellsToCheck.Dequeue();
                NativeList<int> curNeighbors = GetNeighborCells(currentCellIndex);

                for (int i = 0; i < curNeighbors.Length; i++)
                {
                    int neighborIndex = curNeighbors[i];
                    if (CellsCost[neighborIndex] >= 255) { continue; }
                    Debug.Log($"Pass NOT max Value {neighborIndex}");
                    Debug.Log($"Pass NOT max Value BEST COST? {CellsBestCost[neighborIndex]} {neighborIndex}");
                    if (CellsCost[neighborIndex] + CellsCost[currentCellIndex] < CellsBestCost[neighborIndex])
                    {
                        Debug.Log($"Pass best cost OK BEST COST? {CellsBestCost[neighborIndex]}");
                        Debug.Log($"Pass best cost OK {CellsCost[neighborIndex] + CellsCost[currentCellIndex]}");
                        CellsBestCost[neighborIndex] = CellsCost[neighborIndex] + CellsBestCost[currentCellIndex];
                        cellsToCheck.Enqueue(neighborIndex);
                    }
                }
                curNeighbors.Dispose();
            }
        }
        
        private NativeList<int> GetNeighborCells(int index)
        {
            NativeList<int> curNeighbors = new NativeList<int>(Allocator.Temp);

            int2 coord = index.GetXY2(PointPerAxis);
            for (int i = 0; i < 4; i++)
            {
                int adjacentIndex = DestinationCellIndex.AdjCellFromIndex((1 << i), coord, PointPerAxis);
                
                if (adjacentIndex == -1) continue;
                Debug.Log($"adj index {adjacentIndex}");
                curNeighbors.Add(adjacentIndex);
            }

            return curNeighbors;
        }
    }


    /// <summary>
    /// Process Cell Index
    /// </summary>
    public struct JCellsCost : IJobFor
    {
        [ReadOnly] public int2 TargetGridPos;
        [ReadOnly] public int NumCellMap;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> CellCostGrid;
        
        public void Execute(int index)
        {
            (int x, int z) = index.GetXY(NumCellMap);

            int varX = abs(x - TargetGridPos.x);
            int varY = abs(z - TargetGridPos.y);

            CellCostGrid[index] = varX + varY;
        }
    }
    
    /// <summary>
    /// Get Cells Center
    /// </summary>
    public struct JCellsPosition : IJobFor
    {
        //MapSettings
        [ReadOnly] public int HalfMapOffset; //DONT FORGET THIS ONE!
        [ReadOnly] public int PointPerAxis;
        [ReadOnly] public float Spacing;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> Vertices;
        
        public void Execute(int index)
        {
            (int x, int z) = index.GetXY(PointPerAxis);
            float3 pointPosition = float3(x-HalfMapOffset, 0, z-HalfMapOffset) * float3(Spacing) + (float3(Spacing/ 2f,0,Spacing/ 2f));
            Vertices[index] = pointPosition;
        }
    }
}





/*


    /// <summary>
    /// Process Cell Index
    /// </summary>
    public struct JCellsIndex : IJobFor
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
*/