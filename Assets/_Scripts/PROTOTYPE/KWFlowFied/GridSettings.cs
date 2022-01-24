using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;

namespace KaizerWaldCode.Grid
{
    public enum FlowFieldDisplayType { None, AllIcons, DestinationIcon, CostField, IntegrationField };
    public class GridSettings : MonoBehaviour
    {
        public FlowFieldDisplayType curDisplayType;
        
        [SerializeField] private Transform Player;
        
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

        //public int[] realCost;
        
#if UNITY_EDITOR
        //DEBUG PURPOSE
        public int editorMapSize;
        public float editorPointSpacing;

        private string MaxDebug = "Max";
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
            
            FlowField = new FlowField(this);
            FlowField.InitGrid(Player.position, this);
            //realCost = new int[FlowField.CellsCost.Length];
            //realCost = FlowField.CellsCost;
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

        private void DrawGrid(int2 drawGridSize, Color drawColor, float drawCellRadius, bool flowField, GUIStyle style)
        {
            Gizmos.color = drawColor;
            for (int y = 0; y < drawGridSize.y; y++)
            {
                for (int x = 0; x < drawGridSize.x; x++)
                {
                    int index = (y * drawGridSize.y) + x;
                    Vector3 center = FlowField.CellsCenterPosition[index]-(Vector3.up * 0.5f);
                    Vector3 size = Vector3.one * drawCellRadius * 2;
                    //Gizmos.DrawWireCube(center, size);
                    //Gizmos.color = Color.red;
                    //Gizmos.DrawWireSphere(FlowField.CellsCenterPosition[index]-(Vector3.up/2f), 0.1f);
                    if (flowField)
                    {
                        switch (curDisplayType)
                        {
                            case FlowFieldDisplayType.CostField:
                                Handles.Label(center, FlowField.CellsCost[index].ToString(), style);
                                break;

                            case FlowFieldDisplayType.IntegrationField:
                                string text = FlowField.CellsBestCost[index] >= ushort.MaxValue
                                    ? MaxDebug
                                    : FlowField.CellsBestCost[index].ToString();
                                if (FlowField.CellsBestCost[index] >= ushort.MaxValue) continue;
                                Handles.Label(center, text, style);
                                break;
                            
                            case FlowFieldDisplayType.AllIcons :
                                int2 coord = FlowField.BestDirection[index];
                                Vector3 dir = new Vector3(coord.x,0,coord.y);
                                DrawArrow.ForGizmo(center, dir);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
