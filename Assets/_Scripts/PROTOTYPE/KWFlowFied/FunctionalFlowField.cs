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
    public class FunctionalFlowField : MonoBehaviour
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

            TEST(targetPosition, gc);
        }
        
        public void TEST(in float3 targetPosition, in GridSettings gc)
        {
            float3 offset = new float3((gc.MapSize / 2f),0, (gc.MapSize / 2f));
            int index1 = targetPosition.Get2DCellID(gc.MapSize, gc.PointSpacing, offset);
            PositioninGrid = index1.GetXY2(gc.MapSize);

            Queue<int> cellsToCheck = new Queue<int>(1);

            CellsCost[index1] = 0;
            CellsBestCost[index1] = 0;
            
            cellsToCheck.Enqueue(index1);
            while(cellsToCheck.Count > 0)
            {
                int currentCellIndex = cellsToCheck.Dequeue();
                List<int> curNeighbors = GetNeighborCells(currentCellIndex, gc, GridDirection.CardinalDirections);
                foreach (int neighborIndex in curNeighbors)
                {
                    if (CellsCost[neighborIndex] >= byte.MaxValue) continue;
                    if (CellsCost[neighborIndex] + CellsBestCost[currentCellIndex] < CellsBestCost[neighborIndex])
                    {
                        CellsBestCost[neighborIndex] = CellsCost[neighborIndex] + CellsBestCost[currentCellIndex];
                        cellsToCheck.Enqueue(neighborIndex);
                    }
                }
            }
        }
        
        private int GetCellAtRelativePos(int2 orignPos, int2 relativePos, in GridSettings settings)
        {
            int2 finalPos = orignPos + relativePos;
 
            if (finalPos.x < 0 || finalPos.x >= settings.MapSize || finalPos.y < 0 || finalPos.y >= settings.MapSize)
            {
                return -1;
            }
            return mad(finalPos.y, settings.MapSize,finalPos.x);
        }
        
        private List<int> GetNeighborCells(int index, in GridSettings settings, in List<GridDirection> directions)
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
            return curNeighbors;
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
}
