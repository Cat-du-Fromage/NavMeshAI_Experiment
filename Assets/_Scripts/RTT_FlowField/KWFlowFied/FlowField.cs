using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWGrid;
using static KWUtils.KWmath;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.Grid
{
    public class FlowField
    {
        public int[] CellsCost;
        public int2 PositioninGrid;


        public void InitGrid(float3 targetPosition, GridSettings gc)
        {
            CellsCost = new int[sq(gc.MapSize)];
            float2 test = float2(0 - (gc.MapSize / 2f));
            float3 offset = new float3((gc.MapSize / 2f),0, (gc.MapSize / 2f));
            int index1 = targetPosition.Get2DCellID(gc.MapSize, gc.PointSpacing, offset);
            int2 index = targetPosition.GetGridCoordFromPosition(gc.MapSize, gc.PointSpacing);
            Debug.Log($"PERCENT METHOD {index} for {targetPosition}");
            PositioninGrid = index1.GetXY2(gc.MapSize);
            Debug.Log($"Target at index {index1} coord {PositioninGrid}");
            NativeArray<int> tempGrid = new NativeArray<int>(sq(gc.MapSize), Allocator.TempJob);

            JCellsCost job = new JCellsCost
            {
                TargetGridPos = PositioninGrid,
                NumCellMap = gc.MapSize,
                CellCostGrid = tempGrid
            };
            
            JobHandle jh = job.ScheduleParallel(sq(gc.MapSize), JobsUtility.JobWorkerCount - 1, default);
            jh.Complete();
            tempGrid.CopyTo(CellsCost);
            tempGrid.Dispose();
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
}





/*
 /// <summary>
    /// Process cells Positions
    /// </summary>
    public struct JCellsPosition : IJobFor
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