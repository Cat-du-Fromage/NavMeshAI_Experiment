#pragma warning disable 4014

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using KaizerWaldCode.RTTUnits;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks.Triggers;
using KaizerWaldCode.Globals;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;

using static KaizerWaldCode.Globals.StaticDatas;
using static KWUtils.KWmath;
using static KWUtils.KWmesh;
using static KWUtils.KWRect;
using static KaizerWaldCode.PlayerEntityInteractions.RTTSelection.SelectionMeshUtils;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{
    public class SelectionManager : MonoBehaviour, ISelector<Regiment>
    {
        public IMediator<Regiment> Mediator { get; set; }
        
        //=========================================
        //SELECTION ORDER
        private Queue<Regiment> RegimentsToSelect = new Queue<Regiment>();
        private bool ClearSelection;
        //=========================================
        
        //NEW INPUT SYSTEM
        [SerializeField] private PlayerEntityInteractionInputsManager inputs;

        private Camera PlayerCamera;

        //SELECTION CACHE
        private Regiment RegimentSelected;

        private RaycastHit SingleHit;
        private readonly RaycastHit[] Hits = new RaycastHit[4]; //when mouse click we cast a ray

        private Ray SingleRay;
        private readonly Ray[] BoxRays = new Ray[4];
        
        //UI RECTANGLE
        private readonly Vector2[] UiCorners = new Vector2[4] {Vector2.down, Vector2.one, Vector2.up ,Vector2.right};

        //MESH SELECTION
        private MeshCollider SelectionCollider;
        private Mesh SelectionMesh;
        private readonly Vector3[] SelectionMeshVertices = BasicCube;
        
        private bool RunJob;
        private bool HitsSucceed;

        //INITIALIZATION
        //==============================================================================================================
        private void Awake()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            PlayerCamera = Camera.main;
            SelectionMesh = InitializeMesh(SelectionMeshVertices);
            SelectionCollider = gameObject.InitializeCollider(SelectionMesh);
        }

        private void Start()
        {
            inputs ??= GetComponent<PlayerEntityInteractionInputsManager>();
            inputs.SelectionEvents.EnablePerformCancelEvent(OnPerformLeftClickMoveMouse, OnLeftClickRelease);
        }

        private void OnDestroy() => inputs.SelectionEvents.DisablePerformCancelEvent(OnPerformLeftClickMoveMouse, OnLeftClickRelease);

        /// <summary>
        /// We check each update if we need to add or clear Selection
        /// </summary>
        private void Update()
        {
            if (RegimentsToSelect.Count == 0 && !ClearSelection) return;
            if (ClearSelection)
            {
                Mediator.NotifyClearSelections(this);
                ClearSelection = false;
                return;
            }

            for (int i = 0; i < RegimentsToSelect.Count; i++)
            {
                Mediator.NotifyEntitySelected(this, RegimentsToSelect.Dequeue());
            }
        }

        //EVENTS CALLBACKS
        //==============================================================================================================

        /// <summary>
        /// EVENT : When left-mouse click + move mouse
        /// </summary>
        /// <param name="ctx">Context(performed in this case); use to get (Vector2)mouse position</param>
        private void OnPerformLeftClickMoveMouse(InputAction.CallbackContext ctx)
        {
            RunJob = inputs.IsDragging && inputs.LeftClick && inputs.EndMouseClick[0] != inputs.EndMouseClick[1];
            if(!RunJob) return;
            UiCorners.GetBoxSelectionVertices(inputs.StartMouseClick, inputs.EndMouseClick[1]);
            HitsSucceed = BoxRaycast();
        }
        
        private void OnLeftClickRelease(InputAction.CallbackContext obj)
        {
            if (!inputs.ShiftPressed) ClearSelection = true;
            if (RunJob && HitsSucceed)
            {
                RetrieveBoxHits();
                UpdateSelectionMesh();
                SelectionCollider.enabled = true;
                DisableAfter();
                RunJob = HitsSucceed = false;
            }
            else if(SingleRayCast())
            {
                SingleHitSelect();
            }
        }
        
        //METHODS
        //==============================================================================================================
        
        /// <summary>
        /// Update vertices of the selectionMesh
        /// </summary>
        private void UpdateSelectionMesh() => SelectionMesh.SetVertices(SelectionMeshVertices);

        /// <summary>
        /// Mark Unit as selected on Click
        /// </summary>
        private bool SingleRayCast()
        {
            if (!inputs.ShiftPressed) ClearSelection = true;
            SingleRay = PlayerCamera.ScreenPointToRay(inputs.EndMouseClick[1]);
            //Check if we hit more than 1 unit with a sphere cast
            return SphereCastNonAlloc(SingleRay, 0.5f, Hits, UnitLayer) > 1 ? 
                Raycast(SingleRay, out SingleHit, INFINITY, UnitLayer) : 
                SphereCast(SingleRay,0.5f, out SingleHit, INFINITY, UnitLayer);
        }

        private void SingleHitSelect()
        {
            if (!SingleHit.transform.TryGetComponent(out Unit unit)) return;
            if (unit.Regiment.IsSelected) return;
            RegimentsToSelect.Enqueue(unit.Regiment);
        }

        private bool BoxRaycast()
        {
            for (int i = 0; i < UiCorners.Length; i++)
            {
                BoxRays[i] = PlayerCamera.ScreenPointToRay(UiCorners[i]);
                if (!Raycast(BoxRays[i], out Hits[i], INFINITY, StaticDatas.TerrainLayer)) return false;
            }
            return true;
        }
        
        private void RetrieveBoxHits()
        {
            for (int i = 0; i < Hits.Length; i++)
            {
                SelectionMeshVertices[i] = Hits[i].point;
                Vector3 direction = Direction(Hits[i].point, BoxRays[i].origin);
                SelectionMeshVertices[i + 4] = BoxRays[i].origin + direction * PlayerCamera.nearClipPlane; //Use clip plane of the camera as vertices for the top mesh
                Debug.DrawLine(PlayerCamera.ScreenToWorldPoint(UiCorners[i]), Hits[i].point, Color.red, 3.0f);
            }
        }

        private async UniTask DisableAfter()
        {
            await UniTask.DelayFrame(3);
            SelectionCollider.enabled = false;
        }

        //USE FOR DRAG SELECTION
        private void OnTriggerEnter(Collider unitCollider)
        {
            if(!unitCollider.TryGetComponent(out Unit unit)) return;
            if(unit.Regiment.IsSelected) return;
            RegimentsToSelect.Enqueue(unit.Regiment);
        }
        
        //VISUAL UI FOR RECTANGLE
        //==============================================================================================================
        private void OnGUI()
        {
            if (!inputs.IsDragging || !inputs.LeftClick) return;
            Rect selectRectangle = GetScreenRect(inputs.StartMouseClick, inputs.EndMouseClick[0]);
            DrawFullScreenRect(selectRectangle, 2);
        }
    }
}
