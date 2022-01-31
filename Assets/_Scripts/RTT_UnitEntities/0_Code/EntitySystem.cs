using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using Unity.VisualScripting;
using UnityEngine;

namespace KaizerWaldCode
{
    public class EntitySystem : MonoBehaviour, IEntitySystem<Regiment>
    {
        [SerializeField] private RegimentsRegister regimentsRegister;
        [SerializeField] private LeaderSubSystem leaderSubSystem;
        [SerializeField] private UnitSubSystem unitSubSystem;
        private void Awake()
        {
            regimentsRegister ??= FindObjectOfType<RegimentsRegister>();
            leaderSubSystem ??= GetComponent<LeaderSubSystem>();
            unitSubSystem ??= GetComponent<UnitSubSystem>();
            
            (leaderSubSystem as IEntitySubSystem<Regiment>).MainSystem = this;
            (unitSubSystem as IEntitySubSystem<Regiment>).MainSystem = this;
        }
        
        private void Start()
        {
            OnGameStart();
            regimentsRegister.OnMovingRegimentsChanged += DispatchToLeaderSubSystem;
        }

        private void OnGameStart()
        {
            if (!TryGetComponent(out RegimentFactory factory)) return;
            regimentsRegister.Regiments = factory.CreateRegiments();
            Destroy(factory);
        }

        //DISPATCH REGISTER INFORMATION TO SUBSYSTEMS
        //==============================================================================================================
        private void DispatchToLeaderSubSystem(Regiment regimentToMove)
        {
            leaderSubSystem.AddRegimentToMove(regimentToMove);
            unitSubSystem.AddRegimentToMove(regimentToMove);
        }
        
        //OnDestinationSet
        //Get destination's middle row + 1 in forward direction
        //if leader not active => Get current Position MiddleRow + 1 in forward direction
        //else => take leader currentPose
        
        //NOTIFICATION FROM SUBSYSTEM
        //==============================================================================================================
        public void Notify(IEntitySubSystem<Regiment> subSystem, Regiment entity)
        {
            if (subSystem is LeaderSubSystem)
            {
                unitSubSystem.RemoveRegimentToMove(entity);
            }
            
        }
    }
}
