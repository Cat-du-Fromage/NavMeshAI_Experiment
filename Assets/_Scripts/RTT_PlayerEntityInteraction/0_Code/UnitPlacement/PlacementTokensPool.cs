using System;
using System.Collections.Generic;
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
            tokens = new ObjectPool<GameObject>(CreateToken, OnGet, OnRelease, OnDestroyToken);
        }

        private GameObject CreateToken()
        {
            GameObject token = Instantiate(prefab);
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

        private void OnDestroyToken(GameObject token)
        {
            token.SetActive(false);
        }

        public void ReleaseAll(ref List<GameObject> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                tokens.Release(objects[i]);
            }
            objects.Clear();
        }
        
        public void ReleaseAll(ref Dictionary<int, Transform[]> transforms)
        {
            foreach ((int _, Transform[] value) in transforms)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    tokens.Release(value[i].gameObject);
                }
            }
        }
    }
}