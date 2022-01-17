using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    public class DestinationTokenComponent : MonoBehaviour
    {
        public Transform UnitAttached { get; private set; }

        public void AttachToUnit(Transform unit) => UnitAttached = unit;
    }
}