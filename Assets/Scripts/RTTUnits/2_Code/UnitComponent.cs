using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode
{
    public class UnitComponent : MonoBehaviour
    {
        [SerializeField] private string name;
        
        public string Name => name;

        public Transform Regiment { get; private set; }
        public int RegimentId { get; private set; }

        public void SetRegiment(Transform regiment) => Regiment = regiment;
    }
}
