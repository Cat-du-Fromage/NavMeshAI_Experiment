using System;
using System.Linq;
using KaizerWaldCode.RTTSelection;
using KaizerWaldCode.RTTUnits;
using KWUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionUtils;

namespace UnityTemplateProjects.RTTUnitPlacement
{
    public class PlacementSystem : MonoBehaviour
    {
        [SerializeField] private GameObject token;
        private SelectionRegister Selections;
        
        //INPUT SYSTEM
        private PlacementInputController Control;
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

        private bool HitGround(Ray ray) => Raycast(ray, out Hit, INFINITY, TerrainLayer);

        private void OnEnable() => Control.Enable();

        private void OnDisable() => Control.Disable();

        private void Awake()
        {
            PlayerCamera = Camera.main;
            Selections = GetComponent<SelectionRegister>();
            
            Control ??= new PlacementInputController();
            MouseAction = Control.PlacementControl.RightClickMove;
            
            MouseAction.EnableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);
        }

        private void OnDestroy() => MouseAction.DisableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);

        private void OnStartMouseClick(InputAction.CallbackContext ctx)
        {
            Debug.Log("start");
            if (Selections.Count == 0 || MouseStartPosition == ctx.ReadValue<Vector2>()) return;
            
            MouseStartPosition = ctx.ReadValue<Vector2>();
            StartGroundHit = HitGround(StartRay) ? Hit.point : StartGroundHit;
        }

        private void OnPerformMouseMove(InputAction.CallbackContext ctx)
        {
            Debug.Log("perform");
            MouseEndPosition = ctx.ReadValue<Vector2>();
            if (MouseEndPosition == MouseStartPosition) return;
            if (HitGround(EndRay))
            {
                EndGroundHit = Hit.point;
                if (length(EndGroundHit - StartGroundHit) > 4) // NEED UNIT (SIZE + Offset) * (MinRow-1)!
                {
                    Transform regiment = Selections.GetSelections.ElementAt(0).Value;
                    RegimentComponent regimentComp = regiment.GetComponent<RegimentComponent>();
                    int numTransform = regimentComp.GetComponentsInChildren<Transform>().Length;
                    Debug.Log($"Get {regimentComp.Units.Length} should be {regimentComp.CurrentSize}");
                    //TestFormation();
                    
                    
                    
                    NativeArray<float3> unitPosition = AllocNtvAry<float3>(regimentComp.CurrentSize);
                    JPlacement placeJob = new JPlacement
                    {
                        FullUnitSize = regimentComp.UnitSize.x + regimentComp.GetRegimentType.positionOffset,
                        StartPosition = StartGroundHit,
                        EndPosition = EndGroundHit,
                        UnitPositions = unitPosition
                    };
                    JobHandle jh = placeJob.ScheduleParallel(regimentComp.CurrentSize, JobWorkerCount - 1, default);
                    
                    TransformAccessArray transformsAccess = new TransformAccessArray(regimentComp.Units);

                    JTranslateUnit job2 = new JTranslateUnit()
                    {
                        NewPositions = unitPosition,
                    };
                    JobHandle jh2 = job2.Schedule(transformsAccess, jh);
                    jh2.Complete();


                    /*
                    for (int i = 0; i < unitPosition.Length; i++)
                    {
                        regiment.transform.GetChild(i).position = unitPosition[i];
                    }
                    */
                    unitPosition.Dispose();
                    transformsAccess.Dispose();
                }
            }
            
            //Check Length end-Start

            //
        }

        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            Debug.Log("cancel");
            //Apply new placement
        }
//SelectionMeshVertices[i + 4] = ray.origin + (Hit.point - ray.origin) * PlayerCamera.nearClipPlane;

//GOOD CANDIDATE FOR JOB SYSTEM! we will calculate Position and store them in an array
        private void TestFormation()
        {
            RegimentComponent regimentComp;
            //Get How Many are selected ?
            for (int i = 0; i < Selections.Count; i++)
            {
                //EXPENSIVE!
                Transform regiment = Selections.GetSelections.ElementAt(i).Value;
                regimentComp = regiment.GetComponent<RegimentComponent>();
                
                Vector3 startPosition = StartGroundHit;
                //startPosition.y = regimentComp.UnitSize.y;

                //DIRECTIONS
                Vector3 direction = (EndGroundHit - StartGroundHit).normalized;
                Vector3 crossDirection = Vector3.Cross(direction, Vector3.up).normalized;

                float fullUnitSize = regimentComp.UnitSize.x + regimentComp.GetRegimentType.positionOffset;
                int numUnitOnRow = (int)floor(length(EndGroundHit - StartGroundHit) / fullUnitSize);
                int numRow = (int)ceil(regiment.childCount / (float)numUnitOnRow);
                //Debug.Log($"num Unit Per Row {numUnitOnRow}");
                Debug.Log($"numRow {numRow}");
                for (int j = 0; j < regiment.childCount; j++)
                {
                    int z = (int)floor((float)j / numUnitOnRow);
                    int x = j - (z * numUnitOnRow);

                    Vector3 rowStart = startPosition + crossDirection * z;
                    Vector3 rowend = EndGroundHit + crossDirection * z;
                    
                    Vector3 newDir = (rowend - rowStart).normalized;
                    
                    Vector3 unitPos = rowStart + (x * fullUnitSize) * newDir;
                    unitPos.y = 2;
                    
                    regiment.transform.GetChild(j).position = unitPos;
                    
                }
            }
        }
        
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct JPlacement : IJobFor
    {
        //[ReadOnly]public int NumRows;
        //[ReadOnly]public int UnitPerRow;
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

    [BurstCompile]
    public struct JTranslateUnit : IJobParallelForTransform
    {
        public NativeArray<float3> NewPositions;
        public void Execute(int index, TransformAccess transform)
        {
            transform.position = NewPositions[index];
        }
    }
}