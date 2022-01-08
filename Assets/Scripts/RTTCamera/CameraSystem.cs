using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.InputSystem.Interactions;

using static KWUtils.KWmath;

using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using float2 = Unity.Mathematics.float2;

namespace KaizerWaldCode.RTTCamera
{
    public class CameraSystem : MonoBehaviour
    {
        [Min(1)]
        [SerializeField] private int rotationSpeed, baseMoveSpeed, zoomSpeed;

        private Controls controls;
        
        private bool canRotate;
        private int sprint;
        private float zoom;
        private Vector2 mouseStartPosition, mouseEndPosition;
        private Vector2 moveAxis;
        private Transform cameraTransform;
        private int MoveSpeed => baseMoveSpeed * sprint;
        
        private void Awake()
        {
            controls ??= new Controls();
            controls.Enable();
            
            cameraTransform = transform;
            MinMax(ref rotationSpeed, 1, rotationSpeed);
            MinMax(ref baseMoveSpeed, 1, baseMoveSpeed);
            MinMax(ref zoomSpeed, 1, zoomSpeed);
            MinMax(ref sprint, 1, sprint);
            canRotate = false;
        }

        private void Start()
        {
            MovementEvents(true);
            ZoomEvents(true);
            RotationEvents(true);
            SprintEvents(true);
        }

        private void OnDestroy()
        {
            MovementEvents(false);
            ZoomEvents(false);
            RotationEvents(false);
            SprintEvents(false);
        }

        private void Update()
        {
            if (canRotate)
                SetCameraRotation();

            if (moveAxis != Vector2.zero)
                MoveCamera();

            if (zoom != 0)
                cameraTransform.position = mad(up(), zoom, transform.position);
        }

        private void MoveCamera()
        {
            //real forward of the camera (aware of the rotation)
            Vector3 currentCameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
            Vector3 zAxis = Vector3.zero;
            Vector3 xAxis = Vector3.zero;

            if (moveAxis.x != 0) xAxis = moveAxis.x > 0 ? -cameraTransform.right : cameraTransform.right;
            if (moveAxis.y != 0) zAxis = moveAxis.y > 0 ? currentCameraForward : -currentCameraForward;
            cameraTransform.position += (xAxis + zAxis) * (max(1f,cameraTransform.position.y) * MoveSpeed * Time.deltaTime);
        }

        private void SetCameraRotation()
        {
            mouseEndPosition = Mouse.current.position.ReadValue();
            
            if (mouseEndPosition != mouseStartPosition)
            {
                float distanceX = (mouseEndPosition - mouseStartPosition).x * rotationSpeed;
                float distanceY = (mouseEndPosition - mouseStartPosition).y * rotationSpeed;
                
                cameraTransform.Rotate(0f, distanceX * Time.deltaTime, 0f, Space.World);
                cameraTransform.Rotate(-distanceY * Time.deltaTime, 0f, 0f, Space.Self);

                mouseStartPosition = mouseEndPosition;
            }
        }

        //EVENTS CALLBACK
        //==============================================================================================================
        
        //ROTATION
        private void RotateCamera(InputAction.CallbackContext ctx)
        {
            mouseStartPosition = Mouse.current.position.ReadValue();
            canRotate = true;
        }
        private void StopRotateCamera(InputAction.CallbackContext ctx) => canRotate = false;
        
        //MOVE
        private void MoveCamera(InputAction.CallbackContext ctx) => moveAxis = ctx.ReadValue<Vector2>();
        private void StopMoveCamera(InputAction.CallbackContext ctx) => moveAxis = Vector2.zero;
        //ZOOM
        private void ZoomCamera(InputAction.CallbackContext ctx) => zoom = ctx.ReadValue<float>();
        private void StopZoomCamera(InputAction.CallbackContext ctx) => zoom = 0;
        //SPRINT
        private void SprintCamera(InputAction.CallbackContext context) => sprint = 3;
        private void StopSprintCamera(InputAction.CallbackContext context) => sprint = 1;
        
        //ENABLE/DISABLE EVENTS
        //==============================================================================================================
        
        private void ZoomEvents(bool enable)
        {
            InputAction zoomCamera = controls.CameraControl.Zoom;
            if (enable)
            {
                zoomCamera.performed += ZoomCamera;
                zoomCamera.canceled += StopZoomCamera;
            }
            else
            {
                zoomCamera.performed -= ZoomCamera;
                zoomCamera.canceled -= StopZoomCamera;
            }
        }
        
        private void MovementEvents(bool enable)
        {
            InputAction move = controls.CameraControl.Mouvement;
            if (enable)
            {
                move.performed += MoveCamera;
                move.canceled += StopMoveCamera;
            }
            else
            {
                move.performed -= MoveCamera;
                move.canceled -= StopMoveCamera;
            }
        }
        
        private void RotationEvents(bool enable)
        {
            InputAction rotation = controls.CameraControl.Rotation;
            if (enable)
            {
                rotation.performed += RotateCamera;
                rotation.canceled += StopRotateCamera;
            }
            else
            {
                rotation.performed -= RotateCamera;
                rotation.canceled -= StopRotateCamera;
            }
        }
        
        private void SprintEvents(bool enable)
        {
            InputAction faster = controls.CameraControl.Faster;
            if (enable)
            {
                faster.performed += SprintCamera;
                faster.canceled += StopSprintCamera;
            }
            else
            {
                faster.performed -= SprintCamera;
                faster.canceled -= StopSprintCamera;
            }
        }
    }
}
