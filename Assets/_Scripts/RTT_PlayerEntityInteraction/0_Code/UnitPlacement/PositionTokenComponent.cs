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
        public bool IsDestinationSet { get; private set; } = false;
        public Transform UnitAttached { get; private set; }

        public void AttachToUnit(Transform unit) => UnitAttached = unit;

        public void SetDestination(bool enable) => IsDestinationSet = enable;

        private void OnEnable()
        {
            if (IsDestinationSet || transform.position == UnitAttached.position) return;
            transform.position = UnitAttached.position;
        }
    }
}
