using System;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using KaizerWaldCode.Utils;
using KWUtils;
//using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionUtils;
using quaternion = Unity.Mathematics.quaternion;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    public class PlacementManager : MonoBehaviour
    {
        [SerializeField] private GameObject token;
        
        //====================================================
        private SelectionData Selection = new SelectionData();
        //====================================================
        
        private Camera PlayerCamera;
        private bool TokensVisible;
        
        //INPUT SYSTEM
        private PlayerEntityInteractionInputsManager Control;
        
        //CONSTANT
        private readonly LayerMask TerrainLayer = 1 << 8;
        
        //CACHED MOUSE POSITION ON SCREEN
        private Vector2 MouseStartPosition;
        private Vector2 MouseEndPosition;

        //CACHED POSITION MOUSE IN GAME
        private Vector3 StartGroundHit;
        private Vector3 EndGroundHit;

        private float LengthMouseDrag;
        
        //JOB SYSTEM
        private TransformAccessArray TransformAccesses;
        private NativeList<JobHandle> JobHandles;
        private JobHandle PlacementJobHandle;
        
        //RAYCAST PROPERTIES
        private RaycastHit Hit;
        private Ray StartRay => PlayerCamera.ScreenPointToRay(MouseStartPosition);
        private Ray EndRay => PlayerCamera.ScreenPointToRay(MouseEndPosition);
        private bool HitGround(Ray ray) => Raycast(ray, out Hit, INFINITY, TerrainLayer);
        public void SetSelectionData(in SelectionData data) => Selection = data;

        private void Awake()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            PlayerCamera = Camera.main;
        }

        private void Start()
        {
            Control = PlayerInteractionsSystem.Instance.GetInputs;
            Control.PlacementEvents.EnableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);
        }

        private void OnDestroy() => Control.PlacementEvents.DisableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);

        private void OnStartMouseClick(InputAction.CallbackContext ctx)
        {
            if (Selection.NumSelection == 0 || MouseStartPosition == ctx.ReadValue<Vector2>()) return;
            
            MouseStartPosition = ctx.ReadValue<Vector2>();
            StartGroundHit = HitGround(StartRay) ? Hit.point : StartGroundHit;
        }

        private void OnPerformMouseMove(InputAction.CallbackContext ctx)
        {
            //First : Update Mouse End Position Value
            MouseEndPosition = ctx.ReadValue<Vector2>();
            int numSelection = Selection.NumSelection;
            if( numSelection == 0) return;
            if (MouseEndPosition == MouseStartPosition) return;
            
            if (HitGround(EndRay))
            {
                EndGroundHit = Hit.point;
                LengthMouseDrag  = length(EndGroundHit - StartGroundHit);
                
                if (LengthMouseDrag >= Selection.MinRowLength - 1) // NEED UNIT (SIZE + Offset) * (MinRow-1)!
                {
                    //SET Marker Visible!
                    if (!TokensVisible)
                    {
                        TokensVisible = true;
                        PlayerInteractionsSystem.Instance.StartPlaceEntity();
                    }
                    
                    JobHandles = new NativeList<JobHandle>(numSelection, Allocator.TempJob);
                    for (int i = 0; i < numSelection; i++)
                    {
                        Regiment regiment = Selection.GetSelections[i].GetComponent<Regiment>();
                        using (TransformAccesses = new TransformAccessArray(regiment.PlacementTokens.ToArray()))
                        {
                            JUnitsTokenPlacement job = new JUnitsTokenPlacement
                            {
                                StartSelectionChangeLength = Selection.StartDragPlaceLength,
                                RegimentIndex = i,
                                NumRegimentSelected = Selection.NumSelection,
                                MinRowLength = regiment.GetRegimentType.minRow,
                                MaxRowLength = regiment.GetRegimentType.maxRow,
                                NumUnits = regiment.CurrentSize,
                                FullUnitSize = regiment.GetUnit.unitWidth + regiment.GetRegimentType.offsetInRow,
                                StartPosition = StartGroundHit,
                                EndPosition = EndGroundHit
                            };
                            JobHandles.AddNoResize(job.Schedule(TransformAccesses));
                        }
                    }
                    JobHandles.Dispose(JobHandles[^1]);
                }
            }
        }
        

        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            Debug.Log("cancel");
            //Hide Placer!
            //Apply new placement
        }
        
        //[BurstCompile(CompileSynchronously = true)]
        private struct JUnitsTokenPlacement : IJobParallelForTransform
        {
            [ReadOnly] public float StartSelectionChangeLength;
            [ReadOnly] public int RegimentIndex;
            [ReadOnly] public int NumRegimentSelected;
            [ReadOnly] public int MinRowLength;
            [ReadOnly] public int MaxRowLength;
            [ReadOnly] public int NumUnits;
            [ReadOnly] public float FullUnitSize;
            [ReadOnly] public float3 StartPosition;
            [ReadOnly] public float3 EndPosition;
            public void Execute(int index, TransformAccess transform)
            {
                //1) start drag after we hit the length Min of the selections
                float mouseDragLength = length(EndPosition - StartPosition) - StartSelectionChangeLength;
                
                //2) increase size placement Each length = UnitSize
                int newUnitLength = (int)floor(mouseDragLength / FullUnitSize);
                
                //HOW MANY ROWS?
                int unitPerRow = (int)floor(MinRowLength + max(0,newUnitLength / NumRegimentSelected));
                unitPerRow = clamp(unitPerRow,MinRowLength, MaxRowLength);

                int numRows = (int)ceil((float)NumUnits / unitPerRow); //Use to offset last row

                (int x, int z) = index.GetXY(unitPerRow);

                float3 direction = normalize(EndPosition - StartPosition); //Direction mouse end - start
                float3 crossDirection = normalize(cross(direction, -up())); //direction for back rows (second,thrid line etc...)
                
                //FirstStart when 2nd regiment? : index = 1
                float offsetRegiment = RegimentIndex * 2.5f;
                
                //ROWS POSITION (after first line complete position offset to create an other behind)
                float3 rowStart = GetRowStart(z, numRows, unitPerRow, direction, crossDirection);
                float3 rowEnd = EndPosition + crossDirection * (z * FullUnitSize);
                float3 newRowDirection = normalize(rowEnd - rowStart);
                    
                float3 unitPos = rowStart + newRowDirection * (x * FullUnitSize);

                //CAREFUL need to store somehow previous regimentSize
                unitPos +=((unitPerRow-1)*FullUnitSize) * direction * RegimentIndex; //Add + regiment current Size (row)

                unitPos += offsetRegiment * direction; //Add + regiment offset

                unitPos.y = 2; // need to delete/replace this

                transform.position = unitPos;
                transform.rotation = quaternion.LookRotation(-crossDirection, up());
            }

            private float3 GetRowStart(int z,int numRows,int unitPerRow, in float3 direction, in float3 crossDirection)
            {
                float3 rowStart = StartPosition + crossDirection * (FullUnitSize * z);
                
                int numUnitLeft = NumUnits - (numRows - 1) * unitPerRow;
                if (z == numRows - 1 && numUnitLeft < unitPerRow) //very last Row
                {
                    float lengthWithLeftUnits = (FullUnitSize * numUnitLeft);
                    float offsetStart = ((FullUnitSize * unitPerRow) - lengthWithLeftUnits) / 2f;
                    rowStart += direction * offsetStart;
                }
                return rowStart;
            }
        }
    }
}