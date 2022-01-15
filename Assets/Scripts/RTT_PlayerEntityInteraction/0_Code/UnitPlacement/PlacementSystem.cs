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
    public class PlacementSystem : MonoBehaviour
    {
        private RegimentManager regimentManager;
        
        [SerializeField] private GameObject token;
        private SelectionRegister Register;

        private const int SpaceBeetweenRegiment = 2; //2 units
        //INPUT SYSTEM
        private PlayerEntityInteractionInputsManager Control;

        //RAYCAST
        private Camera PlayerCamera;
        private RaycastHit Hit;
        
        //RAYCAST PROPERTIES
        private Ray StartRay => PlayerCamera.ScreenPointToRay(MouseStartPosition);
        private Ray EndRay => PlayerCamera.ScreenPointToRay(MouseEndPosition);
        
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

        private bool HitGround(Ray ray) => Raycast(ray, out Hit, INFINITY, TerrainLayer);

        private void Awake()
        {
            PlayerCamera = Camera.main;
            Register = GetComponent<SelectionRegister>();
            
            Control = GetComponent<PlayerEntityInteractionInputsManager>();

            regimentManager = FindObjectOfType<RegimentManager>();
        }

        private void Start()
        {
            Control.PlacementEvents.EnableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);
            Control.CtrlEvents.EnableStartCancelEvent(OnStartCtrl, OnCancelCtrl);
        }
        
        private void OnStartCtrl(InputAction.CallbackContext ctx)
        {
            regimentManager.Regiments.ForEach( regiment => regiment.EnablePlacementToken(true));
        }
        private void OnCancelCtrl(InputAction.CallbackContext ctx)
        {
            regimentManager.Regiments.ForEach( regiment => regiment.EnablePlacementToken(false));
        }

        private void OnDestroy() => Control.PlacementEvents.DisableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);

        private void OnStartMouseClick(InputAction.CallbackContext ctx)
        {
            if (Register.Selections.Count == 0 || MouseStartPosition == ctx.ReadValue<Vector2>()) return;
            
            MouseStartPosition = ctx.ReadValue<Vector2>();
            StartGroundHit = HitGround(StartRay) ? Hit.point : StartGroundHit;
        }

        private void OnPerformMouseMove(InputAction.CallbackContext ctx)
        {
            //First : Update Mouse End Position Value
            MouseEndPosition = ctx.ReadValue<Vector2>();
            int numSelection = Register.Selections.Count;
            if( numSelection == 0) return;
            if (MouseEndPosition == MouseStartPosition) return;
            
            if (HitGround(EndRay))
            {
                EndGroundHit = Hit.point;
                LengthMouseDrag  = length(EndGroundHit - StartGroundHit);
                if (LengthMouseDrag >= Register.MinRowLength - 1) // NEED UNIT (SIZE + Offset) * (MinRow-1)!
                {
                    JobHandles = new NativeList<JobHandle>(numSelection, Allocator.TempJob);
                    for (int i = 0; i < numSelection; i++)
                    {
                        Regiment regiment = Register.Selections[i].GetComponent<Regiment>();
                        using (TransformAccesses = new TransformAccessArray(regiment.GetPlacementTokens.ToArray()))
                        {
                            JUnitsTokenPlacement job = new JUnitsTokenPlacement
                            {
                                SelectionMaxUnitPerRow = Register.GetSelectionMaxUniPerRow(),
                                StartSelectionChangeLength = Register.GetStartDragPlaceLength(),
                                MaxSelectionLength = Register.MaxRowLength,
                                RegimentIndex = i,
                                NumRegimentSelected = Register.Selections.Count,
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
                
                
                /*
                Transform regiment = Register.Selections[0];
                RegimentComponent regimentComp = regiment.GetComponent<RegimentComponent>();
                EndGroundHit = Hit.point;
                LengthMouseDrag = length(EndGroundHit - StartGroundHit);
                float maxUnitLength = (regimentComp.UnitSize.x + regimentComp.GetRegimentType.positionOffset) * (regimentComp.GetRegimentType.maxRow);
                if (LengthMouseDrag > (regimentComp.UnitSize.x * 4)) // NEED UNIT (SIZE + Offset) * (MinRow-1)!
                {
                    using (TransformAccesses = new TransformAccessArray(regimentComp.PositionTokens))
                    {
                        JUnitsTokenPlacement job = new JUnitsTokenPlacement
                        {
                            MaxRowLength = regimentComp.GetRegimentType.maxRow,
                            NumUnits = regimentComp.CurrentSize,
                            FullUnitSize = regimentComp.UnitSize.x + regimentComp.GetRegimentType.positionOffset,
                            StartPosition = StartGroundHit,
                            EndPosition = EndGroundHit
                        };
                        PlacementJobHandle = job.Schedule(TransformAccesses);
                    }
                }
                */
            }
        }
        

        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            Debug.Log("cancel");
            //Apply new placement
        }
        
        //[BurstCompile(CompileSynchronously = true)]
        private struct JUnitsTokenPlacement : IJobParallelForTransform
        {
            [ReadOnly] public int SelectionMaxUnitPerRow;
            [ReadOnly] public float StartSelectionChangeLength;
            [ReadOnly] public float MaxSelectionLength;
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
/*
        private void OnDrawGizmos()
        {
            if (StartGroundHit != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(StartGroundHit + Vector3.up, 0.5f);
            }

            if (EndGroundHit != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(EndGroundHit + Vector3.up, 0.5f);
            }
        }
        */
    }
    
    //OLD
    //[BurstCompile(CompileSynchronously = true)]
    /*
    public struct JPlacement : IJobFor
    {
        [ReadOnly]public float FullUnitSize;
        [ReadOnly]public float3 StartPosition;
        [ReadOnly]public float3 EndPosition;

        public NativeArray<float3> UnitPositions;
        public void Execute(int index)
        {
            int unitPerRow = (int)floor(length(EndPosition - StartPosition) / FullUnitSize);
            int numRows = (int)ceil(UnitPositions.Length / (float)unitPerRow);
            
            int z = (int)floor((float)index / unitPerRow);
            int x = index - (z * unitPerRow);
            
            float3 direction = normalize(EndPosition - StartPosition);
            float3 crossDirection = normalize(cross(direction, up()));
            
            float3 rowStart = StartPosition + crossDirection * z;
            float3 rowEnd = EndPosition + crossDirection * z;
                    
            float3 newDir = normalize(rowEnd - rowStart);
                    
            float3 unitPos = rowStart + (x * FullUnitSize) * newDir;
            
            unitPos.y = 2;

            UnitPositions[index] = unitPos;
        }
    }
    */
}