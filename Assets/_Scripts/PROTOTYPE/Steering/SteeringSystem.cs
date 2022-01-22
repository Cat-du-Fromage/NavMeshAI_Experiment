using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

using KWUtils;

namespace KaizerWaldCode
{
    public class SteeringSystem : Singleton<SteeringSystem>
    {
        public RegimentManager regimentManager;
        private bool active = false;
        protected override void Awake()
        {
            base.Awake();
            regimentManager = FindObjectOfType<RegimentManager>();
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (!active && regimentManager.IsSelections)
            {
                active = false;
            }
            else if(active && !regimentManager.IsSelections)
            {
                active = true;
            }
            else
            {
                return;
            }
        }
    }
}
