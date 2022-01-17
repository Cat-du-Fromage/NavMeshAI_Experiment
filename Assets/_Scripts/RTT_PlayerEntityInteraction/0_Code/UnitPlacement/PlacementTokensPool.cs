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
            GameObject token = Instantiate(prefab);
            token.AddComponent<DestinationTokenComponent>();
            //DestinationTokensRenderers.Add();
            //DestinationTokens.Add(DestinationTokensRenderers[i].transform);
            //DestinationTokensRenderers[i].enabled = true;
            return token;
        }

        private void OnGet(GameObject token)
        {
            token.SetActive(true);
        }
        
        private void OnRelease(GameObject token)
        {
            token.SetActive(false);
        }
    }
}