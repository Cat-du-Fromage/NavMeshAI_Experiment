using System;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
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
        [SerializeField] private GameObject token;
        private SelectionRegister Register;

        private const int SpaceBeetweenRegiment = 2; //2 units
        //INPUT SYSTEM
        private SelectionInputController Control;
        private InputAction MouseAction;
        
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
        private JobHandle PlacementJobHandle;

        private bool HitGround(Ray ray) => Raycast(ray, out Hit, INFINITY, TerrainLayer);

        private void OnEnable() => Control.Enable();

        private void OnDisable() => Control.Disable();

        private void Awake()
        {
            PlayerCamera = Camera.main;
            Register = GetComponent<SelectionRegister>();
            
            Control ??= new SelectionInputController();
            MouseAction = Control.MouseControl.PlacementRightClickMove;
            
            MouseAction.EnableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);
        }
        
        private void OnDestroy() => MouseAction.DisableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);

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
                Regiment regiment = Register.Selections[0];
                EndGroundHit = Hit.point;
                LengthMouseDrag  = length(EndGroundHit - StartGroundHit);
                if (LengthMouseDrag >= Register.MinRowLength && LengthMouseDrag <= Register.MaxRowLength) // NEED UNIT (SIZE + Offset) * (MinRow-1)!
                {
                    NativeList<JobHandle> jhs = new NativeList<JobHandle>(numSelection, Allocator.TempJob);
                    for (int i = 0; i < numSelection; i++)
                    {
                        regiment = Register.Selections[i].GetComponent<Regiment>();
                        using (TransformAccesses = new TransformAccessArray(regiment.PositionTokens))
                        {
                            JUnitsTokenPlacement job = new JUnitsTokenPlacement
                            {
                                MaxSelectionLength = Register.MaxRowLength,
                                RegimentIndex = i,
                                NumRegimentSelected = Register.Selections.Count,
                                MaxRowLength = regiment.GetRegimentType.maxRow,
                                NumUnits = regiment.CurrentSize,
                                FullUnitSize = regiment.UnitSize.x + regiment.GetRegimentType.offsetInRow,
                                StartPosition = StartGroundHit,
                                EndPosition = EndGroundHit
                            };
                            jhs.AddNoResize(job.Schedule(TransformAccesses));
                        }
                    }
                    jhs.Dispose(jhs[^1]);
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
            [ReadOnly] public float MaxSelectionLength;
            [ReadOnly] public int RegimentIndex;
            [ReadOnly] public int NumRegimentSelected;
            [ReadOnly] public int MaxRowLength;
            [ReadOnly] public int NumUnits;
            [ReadOnly] public float FullUnitSize;
            [ReadOnly] public float3 StartPosition;
            [ReadOnly] public float3 EndPosition;
            public void Execute(int index, TransformAccess transform)
            {
                int unitPerRow = (int)ceil(length(EndPosition - StartPosition) / FullUnitSize);
                unitPerRow = min(unitPerRow,MaxRowLength);
                
                int numRows = (int)ceil(NumUnits / (float)unitPerRow); //Use to offset last row
                
                int z = (int)floor((float)index / unitPerRow);
                int x = index - (z * unitPerRow);
                float3 direction = normalize(EndPosition - StartPosition);
                float3 crossDirection = normalize(cross(direction, -up()));
                
                //FirstStart when 2nd regiment? : index = 1
                
                float3 rowStart = GetRowStart(z, numRows, unitPerRow, direction, crossDirection);
                
                float3 rowEnd = EndPosition + crossDirection * (z * FullUnitSize);
                
                float3 newDir = normalize(rowEnd - rowStart);
                    
                float3 unitPos = rowStart + (x * FullUnitSize) * newDir;

                unitPos.y = 2; // need to delete/replace this

                transform.position = unitPos;
                transform.rotation = quaternion.LookRotation(-crossDirection, up());
            }

            private float3 GetRowStart(int z,int numRows,int unitPerRow,float3 direction,float3 crossDirection)
            {
                float3 rowStart;
                if (z == numRows - 1)
                {
                    int numUnitLeft = NumUnits - (numRows - 1) * unitPerRow;
                    float lengthWithLeftUnits = FullUnitSize * numUnitLeft;
                    float offsetStart = (unitPerRow * FullUnitSize - lengthWithLeftUnits) / 2f;
                    rowStart = StartPosition + (direction * offsetStart) + crossDirection * (z * FullUnitSize);
                }
                else
                {
                    rowStart = StartPosition + crossDirection * (z * FullUnitSize);
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