using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;

namespace KaizerWaldCode.Grid
{
    public class GridSettings : MonoBehaviour
    {
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
        public float PointSpacing { get; private set; }

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
            
            editorMapSize = MapSize;
            editorPointSpacing = PointSpacing;
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
                PointSpacing = 1f / (pointPerMeter - 1f);
            }
            else
            {
                MapSize = chunkSize * numChunk;
                PointSpacing = 1f / (pointPerMeter - 1f);
            }
            
            editorMapSize = MapSize;
            editorPointSpacing = PointSpacing;
            
            FlowField = new FlowField();
            FlowField.InitGrid(float3(10f,0,10f), this);
        }

        private void OnDrawGizmos()
        {
            if (displayGrid)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                float cellRadius = PointSpacing / 2f;
                
                if (FlowField == null)
                {
                    DrawGrid(int2(MapSize), Color.yellow, cellRadius, false, style);
                }
                else
                {
                    DrawGrid(int2(MapSize), Color.green, cellRadius, true, style);
                }
            }
        }

        private void DrawGrid(int2 drawGridSize, Color drawColor, float drawCellRadius, bool flowfield, GUIStyle style)
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
                    if (flowfield)
                    {
                        
                        Handles.Label(center, FlowField.CellsCost[mad(y,drawGridSize.y,x)].ToString(), style);
                    }
                }
            }
        }
    }
}
