using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PlayerEntityInteractions;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class UnitComponent : MonoBehaviour
    {
        [SerializeField] private string unitName;
        
        public string Name => unitName;

        public Regiment Regiment { get; private set; }
        public void SetRegiment(Regiment regiment) => Regiment = regiment;
    }
}
