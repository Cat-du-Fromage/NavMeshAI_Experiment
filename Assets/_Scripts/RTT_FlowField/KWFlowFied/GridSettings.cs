using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;

namespace KaizerWaldCode.Grid
{
    public enum FlowFieldDisplayType
    {
        None,
        AllIcons,
        DestinationIcon,
        CostField,
        IntegrationField
    };
    
    public class GridSettings : MonoBehaviour
    {
        [SerializeField] private FlowFieldDisplayType curDisplayType;
        
        [SerializeField] private bool displayGrid;
        
        [SerializeField] private MeshFilter terrain;
        
        [SerializeField] private bool useTerrainSize;
        [Min(1)]
        [SerializeField] private int chunkSize;
        [Min(1)]
        [SerializeField] private int numChunk;
        [Range(2, 10)]
        [SerializeField] private int pointPerMeter;
        
        public bool UseTerrainSize { get; private set; }
        public int ChunkSize { get; private set; }
        public int NumChunk { get; private set; }
        public int PointPerMeter { get; private set; }

        public int MapSize { get; private set; }
        
        public int ChunkPointPerAxis { get; private set; }
        public int MapPointPerAxis { get; private set; }
        public float PointSpacing { get; private set; }

        public int[] test;
        
        private FlowField FlowField;
        
#if UNITY_EDITOR  
        //DEBUG PURPOSE
        public int editorMapSize;
        public float editorPointSpacing;

        private void OnValidate()
        {
            if(terrain == null) terrain = GameObject.FindWithTag("TerrainTag").GetComponent<MeshFilter>();
            
            UseTerrainSize = useTerrainSize;
            
            ChunkSize = max(1, chunkSize);
            NumChunk = max(1, numChunk);
            PointPerMeter = clamp(PointPerMeter,2, 10);
            
            if (UseTerrainSize)
            {
                MapSize = (int)(terrain.sharedMesh.bounds.size.x * terrain.transform.localScale.x);
                PointSpacing = 1f / (pointPerMeter - 1f);
            }
            else
            {
                MapSize = chunkSize * numChunk;
                PointSpacing = 1f / (pointPerMeter - 1f);
            }

            //transform.position = new Vector3(0 - MapSize / 2, 0, 0 - MapSize / 2);
            editorMapSize = MapSize;
            editorPointSpacing = PointSpacing;
            //ChunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1);
            //MapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1);
        }
#endif

        private void Awake()
        {
            if(terrain == null) terrain = GameObject.FindWithTag("TerrainTag").GetComponent<MeshFilter>();
            
            UseTerrainSize = useTerrainSize;

            ChunkSize = max(1, chunkSize);
            NumChunk = max(1, numChunk);
            PointPerMeter = clamp(PointPerMeter,2, 10);
            
            if (UseTerrainSize)
            {
                MapSize = (int)(terrain.sharedMesh.bounds.size.x * terrain.transform.localScale.x);
                PointSpacing = MapSize / (pointPerMeter - 1f);
            }
            else
            {
                MapSize = chunkSize * numChunk;
                PointSpacing = 1f / (pointPerMeter - 1f);
            }
            
            editorMapSize = MapSize;
            editorPointSpacing = PointSpacing;
            //ChunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1);
            //MapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1);
            FlowField = new FlowField();
            FlowField.InitGrid(float3(20f,0,12f), this);
            test = FlowField.CellsCost;
        }

        private void OnDrawGizmos()
        {
            if (displayGrid)
            {
                if (FlowField == null)
                {
                    DrawGrid(int2(MapSize), Color.yellow, PointSpacing/2f);
                }
                else
                {
                    DrawGrid(int2(MapSize), Color.green, PointSpacing/2f);
                }
            }
            /*
            if (FlowField == null) { return; }
 
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
 
            switch (curDisplayType)
            {
                case FlowFieldDisplayType.CostField:
 
                    foreach (float3 curCell in FlowField.CellsPosition)
                    {
                        //Handles.Label(curCell.worldPos, curCell.cost.ToString(), style);
                    }
                    break;
                
                case FlowFieldDisplayType.IntegrationField:
 
                    foreach (Cell curCell in curFlowField.grid)
                    {
                        //Handles.Label(curCell.worldPos, curCell.bestCost.ToString(), style);
                    }
                    break;
                
                default:
                    break;
            }
            */
        }

        private void DrawGrid(int2 drawGridSize, Color drawColor, float drawCellRadius)
        {
            Gizmos.color = drawColor;
            for (int x = 0; x < drawGridSize.x; x++)
            {
                for (int y = 0; y < drawGridSize.y; y++)
                {
                    float offset = 0 - MapSize / 2;
                    Vector3 center = new Vector3(
                        (drawCellRadius * 2 * x + drawCellRadius) + offset,
                        0,
                        (drawCellRadius * 2 * y + drawCellRadius) + offset);
                    Vector3 size = Vector3.one * drawCellRadius * 2;
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
    }
}
