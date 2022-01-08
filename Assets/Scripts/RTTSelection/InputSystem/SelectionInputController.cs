//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.2.0
//     from Assets/Scripts/RTTSelection/InputSystem/SelectionInputController.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @SelectionInputController : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @SelectionInputController()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""SelectionInputController"",
    ""maps"": [
        {
            ""name"": ""MouseControl"",
            ""id"": ""5c89c9c1-7a35-4946-bd23-6da79cb4e509"",
            ""actions"": [
                {
                    ""name"": ""ShiftClick"",
                    ""type"": ""Button"",
                    ""id"": ""70060649-fd4d-48a4-a6e9-c3a6fa59eeb0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseLeftClick"",
                    ""type"": ""Value"",
                    ""id"": ""9de3c399-d817-416b-8f67-0b5b7e24276b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MouseMove"",
                    ""type"": ""Value"",
                    ""id"": ""5aed991a-fc19-4bce-b371-d274db86314f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""9b3ea6b9-aa5a-46b8-a9da-49d8b82fcf59"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShiftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""StartPosition"",
                    ""id"": ""57525637-0611-42d9-8e57-b7241cff9e29"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseLeftClick"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""48cbafae-8048-4de8-aed8-4959377bdaf8"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseLeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""1da43313-7fc2-4737-a286-08c22f8439b9"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseLeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""be75f95a-5d5f-4e46-ae71-09118c68563d"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MouseControl
        m_MouseControl = asset.FindActionMap("MouseControl", throwIfNotFound: true);
        m_MouseControl_ShiftClick = m_MouseControl.FindAction("ShiftClick", throwIfNotFound: true);
        m_MouseControl_MouseLeftClick = m_MouseControl.FindAction("MouseLeftClick", throwIfNotFound: true);
        m_MouseControl_MouseMove = m_MouseControl.FindAction("MouseMove", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // MouseControl
    private readonly InputActionMap m_MouseControl;
    private IMouseControlActions m_MouseControlActionsCallbackInterface;
    private readonly InputAction m_MouseControl_ShiftClick;
    private readonly InputAction m_MouseControl_MouseLeftClick;
    private readonly InputAction m_MouseControl_MouseMove;
    public struct MouseControlActions
    {
        private @SelectionInputController m_Wrapper;
        public MouseControlActions(@SelectionInputController wrapper) { m_Wrapper = wrapper; }
        public InputAction @ShiftClick => m_Wrapper.m_MouseControl_ShiftClick;
        public InputAction @MouseLeftClick => m_Wrapper.m_MouseControl_MouseLeftClick;
        public InputAction @MouseMove => m_Wrapper.m_MouseControl_MouseMove;
        public InputActionMap Get() { return m_Wrapper.m_MouseControl; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MouseControlActions set) { return set.Get(); }
        public void SetCallbacks(IMouseControlActions instance)
        {
            if (m_Wrapper.m_MouseControlActionsCallbackInterface != null)
            {
                @ShiftClick.started -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnShiftClick;
                @ShiftClick.performed -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnShiftClick;
                @ShiftClick.canceled -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnShiftClick;
                @MouseLeftClick.started -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.performed -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.canceled -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnMouseLeftClick;
                @MouseMove.started -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnMouseMove;
                @MouseMove.performed -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnMouseMove;
                @MouseMove.canceled -= m_Wrapper.m_MouseControlActionsCallbackInterface.OnMouseMove;
            }
            m_Wrapper.m_MouseControlActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ShiftClick.started += instance.OnShiftClick;
                @ShiftClick.performed += instance.OnShiftClick;
                @ShiftClick.canceled += instance.OnShiftClick;
                @MouseLeftClick.started += instance.OnMouseLeftClick;
                @MouseLeftClick.performed += instance.OnMouseLeftClick;
                @MouseLeftClick.canceled += instance.OnMouseLeftClick;
                @MouseMove.started += instance.OnMouseMove;
                @MouseMove.performed += instance.OnMouseMove;
                @MouseMove.canceled += instance.OnMouseMove;
            }
        }
    }
    public MouseControlActions @MouseControl => new MouseControlActions(this);
    public interface IMouseControlActions
    {
        void OnShiftClick(InputAction.CallbackContext context);
        void OnMouseLeftClick(InputAction.CallbackContext context);
        void OnMouseMove(InputAction.CallbackContext context);
    }
}
