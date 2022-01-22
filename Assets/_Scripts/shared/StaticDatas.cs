using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.Globals
{
    public static class StaticDatas
    {
        public static readonly LayerMask TerrainLayer = 1 << 8;
        public static readonly LayerMask UnitLayer = 1 << 9;
        public static readonly LayerMask ObstacleLayer = 1 << 10;
    }
}
