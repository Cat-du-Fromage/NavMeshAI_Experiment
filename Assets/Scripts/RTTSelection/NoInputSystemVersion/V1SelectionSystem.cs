using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.Utils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.KWmesh;
using static Unity.Mathematics.math;

using UnityEngine.Rendering;

namespace KaizerWaldCode.RTTSelection
{
    public class V1SelectionSystem : MonoBehaviour
    {
        public SelectionRegister selectionRegister;
        
        public Camera playerCamera;

        //SELECTION CACHE
        private GameObject unitSelected;
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
        
        private readonly Vector3[] bottomMeshVertices = CubeBottom;
        private readonly Vector3[] topMeshVertices = CubeTop;
        private readonly Vector3[] selectionMeshVertices = BasicCube;

        //MOUSE POSITION VARIABLES
        private Vector2 startMouseClick = Vector2.zero;
        private Vector2 EndMouseClick => Mouse.current.position.ReadValue();
        
        //NEW INPUT SYSTEM
        private bool leftClickPressed = false;
        //INPUTS
        //may switch to input system for more clarity
        
        private Mouse mouse;
        
        private ButtonControl mouseLeftClick;
        private Keyboard keyboard;
        private bool MouseIsDragged => startMouseClick != EndMouseClick && lengthsq(EndMouseClick - startMouseClick) > 160;
        
        //OLD
        //=================
        private bool ShiftKey => keyboard.shiftKey.isPressed;

        //INITIALIZATION
        //==============================================================================================================

        private void Awake()
        {
            mouse = Mouse.current;
            keyboard = Keyboard.current;
            mouseLeftClick = Mouse.current.leftButton;
            
            playerCamera = Camera.main;
            selectionRegister = GetComponent<SelectionRegister>();
            InitializeMesh();
            InitializeCollider();
            
        }

        private void Start() => startMouseClick = mouse.position.ReadValue();

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
            selectionMesh.name = "SelectorMesh";
            selectionMesh.SetVertices(selectionMeshVertices);
            selectionMesh.SetTriangles(CubeVertices,0,false);
        }
        
        //UPDATE
        //==============================================================================================================


        private void Update()
        {
            
            if(!mouseLeftClick.wasReleasedThisFrame && !mouseLeftClick.isPressed) return;

            if (mouseLeftClick.wasPressedThisFrame)
            {
                OnLeftClick();
            }
            else if (mouseLeftClick.wasReleasedThisFrame)
            {
                if (!ShiftKey) selectionRegister.DeselectAll();

                if (!MouseIsDragged)
                {
                    SimpleClickSelection();
                }
                else
                {
                    GetBoxSelectionVertices(); //selectionRectangleCorners is updated here
                    selectionBox.enabled = DragSelection();
                    if (selectionBox.enabled)
                    {
                        StartCoroutine(DisableAfterSelections());
                    }
                }
            }
        }

        /// <summary>
        /// Initialize Start position of the mouse (only on "pressed this frame")
        /// </summary>
        private void OnLeftClick() => startMouseClick = mouse.position.ReadValue();
        
        /// <summary>
        /// Update vertices of the selectionMesh
        /// </summary>
        private void UpdateSelectionMesh()
        {
            //selectionMeshVertices.GetFromMerge(bottomMeshVertices, topMeshVertices);
            for (int i = 0; i < selectionMeshVertices.Length >> 1; i++) // (n >> 1) = (n / 2)
            {
                selectionMeshVertices[i] = bottomMeshVertices[i];
                selectionMeshVertices[i + 4] = topMeshVertices[i];
            }
            selectionMesh.SetVertexBufferData(selectionMeshVertices, 0, 0, 8, 0, NoRecalculations);
        }

        /// <summary>
        /// Mark Unit as selected on Click
        /// </summary>
        private void SimpleClickSelection()
        {
            Ray ray = playerCamera.ScreenPointToRay(startMouseClick);
            if (Physics.Raycast(ray, out selectionHit, 5000.0f) && selectionHit.transform.TryGetComponent(out SelectionComponent selectComp))
            {
                unitSelected = selectionHit.transform.gameObject;
                if(selectComp.IsSelected)
                    selectionRegister.DeselectSingleUnit(unitSelected.GetInstanceID());
                else
                    selectionRegister.AddSelection(unitSelected);
            }
        }
        
        /// <summary>
        /// Allow to select multiple Unit by drag selection
        /// </summary>
        /// <returns></returns>
        private bool DragSelection()
        {
            int hitCount = 0; //use to check if we have 4 corners when drag selection
            for (int i = 0; i < uiCorners.Length; i++)
            {
                Ray ray = playerCamera.ScreenPointToRay(uiCorners[i]);
                if (Physics.Raycast(ray, out selectionHit, 50000.0f, terrainLayerMask)) //only intersect terrain
                {
                    bottomMeshVertices[i] = new Vector3(selectionHit.point.x, selectionHit.point.y, selectionHit.point.z);
                    topMeshVertices[i] = ray.origin + (selectionHit.point - ray.origin) * playerCamera.nearClipPlane; //Use clip plane of the camera as vertices for the top mesh
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
            if (!unitCollider.gameObject.TryGetComponent(out SelectionComponent select)) return;
            selectionRegister.AddSelection(unitCollider.gameObject);
        }
        

        //VISUAL UI FOR RECTANGLE
        //==============================================================================================================
        private void OnGUI()
        {
            if (!(mouseLeftClick.isPressed && MouseIsDragged)) return;
            Rect selectRectangle = RectangleSelectionUtils.GetScreenRect(startMouseClick, EndMouseClick);
            RectangleSelectionUtils.DrawScreenRect(selectRectangle, uiColor);
            RectangleSelectionUtils.DrawScreenRectBorder(selectRectangle, 2, uiBorderColor);
        }
        
        /// <summary>
        /// Define the array "uiCorner" Depending on the position of the startMouseClick according to the current mouse position
        /// </summary>
        private void GetBoxSelectionVertices()
        {
            Vector2 endPoint = EndMouseClick;
            if (startMouseClick.IsLeft(endPoint)) //startMouseClick is Left
            {
                if (startMouseClick.IsAbove(endPoint)) //LeftTop
                {
                    uiCorners[0] = startMouseClick;
                    uiCorners[1] = new Vector2(endPoint.x, startMouseClick.y);//top right
                    uiCorners[2] = new Vector2(startMouseClick.x, endPoint.y);//bottom left
                    uiCorners[3] = endPoint;
                }
                else //LeftBot
                {
                    uiCorners[0] = new Vector2(startMouseClick.x, endPoint.y);//top left
                    uiCorners[1] = endPoint;
                    uiCorners[2] = startMouseClick;
                    uiCorners[3] = new Vector2(endPoint.x, startMouseClick.y);//bottom right
                }
            }
            else //startMouseClick is Right
            {
                if (startMouseClick.IsAbove(endPoint)) //RightTop
                { 
                    uiCorners[0] = new Vector2(endPoint.x, startMouseClick.y);//top left
                    uiCorners[1] = startMouseClick;
                    uiCorners[2] = endPoint;
                    uiCorners[3] = new Vector2(startMouseClick.x, endPoint.y);//bottom right
                }
                else //RightBot
                {
                    uiCorners[0] = endPoint;
                    uiCorners[1] = new Vector2(startMouseClick.x, endPoint.y);//top right
                    uiCorners[2] = new Vector2(endPoint.x, startMouseClick.y);//bottom left
                    uiCorners[3] = startMouseClick;
                }
            }
        }
    }
}
