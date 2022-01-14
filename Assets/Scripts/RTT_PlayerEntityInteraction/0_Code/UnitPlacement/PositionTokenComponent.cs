using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    public class PositionTokenComponent : MonoBehaviour
    {
        public Transform UnitAttached { get; private set; }

        public void AttachToUnit(Transform unit) => UnitAttached = unit;
    }
}
