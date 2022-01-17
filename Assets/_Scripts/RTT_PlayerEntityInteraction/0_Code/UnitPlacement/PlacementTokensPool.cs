using System;
using UnityEngine;
using UnityEngine.Pool;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTUnitPlacement
{
    public class PlacementTokensPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        
        public IObjectPool<GameObject> tokens;

        private void Awake()
        {
            tokens = new ObjectPool<GameObject>(CreateToken, OnGet, OnRelease);
        }

        private GameObject CreateToken()
        {
            GameObject obj = Instantiate(prefab);
            //DestinationTokensRenderers.Add();
            //DestinationTokens.Add(DestinationTokensRenderers[i].transform);
            //DestinationTokensRenderers[i].enabled = true;
            return obj;
        }

        private void OnGet(GameObject prefab)
        {
            
        }
        
        private void OnRelease(GameObject prefab)
        {
            
        }
        
        private void OnDestroy()
        {
            
        }
    }
}