using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

using static UnityEngine.Physics;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
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
        private Regiment CachedRegiment;
        private Transform CachedUnitPreselection;
        private Regiment CurrentPreselection;
        private bool PreselectOn;
        
        //RAYCAST 
        private RaycastHit hit;
        private Ray RayCam(Vector2 mousPos) => PlayerCamera.ScreenPointToRay(mousPos);
        
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
            bool hitUnit = Raycast(RayCam(ctx.ReadValue<Vector2>()), out hit, INFINITY, UnitLayer);
            
            CachedUnitPreselection = hitUnit ? hit.transform : CachedUnitPreselection;
            
            if (hitUnit && CachedUnitPreselection.TryGetComponent(out SelectionComponent comp))
            {
                if (comp.IsPreselected) return;
                if (PreselectOn && CachedRegiment != CurrentPreselection)
                {
                    Register.Clear();
                }
                
                //CurrentPreselection = CachedUnitPreselection.parent;
                CurrentPreselection = CachedRegiment;
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