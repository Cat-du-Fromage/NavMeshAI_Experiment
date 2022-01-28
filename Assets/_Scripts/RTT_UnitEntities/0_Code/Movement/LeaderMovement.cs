using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class LeaderMovement : MonoBehaviour
    {
        private HashSet<GameObject> Leaders;

        public void OnLeaderAssigned(Regiment[] regiments)
        {
            //Init capacity
            Leaders = new HashSet<GameObject>(regiments.Length);
            
            
        }
    }
}