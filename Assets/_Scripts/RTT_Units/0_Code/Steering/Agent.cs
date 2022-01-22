using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
namespace KaizerWaldCode
{
    public class Agent : MonoBehaviour
    {
        private readonly Vector3 Vector3Y0 = new Vector3(1f, 0, 1f);
        
        public float MaxSpeed = 10.0f;
        public float TrueMaxSpeed;
        public float MaxAcceleration;

        public float Orientation; //rotation Y
        public float Rotation; // how much degree we rotate per frame
        public Vector3 Velocity = Vector3.zero; // position change per frame (vitesse)
        protected Steering Steering = new Steering();

        public float MaxRotation = 45.0f; //max rotation value per frame
        public float MaxAngularAccel = 45.0f; //max acceleration value per frame 
        
        private Vector3 GetDisplacement() => Vector3.Scale((Velocity * Time.deltaTime), Vector3Y0);
        private void UpdateOrientation() => Orientation += Rotation * Time.deltaTime;
        
        private void SetSteering(Steering steering, float weight)
        {
            Steering.Linear += weight * steering.Linear;
            Steering.Angular += weight * steering.Angular;
        }
        
        private void Awake()
        {
            TrueMaxSpeed = MaxSpeed;
        }

        private void Update()
        {
            Vector3 displacement = GetDisplacement();
            UpdateOrientation();
            
            //Limit orientation between 0 and 360
            Orientation = clamp(Orientation, 0.0f, 360.0f); //WATCH BEHACIOUR otherwise if <0 => orient += 360 elseif(>360) orient -=360
            transform.Translate(displacement, Space.World);
            //transform.rotation = new Quaternion();
            transform.Rotate(Vector3.up, Orientation);
        }

        private void LateUpdate()
        {
            Velocity += Steering.Linear * Time.deltaTime;
            Rotation += Steering.Angular * Time.deltaTime;

            Velocity = Velocity.normalized * MaxSpeed;
            Velocity = Steering.Linear.magnitude == 0 ? Vector3.zero : Velocity;
            Steering = new Steering();
        }
    }
}
