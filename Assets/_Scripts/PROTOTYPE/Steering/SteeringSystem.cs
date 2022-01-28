using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.InputSystem;

using KaizerWaldCode.Globals;
using KaizerWaldCode.Grid;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;

using static KaizerWaldCode.Globals.StaticDatas;
using static KWUtils.KWmath;
using static KWUtils.KWmesh;
using static KWUtils.KWRect;
using static KaizerWaldCode.PlayerEntityInteractions.RTTSelection.SelectionMeshUtils;
using FlowField = KaizerWaldCode.Grid.FlowField;
namespace KaizerWaldCode
{
    public class SteeringSystem : KWUtils.Singleton<SteeringSystem>
    {
        [SerializeField] private MoveUpdateManager updateManager;
        [SerializeField] private GridSettings gridSettings;
        
        [SerializeField] private GameObject leaderPrefab;
        
        [SerializeField] private GameObject prefabTargetStart;
        [SerializeField] private GameObject prefabTargetEnd;
        
        public RegimentManager regimentManager;
        public bool IsActive = false;

        public Vector3 startMouse; //Will corespond to : regiment's middle current row formation
        public Vector3 EndMouse; //Will corespond to : regiment's middle current row formation destiunations

        private Grid.FlowField flowField;

        private Dictionary<GameObject, Grid.FlowField> LeaderFlowField = new Dictionary<GameObject, Grid.FlowField>();

        protected override void Awake()
        {
            base.Awake();
            prefabTargetStart.SetActive(false);
            prefabTargetEnd.SetActive(false);
            startMouse = leaderPrefab.transform.position;
            regimentManager = FindObjectOfType<RegimentManager>();
            gridSettings ??= FindObjectOfType<GridSettings>();
            updateManager ??= FindObjectOfType<MoveUpdateManager>();
        }

        public void Start()
        {
            flowField = new Grid.FlowField(gridSettings);
            gridSettings.FlowField = flowField;
        }

        private void Update()
        {
            if (Mouse.current.press.wasPressedThisFrame)
            {
                SimulateSetNewDestination();
                flowField.InitGrid(prefabTargetEnd.transform.position, gridSettings);
                gridSettings.FlowField = flowField;
                updateManager.AddObjectToMove(leaderPrefab, flowField);
            }
        }

        //Simulate when we set destination by : Release Right Click
        public void SimulateSetNewDestination()
        {
            SetStartToken();
                
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Raycast(ray, out RaycastHit hit, INFINITY, StaticDatas.TerrainLayer))
            {
                EndMouse = hit.point; //NEW DESTINATION
            }

            if (EndMouse == startMouse) return;
            SetEndToken();
        }

        public void SetStartToken()
        {
            prefabTargetStart.SetActive(true);
            prefabTargetStart.transform.position = leaderPrefab.transform.position;
        }
        
        public void SetEndToken()
        {
            prefabTargetEnd.SetActive(true);
            prefabTargetEnd.transform.position = EndMouse;
        }
    }
}
