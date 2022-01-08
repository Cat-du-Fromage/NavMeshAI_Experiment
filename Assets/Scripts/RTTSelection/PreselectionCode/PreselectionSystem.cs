using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

using static UnityEngine.Physics;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTSelection
{
    [RequireComponent(typeof(PreselectionRegister))]
    public class PreselectionSystem : MonoBehaviour
    {
        public PreselectionRegister Register;
        public Camera PlayerCamera;

        private SelectionInputController Control;
        private SelectionInputController.MouseControlActions MouseCtrl;
        
        //CONSTANT
        private readonly LayerMask UnitLayer = 1 << 9;
        
        //SELECTION CACHE
        private Transform CachedUnitPreselection;
        private Transform CurrentPreselection;
        private RaycastHit hit;
        private Ray ray;

        private bool PreselectOn;
        
        private void OnEnable() => Control.Enable();
        private void OnDisable() => Control.Disable();
        
        private void Awake()
        {
            PlayerCamera = Camera.main;
            Register = GetComponent<PreselectionRegister>();
            
            Control ??= new SelectionInputController();
            MouseCtrl = Control.MouseControl;

            MouseCtrl.PreselectMouseMove.EnablePerformEvent(OnMouseMove);
        }

        private void OnDestroy() => MouseCtrl.PreselectMouseMove.DisablePerformEvent(OnMouseMove);

        private void OnMouseMove(InputAction.CallbackContext ctx)
        {
            ray = PlayerCamera.ScreenPointToRay(ctx.ReadValue<Vector2>());
            
            bool hitUnit = Raycast(ray, out hit, INFINITY, UnitLayer);
            CachedUnitPreselection = hitUnit ? hit.transform : CachedUnitPreselection;
            
            if (hitUnit && CachedUnitPreselection.TryGetComponent(out SelectionComponent comp))
            {
                if (comp.IsPreselected) return;
                
                if (PreselectOn && CachedUnitPreselection.parent != CurrentPreselection)
                {
                    Register.Clear();
                }
                
                CurrentPreselection = CachedUnitPreselection.parent;
                Register.Add(CurrentPreselection);
                    
                PreselectOn = true;
            }
            else if (!hitUnit && PreselectOn)
            {
                Register.Clear();
                PreselectOn = false;
            }
        }
    }
}
