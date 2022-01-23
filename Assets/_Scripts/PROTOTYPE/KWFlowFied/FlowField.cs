using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Physics;
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


        public void InitGrid(in float3 targetPosition, in GridSettings gc)
        {
            //Init Arrays
            CellsCenterPosition = new Vector3[sq(gc.MapSize)];
            CellsCost = new int[sq(gc.MapSize)];
            CellsBestCost = new int[sq(gc.MapSize)];
            Array.Fill(CellsBestCost, ushort.MaxValue);
            //INIT POSITIONS
            GridCellPositions(gc);
            //Obstacle Map
            GetObstaclesGrid(gc);
            //Best Cost
            BestCostJob(targetPosition, gc);
        }

        public void BestCostJob(in float3 targetPosition, in GridSettings gc)
        {
            int destinationIndex = targetPosition.GetIndexFromPosition(gc.MapSize, gc.PointSpacing);

            using NativeArray<int> tempCost = CellsCost.ToNativeArray();
            using NativeArray<int> tempBestCost = CellsBestCost.ToNativeArray();

            JCellBestCost job = new JCellBestCost
            {
                DestinationCellIndex = destinationIndex,
                PointPerAxis = gc.MapSize,
                CellsCost = tempCost,
                CellsBestCost = tempBestCost
            };
            JobHandle jh = job.Schedule();
            jh.Complete();
            tempBestCost.CopyTo(CellsBestCost);
        }

        //INIT CELLS POSITION IN GRID
        public void GridCellPositions(in GridSettings settings)
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
            for (int i = 0; i < CellsCenterPosition.Length; i++)
            {
                CellsCost[i] = !CheckBox(CellsCenterPosition[i], cellHalfExtents, Quaternion.identity, ObstacleLayer) ? 
                        1 : byte.MaxValue;
            }
        }
    }

    public struct JCellBestCost : IJob
    {
        [ReadOnly] public int DestinationCellIndex;
        [ReadOnly] public int PointPerAxis;
        
        public NativeArray<int> CellsCost;
        public NativeArray<int> CellsBestCost;
        
        //public NativeQueue<int> cellsToCheck;
        public void Execute()
        {
            NativeQueue<int> cellsToCheck = new NativeQueue<int>(Allocator.Temp);
            NativeList<int> currentNeighbors = new NativeList<int>(Allocator.Temp);
            
            CellsCost[DestinationCellIndex] = 0;
            CellsBestCost[DestinationCellIndex] = 0;
            
            cellsToCheck.Enqueue(DestinationCellIndex);
            
            while(cellsToCheck.Count > 0)
            {
                int currentCellIndex = cellsToCheck.Dequeue();
                currentNeighbors = GetNeighborCells(currentCellIndex, currentNeighbors);

                foreach (int neighborIndex in currentNeighbors)
                {
                    if (CellsCost[neighborIndex] >= byte.MaxValue) continue;
                    if (CellsCost[neighborIndex] + CellsBestCost[currentCellIndex] < CellsBestCost[neighborIndex])
                    {
                        CellsBestCost[neighborIndex] = CellsCost[neighborIndex] + CellsBestCost[currentCellIndex];
                        cellsToCheck.Enqueue(neighborIndex);
                    }
                }
                currentNeighbors.Clear();
            }

            currentNeighbors.Dispose();
            cellsToCheck.Dispose();
        }
        
        private NativeList<int> GetNeighborCells(int index, NativeList<int> curNeighbors)
        {
            int2 coord = index.GetXY2(PointPerAxis);
            for (int i = 0; i < 4; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, PointPerAxis);
                if (neighborId == -1) continue;
                curNeighbors.Add(neighborId);
            }
            return curNeighbors;
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