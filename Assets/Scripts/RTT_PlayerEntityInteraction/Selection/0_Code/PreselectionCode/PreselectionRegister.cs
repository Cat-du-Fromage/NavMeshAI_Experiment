using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{
    public class PreselectionRegister : MonoBehaviour
    {
        public List<Transform> Preselections { get; } = new List<Transform>();
        
        private Transform CurrentPreselection;
        //REGIMENT SECTION

        //PRESELECTION
        public void Add(Transform regiment)
        {
            CurrentPreselection = regiment;
            CurrentPreselection.GetComponent<RegimentComponent>().SetPreselected(true);
        }
        
        public void Clear()
        {
            if(CurrentPreselection != null)
                CurrentPreselection.GetComponent<RegimentComponent>().SetPreselected(false);
            CurrentPreselection = null;
        }
    }
}
