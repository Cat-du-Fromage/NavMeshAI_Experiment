using System;
using UnityEngine;

namespace KaizerWaldCode.RTTUnits
{
    public class Leader : MonoBehaviour
    {
        private Regiment AttachRegiment;
        
        private Vector3 StartDestination = Vector3.zero;
        private Vector3 EndDestination = Vector3.zero;

        private Vector3[] FormationSlots;
        public void AttachTo(Regiment regiment) => AttachRegiment = regiment;

        private void Awake() => gameObject.SetActive(false);

        private Vector3 GetEndDestination()
        {
            int lastRowUnitIndex = AttachRegiment.GetCurrentRowFormation() - 1;

            Vector3 startPosition = AttachRegiment.DestinationTokens[0].transform.position;
            
            Vector3 startEndDifference = (AttachRegiment.DestinationTokens[lastRowUnitIndex].transform.position - startPosition);
            float rowLength = startEndDifference.magnitude;
            Vector3 direction = startEndDifference.normalized;
            
            return startPosition + (direction * (rowLength / 2));
        }

        private Vector3 GetStartDestination()
        {
            int middleRowIndex = AttachRegiment.PreviousRowFormation / 2;
            //PickPosition of the unit in the middle of the first Row (even of odd is not important)
            Vector3 regimentCurrentPosition = AttachRegiment.Units[middleRowIndex].transform.position;
            Vector3 startToEndDirection = (EndDestination - regimentCurrentPosition).normalized;
            
            //Take middle width of the first row (draw a circle around regiment)
            float offsetDistanceLeaderRegiment = (AttachRegiment.GetUnitType.unitWidth + AttachRegiment.GetRegimentType.offsetInRow) 
                                                * middleRowIndex;

            Vector3 rePositionAt = regimentCurrentPosition + (startToEndDirection * offsetDistanceLeaderRegiment);
            
            return rePositionAt;
        }

        public void OnDestinationSet()
        {
            if (gameObject.activeSelf == false)
            {
                gameObject.SetActive(true);
            }
            
            
            //Get MiddlePosition of the new destination
            EndDestination = GetEndDestination();
            StartDestination = GetStartDestination();

            transform.position = StartDestination;
        }

        /*
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(EndDestination, 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(StartDestination, 0.5f);
            Gizmos.DrawRay(StartDestination, EndDestination-StartDestination);
        }
        */
    }
}