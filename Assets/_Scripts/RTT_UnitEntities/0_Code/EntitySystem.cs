using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode
{
    public class EntitySystem : MonoBehaviour
    {
        [SerializeField] private RegimentManager regimentManager;
        
        private HashSet<Regiment> RegimentsToMove = new HashSet<Regiment>();

        private HashSet<Leader> Leaders = new HashSet<Leader>();

        private void Awake()
        {
            regimentManager ??= FindObjectOfType<RegimentManager>();
            regimentManager.EntitySystem = this;
        }

        public void OnDestinationsSet(Regiment[] regimentsToMove)
        {
            foreach (Regiment regiment in regimentsToMove)
            {
                RegimentsToMove.Add(regiment);
                Debug.Log($"{regiment.Index}");
            }
        }
    }
}
