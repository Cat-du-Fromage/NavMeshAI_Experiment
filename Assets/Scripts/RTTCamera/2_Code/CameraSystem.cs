using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTCamera
{
    public class CameraSystem : MonoBehaviour
    {
        //[Min(1)]
        //[SerializeField] private int rotationSpeed, baseMoveSpeed, zoomSpeed;

        [SerializeField]private CameraInputData cameraData;
        
        private Transform cameraTransform;
        private Controls controls;
        
        private bool canRotate = false;
        //private int sprint;
        private bool IsSprinting = false;
        private float zoom;
        private Vector2 mouseStartPosition, mouseEndPosition;
        private Vector2 moveAxis;
        
        //INPUT ACTIONS
        InputAction ZoomCameraAction => controls.CameraControl.Zoom;
        InputAction MoveAction => controls.CameraControl.Mouvement;
        InputAction RotationAction => controls.CameraControl.Rotation;
        InputAction SprintAction => controls.CameraControl.Faster;
        
        //UPDATED MOVE SPEED
        private int MoveSpeed => IsSprinting ? cameraData.baseMoveSpeed * cameraData.sprint : cameraData.baseMoveSpeed;
        
        private void Awake()
        {
            controls ??= new Controls();
            controls.Enable();
            
            cameraTransform = transform;
            //CameraData.rotationSpeed = max(1, CameraData.rotationSpeed);
            //baseMoveSpeed = max(1, baseMoveSpeed);
            //zoomSpeed = max(1, zoomSpeed);
            //sprint = max(1, sprint);
        }

        private void Start()
        {
            ZoomCameraAction.EnablePerformCancelEvent(ZoomCamera, StopZoomCamera);
            MoveAction.EnablePerformCancelEvent(MoveCamera, StopMoveCamera);
            RotationAction.EnablePerformCancelEvent(RotateCamera, StopRotateCamera);
            SprintAction.EnableStartCancelEvent(SprintCamera, StopSprintCamera);
        }

        private void OnDestroy()
        {
            ZoomCameraAction.DisablePerformCancelEvent(ZoomCamera, StopZoomCamera);
            MoveAction.DisablePerformCancelEvent(MoveCamera, StopMoveCamera);
            RotationAction.DisablePerformCancelEvent(RotateCamera, StopRotateCamera);
            SprintAction.EnableStartCancelEvent(SprintCamera, StopSprintCamera);
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
                float distanceX = (mouseEndPosition - mouseStartPosition).x * cameraData.rotationSpeed;
                float distanceY = (mouseEndPosition - mouseStartPosition).y * cameraData.rotationSpeed;
                
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
        private void SprintCamera(InputAction.CallbackContext ctx)
        {
            Debug.Log(ctx.started);
            IsSprinting = true;
        }

        private void StopSprintCamera(InputAction.CallbackContext context) => IsSprinting = false;
    }
}
