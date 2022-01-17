using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    //Issue
    //Need to know Only the Destination
    //If not Set => token is on the unit
    public class PositionTokenComponent : MonoBehaviour
    {
        //private bool Initialized = false;
        public bool IsDestinationSet { get; private set; } = false;
        public Transform UnitAttached { get; private set; }

        public void AttachToUnit(Transform unit) => UnitAttached = unit;

        public void SetDestination(bool enable) => IsDestinationSet = enable;

        /// <summary>
        /// trick: so UnitAttached has a value when OnEnable is launch when GameObject is created
        /// Note : the value is wrong during awake but reassign directly after
        /// </summary>
        private void Awake() => UnitAttached = FindObjectOfType<DestinationTokenComponent>().UnitAttached;

        private void OnEnable()
        {
            if (IsDestinationSet || transform.position == UnitAttached?.position) return;
            transform.position = UnitAttached.position;
        }
    }
}
