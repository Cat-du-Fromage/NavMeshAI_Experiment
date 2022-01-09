using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWaldCode.PlayerEntityInteractions
{
    public class PlayerEntityInteractionInputsManager : MonoBehaviour
    {
        private SelectionInputController Control;
        private SelectionInputController.MouseControlActions MouseCtrl;
        private InputAction SelectionEvents;
        
        //Selection Datas

        public bool ShiftPressed = false;
        public bool LeftClick = false;
        
        private Vector2 StartMouseClick = Vector2.zero;
        private Vector2 EndMouseClick = Vector2.zero;

        private void OnEnable() => Control.Enable();
        private void OnDisable() => Control.Disable();
        
        private void Awake()
        {
            Control ??= new SelectionInputController();
            MouseCtrl = Control.MouseControl;
            SelectionEvents = Control.MouseControl.SelectionMouseLeftClick;
            
            Control.MouseControl.ShiftClick.EnableStartCancelEvent(OnStartShift, OnCancelShift);
            SelectionEvents.EnableAllEvents(OnStartMouseClick, OnPerformMoveMouse, OnCancelMouseClick);
        }

        
        private void OnStartShift(InputAction.CallbackContext ctx) => ShiftPressed = true;
        private void OnCancelShift(InputAction.CallbackContext ctx) => ShiftPressed = false;

        private void OnStartMouseClick(InputAction.CallbackContext ctx)
        {
            StartMouseClick = ctx.ReadValue<Vector2>();
            LeftClick = true;
        }
        
        private void OnPerformMoveMouse(InputAction.CallbackContext ctx)
        {
            EndMouseClick = ctx.ReadValue<Vector2>();
        }
        
        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            LeftClick = false;
        }
        
    }
}
