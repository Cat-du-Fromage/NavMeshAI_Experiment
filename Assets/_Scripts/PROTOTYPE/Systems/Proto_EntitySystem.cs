using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KaizerWaldCode.RTTUnits;

namespace KaizerWaldCode
{
    [Serializable]
    public class Proto_RegimentsRegister
    {
        public List<Regiment> Regiments = new List<Regiment>(1);
        public HashSet<Regiment> SelectedRegiments = new HashSet<Regiment>(1);
        public HashSet<Regiment> MovingRegiments = new HashSet<Regiment>(1);
    }
    
    public class Proto_EntitySystem : MonoBehaviour
    {
        [SerializeField] private RegimentManager RegimentManager;
        
        [SerializeField] private Porto_InteractionSystem InteractionSystem;

        private Proto_RegimentsRegister Register;

        private void Awake()
        {
            Register = new Proto_RegimentsRegister();
        }

        private void Start() => OnGameStart();

        private void OnGameStart()
        {
            if (!TryGetComponent(out EntityFactory factory)) return;
            Register.Regiments = factory.CreateRegiments();
            Destroy(factory);
        }
    }
}
