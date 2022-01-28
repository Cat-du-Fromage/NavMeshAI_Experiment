using System;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    [CreateAssetMenu(fileName = "NewCategory", menuName = "Unit", order = 0)]
    public class UnitType : ScriptableObject
    {
        public GameObject unitPrefab;
        public GameObject positionTokenPrefab;
        public float unitWidth;
        public float unitHeight;
        private void OnEnable()
        {
            unitWidth = positionTokenPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
            unitHeight = positionTokenPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
        }
    }
}