using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public class PlayerEntityInteractionInputsManager : MonoBehaviour
    {
        private SelectionInputController Control;
        public SelectionInputController.MouseControlActions MouseCtrl { get; private set; }
        public InputAction SelectionEvents { get; private set; }
        
        //Selection Datas

        public bool ShiftPressed{ get; private set; }
        public bool LeftClick{ get; private set; }
        public bool IsDragging{ get; private set; }

        public Vector2 StartMouseClick{ get; private set; }
        
        public readonly Vector2[] EndMouseClick = new Vector2[2];

        private void OnEnable() => Control.Enable();
        private void OnDisable() => Control.Disable();
        
        private void Awake()
        {
            Control ??= new SelectionInputController();
            MouseCtrl = Control.MouseControl;
            SelectionEvents = Control.MouseControl.SelectionMouseLeftClick;
            
            Control.MouseControl.ShiftClick.EnableStartCancelEvent(OnStartShift, OnCancelShift);
            SelectionEvents.EnableAllEvents(OnStartMouseClick, OnPerformLeftClickMoveMouse, OnCancelMouseClick);
        }

        private void OnDestroy()
        {
            Control.MouseControl.ShiftClick.DisableStartCancelEvent(OnStartShift, OnCancelShift);
            SelectionEvents.DisableAllEvents(OnStartMouseClick, OnPerformLeftClickMoveMouse, OnCancelMouseClick);
        }

        private void OnStartShift(InputAction.CallbackContext ctx) => ShiftPressed = true;
        private void OnCancelShift(InputAction.CallbackContext ctx) => ShiftPressed = false;
        
        //LEFT CLICK + MOUSE MOVE
        //==============================================================================================================
        private void OnStartMouseClick(InputAction.CallbackContext ctx)
        {
            StartMouseClick = ctx.ReadValue<Vector2>();
            LeftClick = true;
        }
        
        private void OnPerformLeftClickMoveMouse(InputAction.CallbackContext ctx)
        {
            if(EndMouseClick[0] != ctx.ReadValue<Vector2>()) //this way we can compare arr[0] and arr[1] in other systems
            {
                EndMouseClick[0] = ctx.ReadValue<Vector2>(); //swap : new current [0]
                (EndMouseClick[0], EndMouseClick[1]) = (EndMouseClick[1], EndMouseClick[0]); //swap : current become previous 
                
            }
            IsDragging = (EndMouseClick[1] - StartMouseClick).sqrMagnitude > 200;
        }
        
        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            LeftClick = false;
            IsDragging = false;
        }

    }
}
