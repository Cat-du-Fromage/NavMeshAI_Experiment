using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTUnits;
using UnityEngine;

namespace KaizerWaldCode
{
    public class EntitySystem : MonoBehaviour
    {
        [SerializeField] private RegimentsRegister regimentsRegister;

        private void Awake()
        {
            regimentsRegister ??= FindObjectOfType<RegimentsRegister>();
        }
        
        private void Start()
        {
            OnGameStart();
        }

        private void OnGameStart()
        {
            if (!TryGetComponent(out RegimentFactory factory)) return;
            regimentsRegister.Regiments = factory.CreateRegiments();
            Destroy(factory);
        }
        
    }
}
