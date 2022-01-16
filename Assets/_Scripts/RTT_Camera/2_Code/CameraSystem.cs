using System;
using KWUtils;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

using static KWUtils.KWmath;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTCamera
{
    public class CameraSystem : MonoBehaviour
    {
        //[Min(1)]
        //[SerializeField] private int rotationSpeed, baseMoveSpeed, zoomSpeed;

        [SerializeField]private CameraInputData cameraData;
        
        private Transform CameraTransform;
        private Controls CameraControls;
        
        private bool IsRotating;
        private bool IsSprinting;
        private float Zoom;
        private Vector2 MouseStartPosition, MouseEndPosition;
        private Vector2 MoveAxis;

        private Vector3 ZAxisRotation;
        private Vector3 XAxisRotation;
        
        private float TargetYaw;
        private float TargetPitch;
        
        //INPUT ACTIONS
        InputAction ZoomCameraAction => CameraControls.CameraControl.Zoom;
        InputAction MoveAction => CameraControls.CameraControl.Mouvement;
        InputAction RotationAction => CameraControls.CameraControl.Rotation;
        InputAction SprintAction => CameraControls.CameraControl.Faster;
        
        //UPDATED MOVE SPEED
        private int MoveSpeed => IsSprinting ? cameraData.baseMoveSpeed * cameraData.sprint : cameraData.baseMoveSpeed;
        
        
        private void Awake()
        {
            CameraControls ??= new Controls();
            CameraControls.Enable();
            
            CameraTransform = transform;
        }

        private void Start()
        {
            MoveAction.EnablePerformCancelEvent(PerformMove, CancelMove);
            SprintAction.EnableStartCancelEvent(StartSprint, CancelSprint);
            ZoomCameraAction.EnablePerformCancelEvent(PerformZoom, CancelZoom);
            RotationAction.EnableAllEvents(StartRotation, PerformRotation, CancelRotation);
        }

        private void OnDestroy()
        {
            MoveAction.DisablePerformCancelEvent(PerformMove, CancelMove);
            SprintAction.DisableStartCancelEvent(StartSprint, CancelSprint);
            ZoomCameraAction.DisablePerformCancelEvent(PerformZoom, CancelZoom);
            RotationAction.DisableAllEvents(StartRotation, PerformRotation, CancelRotation);
        }
        

        private void Update()
        {
            if (IsRotating) SetCameraRotation();

            if (MoveAxis != Vector2.zero) MoveCamera(CameraTransform.position, CameraTransform.forward, CameraTransform.right);

            if (Zoom != 0) CameraTransform.position = mad(up(), Zoom, transform.position);
        }

        private void MoveCamera(Vector3 cameraPosition, Vector3 cameraForward, Vector3 cameraRight)
        {
            //real forward of the camera (aware of the rotation)
            Vector3 cameraForwardXZ = new Vector3(cameraForward.x, 0, cameraForward.z);
            
            XAxisRotation = MoveAxis.x != 0 ? (MoveAxis.x > 0 ? -cameraRight : cameraRight) : Vector3.zero;
            ZAxisRotation = MoveAxis.y != 0 ? (MoveAxis.y > 0 ? cameraForwardXZ : -cameraForwardXZ) : Vector3.zero;
            
            CameraTransform.position += (XAxisRotation + ZAxisRotation) * (max(1f,cameraPosition.y) * MoveSpeed * Time.deltaTime);
        }
        
        private void SetCameraRotation()
        {
            if (MouseEndPosition != MouseStartPosition)
            {
                float distanceX = (MouseEndPosition - MouseStartPosition).x * cameraData.rotationSpeed;
                float distanceY = (MouseEndPosition - MouseStartPosition).y * cameraData.rotationSpeed;

                float deltaTime = Time.deltaTime;
                
                Quaternion worldRot = CameraTransform.rotation.RotateFWorld(0f, distanceX * deltaTime,0f);
                CameraTransform.rotation = worldRot;

                Quaternion selfRot = CameraTransform.localRotation.RotateFSelf(-distanceY * deltaTime, 0f, 0f);
                CameraTransform.localRotation =  selfRot;
                
                //CameraTransform.Rotate(0f, distanceX * deltaTime, 0f, Space.World);
                //CameraTransform.Rotate(-distanceY * deltaTime, 0f, 0f, Space.Self);

                MouseStartPosition = MouseEndPosition;
            }
        }

        //EVENTS CALLBACK
        //==============================================================================================================
        
        //Rotation
        //====================
        private void StartRotation(InputAction.CallbackContext ctx)
        {
            MouseStartPosition = ctx.ReadValue<Vector2>();
            IsRotating = true;
        }
        
        private void PerformRotation(InputAction.CallbackContext ctx) => MouseEndPosition = ctx.ReadValue<Vector2>();

        private void CancelRotation(InputAction.CallbackContext ctx) => IsRotating = false;
        
        //Sprint
        //====================
        private void StartSprint(InputAction.CallbackContext ctx) => IsSprinting = true;

        private void CancelSprint(InputAction.CallbackContext context) => IsSprinting = false;

        //Move
        //====================
        private void PerformMove(InputAction.CallbackContext ctx) => MoveAxis = ctx.ReadValue<Vector2>();
        private void CancelMove(InputAction.CallbackContext ctx) => MoveAxis = Vector2.zero;

        //Zoom
        //====================
        private void PerformZoom(InputAction.CallbackContext ctx) => Zoom = ctx.ReadValue<float>();
        private void CancelZoom(InputAction.CallbackContext ctx) => Zoom = 0;
    }
}
