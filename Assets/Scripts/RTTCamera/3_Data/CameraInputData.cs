using System;
using UnityEngine;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTCamera
{
    [CreateAssetMenu(fileName = "CameraData", menuName = "RTTCamera", order = 0)]
    public class CameraInputData : ScriptableObject
    {
        [Min(1)]
        public int rotationSpeed; 
        [Min(1)]
        public int baseMoveSpeed; 
        [Min(1)]
        public int zoomSpeed;
        [Min(1)]
        public int sprint;

        private void Awake()
        {
            rotationSpeed = max(1,rotationSpeed);
            baseMoveSpeed = max(1, baseMoveSpeed);
            zoomSpeed = max(1, zoomSpeed);
            sprint = max(baseMoveSpeed, sprint);
        }
    }
}