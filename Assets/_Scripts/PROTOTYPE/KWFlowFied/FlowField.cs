using System;
using System.Collections;
using System.Collections.Generic;
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
        public int[] CellsCost;
        
        public int2 PositioninGrid;


        public void InitGrid(float3 targetPosition, GridSettings gc)
        {
            //Init Arrays
            CellsCenterPosition = new Vector3[sq(gc.MapSize)];
            CellsCost = new int[sq(gc.MapSize)];
            
            //INIT POSITIONS
            GridCellPositions(gc);

            //float2 test = float2(0 - (gc.MapSize / 2f));
            float3 offset = new float3((gc.MapSize / 2f),0, (gc.MapSize / 2f));
            int index1 = targetPosition.Get2DCellID(gc.MapSize, gc.PointSpacing, offset);
            //int2 index = targetPosition.GetGridCoordFromPosition(gc.MapSize, gc.PointSpacing);
            
            PositioninGrid = index1.GetXY2(gc.MapSize);
            
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

            GetObstaclesGrid(gc);
        }

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
            Vector3 cellHalfExtents = Vector3.one * settings.PointSpacing/2f;
            float radius = settings.PointSpacing / 2f;
            for (int i = 0; i < CellsCenterPosition.Length; i++)
            {
                //if (!Physics.CheckSphere(CellsCenterPosition[i], radius, ObstacleLayer)) continue;
                if (!Physics.CheckBox(CellsCenterPosition[i], cellHalfExtents, Quaternion.identity,ObstacleLayer)) continue;
                CellsCost[i] += 255; 
            }
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