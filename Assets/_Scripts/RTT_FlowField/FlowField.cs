using System;
using System.Collections.Generic;
using KaizerWaldCode.Grid;
using KWUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWaldCode.Globals.StaticDatas;
using static UnityEngine.Physics;
using static KWUtils.NativeCollectionUtils;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.FlowField
{
    /// <summary>
    /// FLOWFIELD seems a bit overkilled for the intended useCase
    /// May switch to A* later
    /// </summary>
    public static class FlowField
    {
        private static GridData GridData;

        private static int TargetCellIndex;

        //HINT
        //We may want to store obstacles so we dont recalculate physics every time!
        public static Vector3[] DebugPosition;
        public static int[] DebugBestCost;
        public static int[] DebugCellCost;
        
        public static Vector3[] GetFlowField(in float3 targetPosition, in GridData gridData)
        {
            GridData = gridData;
            //Init Arrays
            int numCells = gridData.TotalCells;
            TargetCellIndex = targetPosition.GetIndexFromPosition(gridData.MapWidth, gridData.CellSize);

            using NativeArray<float3> cellsCenterPosition = AllocNtvAry<float3>(numCells);
            using NativeArray<int> cellsCost = AllocFillNtvAry<int>(numCells, 1);
            using NativeArray<int> cellsBestCost = AllocFillNtvAry<int>(numCells, ushort.MaxValue);
            using NativeArray<float3> bestDirection = AllocNtvAry<float3>(numCells);
            //Initialize Cells Positions
            GridCellPositions(cellsCenterPosition).Complete();
            //Get Obstacles on the grid
            GetObstaclesGrid(cellsCenterPosition, cellsCost);
            //Best Cost
            JobHandle jHBestCosts = GetBestCosts(cellsCost, cellsBestCost);
            //DirectionField
            JobHandle jHFlowField = GenerateFlowField(cellsBestCost, bestDirection, jHBestCosts);
            jHFlowField.Complete();
            /*
            DebugPosition = new Vector3[numCells];
            DebugPosition = cellsCenterPosition.Reinterpret<Vector3>().ToArray();
            DebugBestCost = cellsBestCost.ToArray();
            DebugCellCost = cellsCost.ToArray();
            */
            return bestDirection.Reinterpret<Vector3>().ToArray();
        }
        
        private static JobHandle GridCellPositions(NativeArray<float3> cellPos)
        {
            JCellsPosition job = new JCellsPosition(GridData, cellPos);
            return job.ScheduleParallel(cellPos.Length, JobsUtility.JobWorkerCount - 1, default);
        }
        
        private static void GetObstaclesGrid(NativeArray<float3> cellPos, NativeArray<int> cellsCost)
        {
            /*
            JObstacles job = new JObstacles
            {
                CellHalfExtents = cellHalfExtents,
                CellPos = cellPos,
                CellsCost = cellsCost
            };
            JobHandle jh = job.Schedule(GridData.TotalCells, 1, default);
            jh.Complete();
            */
            
            //Wait.. we can modify a "using statement"(cellsCost) if in a separate function?!
            float3 cellHalfExtents = new float3(GridData.CellSize/2f);
            for (int i = 0; i < cellsCost.Length; i++)
            {
                if (CheckBox(cellPos[i], cellHalfExtents, Quaternion.identity, ObstacleLayer))
                {
                    cellsCost[i] = byte.MaxValue;
                }
            }
            
        }
        
        private static JobHandle GetBestCosts(NativeArray<int> cellsCost, NativeArray<int> cellsBestCost, JobHandle dependency = default)
        {
            JCellBestCost job = new JCellBestCost(TargetCellIndex, GridData.MapWidth, cellsCost, cellsBestCost);
            return job.Schedule(dependency);
        }
        
        private static JobHandle GenerateFlowField(NativeArray<int> cellsBestCost, NativeArray<float3> bestDirection, JobHandle dependency = default)
        {
            JCellBestDirection job = new JCellBestDirection(GridData.NumCellsX, cellsBestCost, bestDirection);
            return job.ScheduleParallel(GridData.TotalCells, JobsUtility.JobWorkerCount - 1, dependency);
        }
    }
    
    /// <summary>
    /// Get Cells Center
    /// </summary>
    //[BurstCompile(CompileSynchronously = true)]
    public struct JCellsPosition : IJobFor
    {
        [ReadOnly] private readonly int HalfMapOffset; //DONT FORGET THIS ONE!
        [ReadOnly] private readonly int NumCellX;
        [ReadOnly] private readonly float CellSize;

        [WriteOnly, NativeDisableParallelForRestriction]
        private NativeArray<float3> CellsCenterPosition;

        public JCellsPosition(in GridData gridData, NativeArray<float3> cellsCenterPosition)
        {
            HalfMapOffset = gridData.MapWidth / 2;
            NumCellX = gridData.NumCellsX;
            CellSize = gridData.CellSize;
            CellsCenterPosition = cellsCenterPosition;
        }

        public void Execute(int index)
        {
            (int x, int z) = index.GetXY(NumCellX);
            //Anchor is on the center of the mesh => we need to offset by half his size
            float3 cellPositionOnMesh = new float3(x - HalfMapOffset, 0, z - HalfMapOffset);
            float3 cellBounds = new float3(CellSize,0,CellSize);
            //anchor is on botLeft this time => x+(1/2)cell AND y+(1/2)Cell
            float3 centerCellOffset = new float3(CellSize / 2f, 0, CellSize / 2f);
            
            float3 pointPosition = cellPositionOnMesh * cellBounds + centerCellOffset;
            CellsCenterPosition[index] = pointPosition;
        }
    }
    
    //[BurstCompile(CompileSynchronously = true)]
    public struct JCellBestCost : IJob
    {
        [ReadOnly] private readonly int DestinationCellIndex;
        [ReadOnly] private readonly int NumCellX;
        
        private NativeArray<int> CellsCost;
        private NativeArray<int> CellsBestCost;

        public JCellBestCost(int destinationCellIndex, int numCellX, NativeArray<int> cellsCost, NativeArray<int> cellsBestCost)
        {
            DestinationCellIndex = destinationCellIndex;
            NumCellX = numCellX;
            CellsCost = cellsCost;
            CellsBestCost = cellsBestCost;
        }

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
                GetNeighborCells(currentCellIndex, ref currentNeighbors);

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
        
        private void GetNeighborCells(int index, ref NativeList<int> curNeighbors)
        {
            int2 coord = index.GetXY2(NumCellX);
            for (int i = 0; i < 4; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, NumCellX);
                if (neighborId == -1) continue;
                curNeighbors.Add(neighborId);
            }
        }
    }
    
    //[BurstCompile(CompileSynchronously = true)]
    public struct JCellBestDirection : IJobFor
    {
        [ReadOnly] private readonly int NumCellX;
        
        [ReadOnly, NativeDisableParallelForRestriction]
        private NativeArray<int> CellsBestCost;
        [WriteOnly, NativeDisableParallelForRestriction]
        private NativeArray<float3> CellBestDirection;

        public JCellBestDirection(int numCellX, NativeArray<int> cellsBestCost, NativeArray<float3> cellBestDirection)
        {
            NumCellX = numCellX;
            CellsBestCost = cellsBestCost;
            CellBestDirection = cellBestDirection;
        }

        //public NativeQueue<int> cellsToCheck;
        public void Execute(int index)
        {
            int currentBestCost = CellsBestCost[index];

            if (currentBestCost >= ushort.MaxValue)
            {
                CellBestDirection[index] = float3.zero;
                return;
            }
            
            NativeList<int> neighbors = new NativeList<int>(8,Allocator.Temp);
            
            GetNeighborCells(index, ref neighbors);

            int2 currentCellCoord = index.GetXY2(NumCellX);
            
            foreach(int currentNeighbor in neighbors)
            {
                if(CellsBestCost[currentNeighbor] < currentBestCost)
                {
                    currentBestCost = CellsBestCost[currentNeighbor];
                    int2 neighborCoord = currentNeighbor.GetXY2(NumCellX);
                    int2 bestDirection = neighborCoord - currentCellCoord;
                    CellBestDirection[index] = new float3(bestDirection.x, 0, bestDirection.y);
                }
            }
            neighbors.Dispose();
        }
        
        private void GetNeighborCells(int index, ref NativeList<int> neighbors)
        {
            int2 coord = index.GetXY2(NumCellX);
            for (int i = 0; i < 8; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, NumCellX);
                if (neighborId == -1) continue;
                neighbors.Add(neighborId);
            }
        }
    }
/*
    //DONT WORK, can only check on mainthread
    public struct JObstacles : IJobParallelFor
    {
        [ReadOnly] public float3 CellHalfExtents;
        [ReadOnly] public NativeArray<float3> CellPos;
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<int> CellsCost;

        public void Execute(int index)
        {
            if (Physics.CheckBox(CellPos[index], CellHalfExtents, Quaternion.identity, ObstacleLayer))
            {
                CellsCost[index] = byte.MaxValue;
            }
        }
    }
    */
}