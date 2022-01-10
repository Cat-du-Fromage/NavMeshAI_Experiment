using System;
using System.Collections;
using KaizerWaldCode.RTTUnits;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

using static UnityEngine.Physics;
using static Unity.Mathematics.math;

using static KWUtils.KWmesh;
using static KWUtils.KWRect;
using static KaizerWaldCode.PlayerEntityInteractions.RTTSelection.SelectionMeshUtils;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{
    [RequireComponent(typeof(SelectionRegister))]
    public class SelectionSystem : MonoBehaviour
    {
        public PlayerEntityInteractionInputsManager SelectionInputs;
        private InputAction SelectEvents;
        
        public SelectionRegister SelectRegister;
        
        private Camera PlayerCamera;

        //SELECTION CACHE
        private Transform RegimentSelected;

        private RaycastHit SingleHit;
        private readonly RaycastHit[] Hits = new RaycastHit[4]; //when mouse click we cast a ray

        private Ray SingleRay;
        private readonly Ray[] BoxRays = new Ray[4];
        //CONSTANT
        private readonly LayerMask TerrainLayer = 1 << 8;
        private readonly LayerMask UnitLayer = 1 << 9;
        
        //UI RECTANGLE
        private readonly Vector2[] UiCorners = new Vector2[4] {Vector2.down, Vector2.one, Vector2.up ,Vector2.right};
        private readonly Color UiColor = new Color(0.8f,0.8f,0.95f,0.25f);
        private readonly Color UiBorderColor = new Color(0.8f, 0.8f, 0.95f);
        
        //MESH SELECTION
        private MeshCollider SelectionCollider;
        private Mesh SelectionMesh;
        private readonly Vector3[] SelectionMeshVertices = BasicCube;

        //NEW INPUT SYSTEM
        private bool RunJob;
        private bool HitsSucceed;

        //INITIALIZATION
        //==============================================================================================================
        private void Awake()
        {
            PlayerCamera = Camera.main;
            SelectRegister = GetComponent<SelectionRegister>();

            SelectionInputs = GetComponent<PlayerEntityInteractionInputsManager>();

            SelectionMesh = InitializeMesh(SelectionMeshVertices);
            SelectionCollider = gameObject.InitializeCollider(SelectionMesh);
        }

        private void Start()
        {
            SelectEvents = SelectionInputs.SelectionEvents;
            SelectEvents.EnablePerformCancelEvent(OnPerformLeftClickMoveMouse, OnLeftClickRelease);
        }

        private void OnDestroy() => SelectEvents.DisablePerformCancelEvent(OnPerformLeftClickMoveMouse, OnLeftClickRelease);
        
        //EVENTS CALLBACKS
        //==============================================================================================================

        /// <summary>
        /// EVENT : When left-mouse click + move mouse
        /// </summary>
        /// <param name="ctx">Context(performed in this case); use to get (Vector2)mouse position</param>
        private void OnPerformLeftClickMoveMouse(InputAction.CallbackContext ctx)
        {
            RunJob = SelectionInputs.IsDragging && SelectionInputs.LeftClick && SelectionInputs.EndMouseClick[0] != SelectionInputs.EndMouseClick[1];
            if(!RunJob) return;
            UiCorners.GetBoxSelectionVertices(SelectionInputs.StartMouseClick, SelectionInputs.EndMouseClick[1]);
            HitsSucceed = BoxRaycast();
        }
        
        private void OnLeftClickRelease(InputAction.CallbackContext obj)
        {
            if (!SelectionInputs.ShiftPressed) SelectRegister.Clear();

            if (RunJob && HitsSucceed)
            {
                RetrieveBoxHits();
                UpdateSelectionMesh();
                SelectionCollider.enabled = true;
                StartCoroutine(DisableAfterSelections());
                RunJob = false;
                HitsSucceed = false;
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
            if (!SelectionInputs.ShiftPressed) SelectRegister.Clear();//selectionRegister.DeselectAll();
            SingleRay = PlayerCamera.ScreenPointToRay(SelectionInputs.EndMouseClick[1]);
            return SphereCast(SingleRay,1f, out SingleHit, INFINITY, UnitLayer); //use sphere so we don't miss between units
        }

        private void SingleHitSelect()
        {
            if (!SingleHit.transform.TryGetComponent(out SelectionComponent selectComp)) return;
            if (selectComp.IsSelected) return;
            
            RegimentSelected = SingleHit.transform.GetComponent<UnitComponent>().Regiment;
            SelectRegister.Add(RegimentSelected);
        }

        private bool BoxRaycast()
        {
            for (int i = 0; i < UiCorners.Length; i++)
            {
                BoxRays[i] = PlayerCamera.ScreenPointToRay(UiCorners[i]);
                if (!Raycast(BoxRays[i], out Hits[i], INFINITY, TerrainLayer)) return false;
            }
            return true;
        }
        
        private void RetrieveBoxHits()
        {
            for (int i = 0; i < Hits.Length; i++)
            {
                SelectionMeshVertices[i] = Hits[i].point;
                Vector3 direction = (Hits[i].point - BoxRays[i].origin).normalized;
                SelectionMeshVertices[i + 4] = BoxRays[i].origin + direction * PlayerCamera.nearClipPlane; //Use clip plane of the camera as vertices for the top mesh
                Debug.DrawLine(PlayerCamera.ScreenToWorldPoint(UiCorners[i]), Hits[i].point, Color.red, 3.0f);
            }
        }
        
        /// <summary>
        /// Wait until trigger life cycle end so every collision is register
        /// then disable collider
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisableAfterSelections()
        {
            yield return new WaitForFixedUpdate();
            SelectionCollider.enabled = false;
        }
        
        //USE FOR DRAG SELECTION
        private void OnTriggerEnter(Collider unitCollider)
        {
            RegimentSelected = unitCollider.transform.GetComponent<UnitComponent>().Regiment;
            
            if(!RegimentSelected.TryGetComponent(out RegimentComponent regComp)) return;
            if(regComp.IsSelected) return; //unit's regiment is already selected
            SelectRegister.Add(RegimentSelected);
        }
        
        //VISUAL UI FOR RECTANGLE
        //==============================================================================================================
        private void OnGUI()
        {
            if (!SelectionInputs.IsDragging || !SelectionInputs.LeftClick) return;
            Rect selectRectangle = GetScreenRect(SelectionInputs.StartMouseClick, SelectionInputs.EndMouseClick[0]);
            DrawFullScreenRect(selectRectangle, 2, UiColor, UiBorderColor);
        }
    }
}
