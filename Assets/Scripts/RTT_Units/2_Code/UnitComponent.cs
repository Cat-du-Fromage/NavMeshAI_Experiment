using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode
{
    public class UnitComponent : MonoBehaviour
    {
        [SerializeField] private string unitName;
        
        public string Name => unitName;

        public Transform Regiment { get; private set; }
        public void SetRegiment(Transform regiment) => Regiment = regiment;
    }
}
