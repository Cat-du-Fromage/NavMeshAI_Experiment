using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KWUtils
{
    public static class InputSystem
    {
        //STARTED EVENT
        public static void EnableStartEvent(this InputAction inputAction, Action<InputAction.CallbackContext> start) => inputAction.started += start;
        public static void DisableStartEvent(this InputAction inputAction, Action<InputAction.CallbackContext> start) => inputAction.started -= start;
        //PERFORMED EVENT
        public static void EnablePerformEvent(this InputAction inputAction, Action<InputAction.CallbackContext> perform) => inputAction.performed += perform;
        public static void DisablePerformEvent(this InputAction inputAction, Action<InputAction.CallbackContext> perform) => inputAction.performed -= perform;
        //CANCELED EVENT
        public static void EnableCancelEvent(this InputAction inputAction, Action<InputAction.CallbackContext> cancel) => inputAction.canceled += cancel;
        public static void DisableCancelEvent(this InputAction inputAction, Action<InputAction.CallbackContext> cancel) => inputAction.canceled -= cancel;

        //STARTED-PERFORMED-CANCELED
        public static void EnableAllEvents(this InputAction inputAction,
            Action<InputAction.CallbackContext> start, 
            Action<InputAction.CallbackContext> performed, 
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.started += start;
            inputAction.performed += performed;
            inputAction.canceled += cancel;
        }
        
        public static void DisableAllEvents(this InputAction inputAction,
            Action<InputAction.CallbackContext> start, 
            Action<InputAction.CallbackContext> performed, 
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.started -= start;
            inputAction.performed -= performed;
            inputAction.canceled -= cancel;
        }
    }
}
