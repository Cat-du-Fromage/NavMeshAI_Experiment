using System;
using System.Collections;
using KaizerWaldCode.RTTUnits;
using KaizerWaldCode.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

using static KaizerWaldCode.Utils.KWmesh;
using static Unity.Mathematics.math;
using static KaizerWaldCode.RTTSelection.RectangleSelectionUtils;

namespace KaizerWaldCode.RTTSelection
{
    public class SelectionSystem : MonoBehaviour
    {
        public SelectionRegister selectionRegister;
        
        public Camera playerCamera;

        private SelectionInputController control;

        private SelectionInputController.MouseControlActions mouseControl;
        
        //SELECTION CACHE
        private GameObject unitSelected;
        private Transform regimentSelected;
        private RaycastHit selectionHit; //when mouse click we cast a ray
        
        //CONSTANT
        private readonly LayerMask terrainLayerMask = 1 << 8;
        
        //UI RECTANGLE
        private readonly Vector2[] uiCorners = new Vector2[4];
        private readonly Color uiColor = new Color(0.8f,0.8f,0.95f,0.25f);
        private readonly Color uiBorderColor = new Color(0.8f, 0.8f, 0.95f);
        
        //MESH SELECTION
        private MeshCollider selectionBox;
        private Mesh selectionMesh;
        
        private readonly Vector3[] selectionMeshVertices = BasicCube;

        //MOUSE POSITION VARIABLES
        private Vector2 startMouseClick = Vector2.zero;
        private Vector2 endMouseClick = Vector2.zero;

        //NEW INPUT SYSTEM
        private bool isDragging;
        
        //INPUTS (clunky with input system)
        private Keyboard keyboard;
        private bool ShiftKey => keyboard.shiftKey.isPressed;

        //INITIALIZATION
        //==============================================================================================================
        private void OnEnable() => control.Enable();
        private void OnDisable() => control.Disable();
        
        private void OnDestroy()
        {
            mouseControl.MouseLeftClick.started -= OnStartMouseClick;
            mouseControl.MouseLeftClick.performed -= OnMouseClickMove;
            mouseControl.MouseLeftClick.canceled -= OnCancelMouseClick;
        }
        
        private void Awake()
        {
            control ??= new SelectionInputController();
            mouseControl = control.MouseControl;
            
            keyboard = Keyboard.current;
            
            playerCamera = Camera.main;
            selectionRegister = GetComponent<SelectionRegister>();
            InitializeMesh();
            InitializeCollider();
            
            mouseControl.MouseLeftClick.started += OnStartMouseClick;
            mouseControl.MouseLeftClick.performed += OnMouseClickMove;
            mouseControl.MouseLeftClick.canceled += OnCancelMouseClick;
        }
        //INITIALIZATION
        private void InitializeCollider()
        {
            selectionBox = gameObject.AddComponent<MeshCollider>();
            selectionBox.convex = true;
            selectionBox.isTrigger = true;
            selectionBox.enabled = false;
            selectionBox.sharedMesh = selectionMesh;
        }

        private void InitializeMesh()
        {
            selectionMesh = new Mesh();
            selectionMesh.name = "SelectorMesh 2";
            selectionMesh.SetVertices(selectionMeshVertices);
            selectionMesh.SetTriangles(CubeVertices,0,false);
        }

        //EVENTS
        //==============================================================================================================
        private void OnStartMouseClick(InputAction.CallbackContext ctx) => startMouseClick = ctx.ReadValue<Vector2>();

        private void OnMouseClickMove(InputAction.CallbackContext ctx)
        {
            endMouseClick = ctx.ReadValue<Vector2>();
            isDragging = lengthsq(endMouseClick - startMouseClick) > 160;
        }
        
        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            if (!isDragging)
            {
                SimpleClickSelection();
            }
            else
            {
                GetBoxSelectionVertices();
                selectionBox.enabled = DragSelection();
                if (selectionBox.enabled) StartCoroutine(DisableAfterSelections());
                isDragging = false;
            }
        }

        //METHODS
        //==============================================================================================================
        
        /// <summary>
        /// Update vertices of the selectionMesh
        /// </summary>
        private void UpdateSelectionMesh()
        {
            selectionMesh.SetVertexBufferData(selectionMeshVertices, 0, 0, 8, 0, NoRecalculations);
        }

        /// <summary>
        /// Mark Unit as selected on Click
        /// </summary>
        private void SimpleClickSelection()
        {
            if (!ShiftKey) selectionRegister.DeselectAllRegiment();//selectionRegister.DeselectAll();
            Ray ray = playerCamera.ScreenPointToRay(startMouseClick);
            if (Physics.Raycast(ray, out selectionHit, 5000.0f) && selectionHit.transform.TryGetComponent(out SelectionComponent selectComp))
            {
                if (selectionHit.transform.parent == null) return; //awkward, will we really have unit without regiment?
                unitSelected = selectionHit.transform.gameObject;
                if(selectComp.IsSelected)
                    selectionRegister.DeselectSingleRegiment(unitSelected);
                else
                    selectionRegister.AddRegimentSelection(unitSelected);
            }
        }
        
        /// <summary>
        /// Allow to select multiple Unit by drag selection
        /// </summary>
        /// <returns></returns>
        private bool DragSelection()
        {
            if (!ShiftKey) selectionRegister.DeselectAllRegiment();//selectionRegister.DeselectAll();
            int hitCount = 0; //use to check if we have 4 corners when drag selection
            for (int i = 0; i < uiCorners.Length; i++)
            {
                Ray ray = playerCamera.ScreenPointToRay(uiCorners[i]);
                if (Physics.Raycast(ray, out selectionHit, 50000.0f, terrainLayerMask)) //only intersect terrain
                {
                    selectionMeshVertices[i] = new Vector3(selectionHit.point.x, selectionHit.point.y, selectionHit.point.z);
                    selectionMeshVertices[i + 4] = ray.origin + (selectionHit.point - ray.origin) * playerCamera.nearClipPlane; //Use clip plane of the camera as vertices for the top mesh
                    hitCount++;
                    Debug.DrawLine(playerCamera.ScreenToWorldPoint(uiCorners[i]), selectionHit.point, Color.red, 3.0f);
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
            selectionBox.enabled = false;
        }
        
        
        //USE FOR DRAG SELECTION
        private void OnTriggerEnter(Collider unitCollider)
        {
            unitSelected = unitCollider.gameObject;
            regimentSelected = unitCollider.transform.parent;
            
            if (regimentSelected == null) return; //awkward will we really have unit without regiment?
            if(!regimentSelected.TryGetComponent(out RegimentComponent regComp)) return;
            if(regComp.SelectState) return;
            
            Debug.Log($"Select State = {regComp.SelectState}");
            //if (!unitSelected.TryGetComponent(out SelectionComponent select) &&
                //unitSelected.GetComponentInParent<RegimentComponent>().SelectState) return;
            selectionRegister.AddRegimentSelection(unitSelected);
            unitSelected.transform.parent.gameObject.GetComponent<RegimentComponent>().SetSelected(true);
            //selectionRegister.AddSelection(unitCollider.gameObject);
        }
        

        //VISUAL UI FOR RECTANGLE
        //==============================================================================================================
        private void OnGUI()
        {
            if (!isDragging) return;
            Rect selectRectangle = GetScreenRect(startMouseClick, endMouseClick);
            DrawFullScreenRect(selectRectangle, 2, uiColor, uiBorderColor);
        }
        
        /// <summary>
        /// Define the array "uiCorner" Depending on the position of the startMouseClick according to the current mouse position
        /// </summary>
        private void GetBoxSelectionVertices()
        {
            if (startMouseClick.IsLeft(endMouseClick)) //startMouseClick is Left
            {
                if (startMouseClick.IsAbove(endMouseClick)) //LeftTop
                {
                    uiCorners[0] = startMouseClick;
                    uiCorners[1] = new Vector2(endMouseClick.x, startMouseClick.y);//top right
                    uiCorners[2] = new Vector2(startMouseClick.x, endMouseClick.y);//bottom left
                    uiCorners[3] = endMouseClick;
                }
                else //LeftBot
                {
                    uiCorners[0] = new Vector2(startMouseClick.x, endMouseClick.y);//top left
                    uiCorners[1] = endMouseClick;
                    uiCorners[2] = startMouseClick;
                    uiCorners[3] = new Vector2(endMouseClick.x, startMouseClick.y);//bottom right
                }
            }
            else //startMouseClick is Right
            {
                if (startMouseClick.IsAbove(endMouseClick)) //RightTop
                { 
                    uiCorners[0] = new Vector2(endMouseClick.x, startMouseClick.y);//top left
                    uiCorners[1] = startMouseClick;
                    uiCorners[2] = endMouseClick;
                    uiCorners[3] = new Vector2(startMouseClick.x, endMouseClick.y);//bottom right
                }
                else //RightBot
                {
                    uiCorners[0] = endMouseClick;
                    uiCorners[1] = new Vector2(startMouseClick.x, endMouseClick.y);//top right
                    uiCorners[2] = new Vector2(endMouseClick.x, startMouseClick.y);//bottom left
                    uiCorners[3] = startMouseClick;
                }
            }
        }
    }
}
