using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class UnitMouvement : MonoBehaviour
    {
        public List<Transform> targets;
        private List<Unit> units;
        //private Queue<Unit> UnitsToUpdate;

        private void Awake()
        {
            units = new List<Unit>(100);
            targets = new List<Transform>(100);
        }

        public void SetUnitToUpdate(Regiment regiment)
        {
            for (int i = 0; i < regiment.Units.Count; i++)
            {
                if (!units.Contains(regiment.Units[i]))
                {
                    units.Add(regiment.Units[i]);
                    targets.Add(regiment.NestedPositionTokens[i]);
                }
                    
            }
        }

        private void Update()
        {
            //Issue unit wont be at destination at the end of the loop
            //condition to stay in queue?
            if (units.Count == 0) return;
            Debug.Log($"Update Start");
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].transform.position == targets[i].position)
                {
                    units.Remove(units[i]);
                    targets.Remove(targets[i]);
                    continue;
                }
                units[i].transform.position = Vector3.MoveTowards(units[i].transform.position, targets[i].position, 10 * Time.deltaTime);
                //UnitsToUpdate.Dequeue();
                
                //Debug.Log($"Update {UnitsToUpdate.Count}");
            }
        }

/*
        private void Move(Vector2 inputMove)
        {
            Vector3 inputDirection = new Vector3(inputMove.x, 0.0f, inputMove.y).normalized;
        
            TargetRotation.Value = degrees(Mathf.Atan2(inputDirection.x, inputDirection.z)) + CameraRotation.Value;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation.Value, ref RotationVelocity, rotationSmoothTime);
        
            // rotate to face input direction relative to camera position
            Vector3 targetDirection = Quaternion.Euler(0.0f, TargetRotation.Value, 0.0f) * Vector3.forward;
        
            Quaternion rotInput = Quaternion.Euler(0.0f, rotation, 0.0f);
            Vector3 posInput = transform.position + targetDirection.normalized * (TargetSpeed.Value * Time.deltaTime);
        
            transform.SetPositionAndRotation(posInput, rotInput);
        }
        */
    }
}
