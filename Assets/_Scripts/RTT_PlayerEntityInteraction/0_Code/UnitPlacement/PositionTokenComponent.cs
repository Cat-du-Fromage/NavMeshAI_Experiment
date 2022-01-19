using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    //Issue
    //Need to know Only the Destination
    //If not Set => token is on the unit
    public class PositionTokenComponent : MonoBehaviour
    {
        private bool Init = false;
        public bool IsDestinationSet { get; private set; } = false;
        public Transform UnitAttached { get; private set; }

        public void AttachToUnit(Transform unit) => UnitAttached = unit;

        public void SetDestination(bool enable) => IsDestinationSet = enable;

        /// <summary>
        /// trick: so UnitAttached has a value when OnEnable is launch when GameObject is created
        /// Note : the value is wrong during awake but reassign directly after
        /// </summary>
        private void Awake() => UnitAttached = null;

        private void OnEnable()
        {
            if (IsDestinationSet || !Init || transform.position == UnitAttached.position) return;
            transform.position = UnitAttached.position;
        }

        private void Start() => Init = true;
    }
}
