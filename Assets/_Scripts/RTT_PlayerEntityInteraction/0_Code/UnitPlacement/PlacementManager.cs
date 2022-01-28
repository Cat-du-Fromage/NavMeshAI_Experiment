using System;
using System.Collections.Generic;
using KaizerWaldCode.Globals;
using KaizerWaldCode.RTTUnits;
using KaizerWaldCode.Utils;
using KWUtils;
//using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionUtils;
using quaternion = Unity.Mathematics.quaternion;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    public class PlacementManager : MonoBehaviour, IPlacement<Regiment>
    {
        [SerializeField] private PlacementTokensPool token;
        public IMediator<Regiment> Mediator { get; set; }
        
        //====================================================
        private readonly SelectionData Selection = new SelectionData();
        private readonly List<Renderer> NextDestinationsRenderer = new List<Renderer>();
        private Dictionary<Regiment, Transform[]> NextDestinations = new Dictionary<Regiment, Transform[]>();
        //====================================================
        
        private Camera PlayerCamera;
        private bool TokensVisible;
        
        //INPUT SYSTEM
        [SerializeField]private PlayerEntityInteractionInputsManager inputs;

        //CACHED MOUSE POSITION ON SCREEN
        private Vector2 MouseStartPosition;
        private Vector2 MouseEndPosition;

        //CACHED POSITION MOUSE IN GAME
        private Vector3 StartGroundHit;
        private Vector3 EndGroundHit;

        private float LengthMouseDrag => (EndGroundHit - StartGroundHit).magnitude;
        
        //JOB SYSTEM
        private TransformAccessArray TransformAccesses;
        private NativeList<JobHandle> JobHandles;
        private JobHandle PlacementJobHandle;
        
        //RAYCAST PROPERTIES
        private RaycastHit Hit;
        private Ray StartRay => PlayerCamera.ScreenPointToRay(MouseStartPosition);
        private Ray EndRay => PlayerCamera.ScreenPointToRay(MouseEndPosition);
        private bool HitGround(Ray ray) => Raycast(ray, out Hit, INFINITY, StaticDatas.TerrainLayer);
        
        private void DisplayNextDestination(bool enable) => NextDestinationsRenderer.ForEach(r => r.enabled = enable);

        public void UpdateSelectionData(in Regiment regiment)
        {
            if (Selection.Regiments.Contains(regiment)) return;
            Selection.OnAddRegiment(regiment);
            Transform[] tokens = new Transform[regiment.Units.Count];
            
            for (int i = 0; i < regiment.Units.Count; i++)
            {
                GameObject pooledToken = token.tokens.Get();
                tokens[i] = pooledToken.transform;
                NextDestinationsRenderer.Add(tokens[i].GetComponent<Renderer>());
            }
            NextDestinations.Add(regiment, tokens);
        }

        public void ClearSelectionData()
        {
            ClearDestinationTokens();
            Selection.OnClearRegiment();
        }

        public void ClearDestinationTokens()
        {
            NextDestinationsRenderer.Clear();
            token.ReleaseAll(ref NextDestinations);
            NextDestinations.Clear();
        }

        private void Awake()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            PlayerCamera = Camera.main;
        }

        private void Start()
        {
            inputs ??= GetComponent<PlayerEntityInteractionInputsManager>();
            inputs.SpaceEvents.EnableStartCancelEvent(OnStartSpace, OnCancelSpace);
            inputs.PlacementEvents.EnableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);
        }
        private void OnDestroy() => inputs.PlacementEvents.DisableAllEvents(OnStartMouseClick, OnPerformMouseMove, OnCancelMouseClick);
        
        private void OnStartSpace(InputAction.CallbackContext ctx) => Mediator.NotifyDisplayTokens(this,true);
        private void OnCancelSpace(InputAction.CallbackContext ctx) => Mediator.NotifyDisplayTokens(this,false);
        
        private void OnStartMouseClick(InputAction.CallbackContext ctx)
        {
            if (Selection.NumSelection == 0 || MouseStartPosition == ctx.ReadValue<Vector2>()) return;
            
            MouseStartPosition = ctx.ReadValue<Vector2>();
            StartGroundHit = HitGround(StartRay) ? Hit.point : StartGroundHit;
        }

        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            if (TokensVisible)
            {
                Mediator.NotifyDestinationSet(this, NextDestinations);
                SetTokenVisible(false);
            }
        }
        
        private void OnPerformMouseMove(InputAction.CallbackContext ctx)
        {
            //First : Update Mouse End Position Value
            MouseEndPosition = ctx.ReadValue<Vector2>();
            if( Selection.NumSelection == 0) return;
            if (MouseEndPosition == MouseStartPosition) return;
            
            if (HitGround(EndRay))
            {
                EndGroundHit = Hit.point;
                if (LengthMouseDrag >= Selection.MinRowLength - 1) // NEED UNIT (SIZE + Offset) * (MinRow-1)!
                {
                    //SET Marker Visible!
                    if (!TokensVisible)
                    {
                        SetTokenVisible(true);
                    }
                    PlaceDestinationTokens();
                }
                else if(TokensVisible)
                {
                    SetTokenVisible(false);
                }
            }
        }

        private void SetTokenVisible(bool enable)
        {
            TokensVisible = enable;
            DisplayNextDestination(enable);
        }

        private void PlaceDestinationTokens()
        {
            JobHandles = new NativeList<JobHandle>(Selection.NumSelection, Allocator.TempJob);
            int currentIteration = 0;
            foreach((Regiment regiment, Transform[] value) in NextDestinations)
            {
                //Regiment regiment = Selection.GetSelections[currentIteration];
                using (TransformAccesses = new TransformAccessArray(value))
                {
                    JUnitsTokenPlacement job = new JUnitsTokenPlacement
                    {
                        StartSelectionChangeLength = Selection.StartDragPlaceLength,
                        RegimentIndex = currentIteration, //Change this, this is not the index of the regiment! but index in the loop
                        NumRegimentSelected = Selection.NumSelection,
                        MinRowLength = regiment.GetRegimentType.minRow,
                        MaxRowLength = regiment.GetRegimentType.maxRow,
                        NumUnits = regiment.CurrentSize,
                        FullUnitSize = regiment.GetUnitType.unitWidth + regiment.GetRegimentType.offsetInRow,
                        StartPosition = StartGroundHit,
                        EndPosition = EndGroundHit
                    };
                    JobHandles.AddNoResize(job.Schedule(TransformAccesses));
                }
                currentIteration++;
            }
            JobHandles.Dispose(JobHandles[^1]);
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

                unitPos.y = 1; // need to delete/replace this

                transform.position = unitPos;
                transform.rotation = quaternion.LookRotation(-crossDirection, up());
            }

            private float3 GetRowStart(int z, int numRows, int unitPerRow, in float3 direction, in float3 crossDirection)
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
