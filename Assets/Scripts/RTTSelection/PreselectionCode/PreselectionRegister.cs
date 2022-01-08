using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode.RTTSelection
{
    public class PreselectionRegister : MonoBehaviour
    {
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
