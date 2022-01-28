using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    [CreateAssetMenu(fileName = "NewCategory", menuName = "Regiment", order = 0)]
    public class RegimentType : ScriptableObject
    {
        public int baseNumUnits = 20;
        public int minRow = 4;
        public int maxRow = 10;
        public float offsetInRow = 0.5f;
    }
}