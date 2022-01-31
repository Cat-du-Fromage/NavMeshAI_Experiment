using System;
using KaizerWaldCode.FlowField;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static KaizerWaldCode.FlowField.FlowField;

namespace KaizerWaldCode.RTTUnits
{
    public class Leader : MonoBehaviour
    {
        //DEBUG PURPOSE
        Transform leaderTransform;
        private GridData gdata;
        
        private Regiment AttachRegiment;
        
        private Vector3 StartDestination = Vector3.zero;
        private Vector3 EndDestination = Vector3.zero;

        private Vector3[] FormationSlots;

        private Vector3[] FlowField;
        private GameObject[] FormationSlotsGhost;

        //For Debug Purpose
        private Vector3 DebugCenterRegiment;
        private void Awake()
        {
            leaderTransform = transform;
            gameObject.SetActive(false);
        }

        public void AttachTo(Regiment regiment)
        {
            AttachRegiment = regiment;
            FormationSlotsGhost = new GameObject[regiment.CurrentSize];
            FillFormationSlots();
        }

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
            
            //Get radius of the circumcenter of the formation
            int numUnitInLastRow = GetNumUnitInLastRow(AttachRegiment.CurrentRowFormation);
            
            Vector3 diameterFormation = numUnitInLastRow == 0
                ? (AttachRegiment.Units[^1].transform.position + AttachRegiment.Units[0].transform.position)
                : (AttachRegiment.Units[^numUnitInLastRow].transform.position + AttachRegiment.Units[0].transform.position);
            
            Vector3 regimentCenter = diameterFormation/2;
            DebugCenterRegiment = regimentCenter;
            
            float radiusFormation = (regimentCenter - AttachRegiment.Units[0].transform.position).magnitude * 2;
            
            //radiusFormation /= 2;
            
            //PickPosition of the unit in the middle of the first Row (even of odd is not important)
            Vector3 regimentCurrentPosition = AttachRegiment.Units[middleRowIndex].transform.position;
            Vector3 startToEndDirection = (EndDestination - regimentCurrentPosition).normalized;


            //Take middle width of the first row (draw a circle around regiment)
            float offsetDistanceLeaderRegiment = (AttachRegiment.GetUnitType.unitWidth + AttachRegiment.GetRegimentType.offsetInRow) 
                                                * radiusFormation;

            Vector3 rePositionAt = regimentCenter + (startToEndDirection * offsetDistanceLeaderRegiment);
            
            Debug.DrawLine(regimentCenter, rePositionAt, Color.magenta, 10f);
            
            return rePositionAt;
        }

        private Vector3 GetSlotStartFrom(int numUnitInRow, float unitSize, float regimentOffsetRow, int offset = 1, int from = 0)
        {
            offset = Mathf.Max(1, offset);
            float unitOffset = (numUnitInRow / 2f) * unitSize;
            float rowOffset = (numUnitInRow - 1) / 2f * regimentOffsetRow;
            float dstOffset = unitOffset + rowOffset;

            Vector3 startRow = (Vector3.left * dstOffset);
            startRow += Vector3.back * (unitSize * offset);
            //Vector3 startRow = StartDestination + (-leaderTransform.right * dstOffset);
            //startRow += -leaderTransform.forward * (unitSize * offset);

            return startRow;
        }

        public int GetNumUnitInLastRow(int rowFormation)
        {
            int numUnitInFullRow = (Mathf.FloorToInt(AttachRegiment.CurrentSize/(float)rowFormation) * rowFormation);
            int unitInLastRow = AttachRegiment.CurrentSize - numUnitInFullRow;
            return unitInLastRow;
        }

        public void FillFormationSlots()
        {
            int rowFormation = AttachRegiment.GetCurrentRowFormation();
            float unitSize = AttachRegiment.GetUnitType.unitWidth;
            float regimentOffsetRow = AttachRegiment.GetRegimentType.offsetInRow;
            int unitsCount = AttachRegiment.CurrentSize;
            
            Vector3 startRow = GetSlotStartFrom(rowFormation, unitSize, regimentOffsetRow);
            GameObject slot = new GameObject();
            
            //Last Row Data
            int unitInLastRow = GetNumUnitInLastRow(rowFormation);
            int lastRowIndex = Mathf.FloorToInt(unitsCount / (float)rowFormation) - 1;
            Vector3 lastRowStart = GetSlotStartFrom(unitInLastRow, unitSize, regimentOffsetRow, lastRowIndex-1);
            
            for (int i = 0; i < FormationSlotsGhost.Length; i++)
            {
                int row = Mathf.FloorToInt(i / (float)rowFormation);
                int index = i - (row * rowFormation);

                Vector3 positionInRow = row > lastRowIndex ? lastRowStart : startRow;
                
                positionInRow += (index * (unitSize + regimentOffsetRow) * leaderTransform.right);
                positionInRow += (row * -leaderTransform.forward);
                
                FormationSlotsGhost[i] = Instantiate(slot, positionInRow, leaderTransform.rotation, leaderTransform);
                FormationSlotsGhost[i].name = $"slot {i}";
            }
            DestroyImmediate(slot);
        }

        public void UpdateFormation()
        {
            if (FormationSlotsGhost[0] is null) return;
            Debug.Log($"{FormationSlotsGhost[0]}");
            //Last Row Data
            int unitsCount = AttachRegiment.CurrentSize;
            float unitSize = AttachRegiment.GetUnitType.unitWidth;
            float regimentOffsetRow = AttachRegiment.GetRegimentType.offsetInRow;
            
            int rowFormation = AttachRegiment.CurrentRowFormation;
            
            int unitInLastRow = GetNumUnitInLastRow(rowFormation);
            int lastRowIndex = Mathf.FloorToInt(unitsCount / (float)rowFormation) - 1;
            
            Vector3 startRow = GetSlotStartFrom(rowFormation, unitSize, regimentOffsetRow);
            Vector3 lastRowStart = GetSlotStartFrom(unitInLastRow, unitSize, regimentOffsetRow, lastRowIndex-1);

            for (int i = 0; i < FormationSlotsGhost.Length; i++)
            {
                int row = Mathf.FloorToInt(i / (float)rowFormation);
                int index = i - (row * rowFormation);

                Vector3 positionInRow = row > lastRowIndex ? lastRowStart : startRow;

                //Vector3 dir = Vector3.Cross(EndDestination-StartDestination, Vector3.up).normalized;
                
                positionInRow += (index * (unitSize + regimentOffsetRow) * Vector3.right);
                positionInRow += (row * Vector3.back);

                FormationSlotsGhost[i].transform.localPosition = positionInRow;
                //FormationSlotsGhost[i].transform.localRotation = leaderTransform.rotation;
            }
            
        }
        

        public void OnDestinationSet(in GridData gridData)
        {
            gdata = gridData;
            
            if (gameObject.activeSelf == false)
            {
                gameObject.SetActive(true);
            }
            
            
            //Get MiddlePosition of the new destination
            EndDestination = GetEndDestination();
            StartDestination = GetStartDestination();
            
            Quaternion targetRotation = Quaternion.LookRotation(EndDestination - transform.position);
            leaderTransform.SetPositionAndRotation(StartDestination, targetRotation);
            //transform.rotation = Quaternion.LookRotation(EndDestination);
            
            FlowField = GetFlowField(EndDestination, gridData);
            
            //Fill formation array
            UpdateFormation();
        }
#if UNITY_EDITOR
        
        private void OnDrawGizmos()
        {
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(EndDestination, 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(StartDestination, 0.5f);
            Gizmos.DrawRay(StartDestination, EndDestination-StartDestination);
            
            Gizmos.DrawSphere(DebugCenterRegiment, 1);

            if (FormationSlotsGhost[0] == null) return;
            for (int i = 0; i < FormationSlotsGhost.Length; i++)
            {
                if (i == 0)
                {
                    Gizmos.color = Color.magenta;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }
                Gizmos.DrawSphere(FormationSlotsGhost[i].transform.position, 0.5f);
            }
            
        }
        
        public void DrawIcons(int choice = 0)
        {
            if (FlowField == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            Gizmos.color = Color.green;
            Vector3 center;
            for (int i = 0; i < gdata.TotalCells; i++)
            {
                center = DebugPosition[i];

                if (choice == 0)
                {
                    center -= (Vector3.up * 0.5f);
                    Vector3 dir = FlowField[i];
                    if (dir == Vector3.zero) continue;
                    KWUtils.Debug.DrawArrow.ForGizmo(center, dir/2f);
                }
                else if (choice == 1)
                {
                    center += (Vector3.up * 0.5f);
                
                    string text = DebugBestCost[i] >= ushort.MaxValue
                        ? "Max"
                        : DebugBestCost[i].ToString();
                    if (DebugBestCost[i] >= ushort.MaxValue) continue;
                
                    Handles.Label(center, DebugBestCost[i].ToString(), style);
                }
                else if (choice == 2)
                {
                    center += (Vector3.up * 0.5f);
                    Handles.Label(center, DebugCellCost[i].ToString(), style);
                }
            }
        }
#endif
    }
}