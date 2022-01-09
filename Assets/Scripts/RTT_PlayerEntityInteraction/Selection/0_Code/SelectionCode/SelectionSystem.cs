using System;
using System.Collections;
using KaizerWaldCode.RTTUnits;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
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
        private SelectionRegister Register;
        private Camera PlayerCamera;

        private SelectionInputController Control;
        private SelectionInputController.MouseControlActions MouseCtrl;
        private InputAction MouseEvents;
        
        //SELECTION CACHE
        private Transform CachedUnit;
        private Transform RegimentSelected;
        private RaycastHit Hit; //when mouse click we cast a ray
        
        //CONSTANT
        private readonly LayerMask TerrainLayer = 1 << 8;
        private readonly LayerMask UnitLayer = 1 << 9;
        
        //UI RECTANGLE
        private readonly Vector2[] UiCorners = new Vector2[4];
        private readonly Color UiColor = new Color(0.8f,0.8f,0.95f,0.25f);
        private readonly Color UiBorderColor = new Color(0.8f, 0.8f, 0.95f);
        
        //MESH SELECTION
        private MeshCollider SelectionCollider;
        private Mesh SelectionMesh;
        private readonly Vector3[] SelectionMeshVertices = BasicCube;

        //MOUSE POSITION VARIABLES
        private Vector2 StartMouseClick = Vector2.zero;
        private Vector2 EndMouseClick = Vector2.zero;

        //NEW INPUT SYSTEM
        private bool IsDragging;
        private bool ShiftKey => Keyboard.current.shiftKey.isPressed;
        
        //JOB SYSTEM
        private NativeArray<RaycastCommand> RaycastCommands;
        private NativeArray<RaycastHit> RaycastHits;
        private JobHandle JobHandle;

        //INITIALIZATION
        //==============================================================================================================
        private void OnEnable() => Control.Enable();
        private void OnDisable() => Control.Disable();

        private void OnDestroy() => MouseEvents.DisableAllEvents(OnStartMouseClick, OnPerformMoveMouse, OnCancelMouseClick);

        private void Awake()
        {
            PlayerCamera = Camera.main;
            Register = GetComponent<SelectionRegister>();
            
            Control ??= new SelectionInputController();
            MouseCtrl = Control.MouseControl;
            MouseEvents = Control.MouseControl.SelectionMouseLeftClick;
            
            SelectionMesh = InitializeMesh(SelectionMeshVertices);
            SelectionCollider = gameObject.InitializeCollider(SelectionMesh);
            
            MouseEvents.EnableAllEvents(OnStartMouseClick, OnPerformMoveMouse, OnCancelMouseClick);
        }

        //EVENTS CALLBACKS
        //==============================================================================================================
        
        /// <summary>
        /// EVENT : When mouse is click this frame (register only once)
        /// </summary>
        /// <param name="ctx">context(start in this case), use to get (Vector2)mouse position</param>
        private void OnStartMouseClick(InputAction.CallbackContext ctx) => StartMouseClick = ctx.ReadValue<Vector2>();

        /// <summary>
        /// EVENT : When left-mouse click + move mouse
        /// </summary>
        /// <param name="ctx">Context(performed in this case); use to get (Vector2)mouse position</param>
        private void OnPerformMoveMouse(InputAction.CallbackContext ctx)
        {
            EndMouseClick = ctx.ReadValue<Vector2>();
            IsDragging = lengthsq(EndMouseClick - StartMouseClick) > 160;
        }
        
        /// <summary>
        /// EVENT : When Left Click Is Released
        /// </summary>
        /// <param name="ctx">context(canceled in this case)</param>
        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            if (!IsDragging)
            {
                SimpleClickSelection();
            }
            else
            {
                UiCorners.GetBoxSelectionVertices(StartMouseClick, EndMouseClick);
                SelectionCollider.enabled = DragSelection();
                if (SelectionCollider.enabled) StartCoroutine(DisableAfterSelections());
                IsDragging = false;
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
        private void SimpleClickSelection()
        {
            if (!ShiftKey) Register.Clear();//selectionRegister.DeselectAll();
            
            Ray ray = PlayerCamera.ScreenPointToRay(StartMouseClick);
            bool hitUnit = Raycast(ray, out Hit, INFINITY, UnitLayer);
            CachedUnit = hitUnit ? Hit.transform : CachedUnit;
            
            if (hitUnit && CachedUnit.TryGetComponent(out SelectionComponent selectComp))
            {
                RegimentSelected = CachedUnit.GetComponent<UnitComponent>().Regiment;
                if(selectComp.IsSelected) 
                    Register.Remove(RegimentSelected);
                else 
                    Register.Add(RegimentSelected);
            }
        }
        
        /// <summary>
        /// Allow to select multiple Unit by drag selection
        /// </summary>
        /// <returns></returns>
        private bool DragSelection()
        {
            if (!ShiftKey) Register.Clear();//selectionRegister.DeselectAll();
            int hitCount = 0; //use to check if we have 4 corners when drag selection
            for (int i = 0; i < UiCorners.Length; i++)
            {
                Ray ray = PlayerCamera.ScreenPointToRay(UiCorners[i]);
                if (Raycast(ray, out Hit, INFINITY, TerrainLayer)) //only intersect terrain
                {
                    SelectionMeshVertices[i] = new Vector3(Hit.point.x, Hit.point.y, Hit.point.z);
                    SelectionMeshVertices[i + 4] = ray.origin + (Hit.point - ray.origin).normalized * PlayerCamera.nearClipPlane; //Use clip plane of the camera as vertices for the top mesh
                    hitCount++;
                    Debug.DrawLine(PlayerCamera.ScreenToWorldPoint(UiCorners[i]), Hit.point, Color.red, 3.0f);
                }
            }
            if (hitCount == 4)
            {
                UpdateSelectionMesh(); //CAREFUL must be done here or strange latency occure
                return true;
            }
            return false;
        }
        
        private bool PreselectOnDrag()
        {
            if (!ShiftKey) Register.Clear();//selectionRegister.DeselectAll();
            int hitCount = 0; //use to check if we have 4 corners when drag selection
            for (int i = 0; i < UiCorners.Length; i++)
            {
                Ray ray = PlayerCamera.ScreenPointToRay(UiCorners[i]);
                if (Raycast(ray, out Hit, INFINITY, TerrainLayer)) //only intersect terrain
                {
                    SelectionMeshVertices[i] = new Vector3(Hit.point.x, Hit.point.y, Hit.point.z);
                    SelectionMeshVertices[i + 4] = ray.origin + (Hit.point - ray.origin).normalized * PlayerCamera.nearClipPlane; //Use clip plane of the camera as vertices for the top mesh
                    hitCount++;
                    Debug.DrawLine(PlayerCamera.ScreenToWorldPoint(UiCorners[i]), Hit.point, Color.red, 3.0f);
                }
            }
            if (hitCount == 4)
            {
                UpdateSelectionMesh(); //CAREFUL must be done here or strange latency occure
                return true;
            }
            return false;
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
            CachedUnit = unitCollider.transform;
            RegimentSelected = CachedUnit.GetComponent<UnitComponent>().Regiment;
            if(!RegimentSelected.TryGetComponent(out RegimentComponent regComp)) return;
            if(regComp.SelectState) return; //unit's regiment is already selected
            
            Register.Add(RegimentSelected);
        }
        

        //VISUAL UI FOR RECTANGLE
        //==============================================================================================================
        private void OnGUI()
        {
            if (!IsDragging) return;
            Rect selectRectangle = GetScreenRect(StartMouseClick, EndMouseClick);
            DrawFullScreenRect(selectRectangle, 2, UiColor, UiBorderColor);
        }
    }
}
