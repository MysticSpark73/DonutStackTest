using System.Collections.Generic;
using UnityEngine;

namespace DonutStack.Core.Pooling
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance;
        [SerializeField] private List<PoolableObject> objects;

        private Dictionary<string, Queue<GameObject>> pools;


        private void Awake()
        {
            CreateInstance();
            InitializePools();
        }

        public void ReturnIntoPool(string key, GameObject go) 
        {
            if (!pools.ContainsKey(key))
            {
                Debug.LogError($"[ObjectPooler][Return] The Key {key} does not exists in a dictionary!");
                return;
            }
            go.SetActive(false);
            pools[key].Enqueue(go);
        }

        public IPoolable SpawnFromPool(string key, Vector3 pos, Transform parent) {
            if (!pools.ContainsKey(key))
            {
                Debug.LogError($"[ObjectPooler][SpawnFromPool] The Key {key} does not exists in a dictionary!");
                return null;
            }

            if (pools[key].Count == 0)
            {
                Debug.LogError($"The Queue with key {key} is empty!");
                return null;
            }
            GameObject obj = pools[key].Dequeue();
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            obj.SetActive(true);
            IPoolable poolable = obj.GetComponent<IPoolable>();

            if (poolable == null)
            {
                Debug.LogError($"[ObjectPooler][SpawnFromPool] The object with key {key} does not have any component that derives from IPoolable interface!");
                return null;
            }

            poolable.OnPool();
            return poolable;
        }

        private void CreateInstance()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
        }

        private void InitializePools() {
            pools = new Dictionary<string, Queue<GameObject>>();

            Queue<GameObject> q = new Queue<GameObject>();
            for (int i = 0; i < objects.Count; i++)
            {
                q.Clear();
                for (int j = 0; j < objects[i].poolSize; j++)
                {
                    GameObject go = Instantiate(objects[i].prefab, objects[i].poolContainer);
                    go.name = $"{objects[i].key}{j}";
                    go.SetActive(false);
                    q.Enqueue(go);
                }
                pools.Add(objects[i].key, q);
            }
        }

        #region Struct
        [System.Serializable]
        public struct PoolableObject {
            public string key;
            public int poolSize;
            [Header("Prefab should contain IPoolable component")]
            public GameObject prefab;
            public Transform poolContainer;
        }
        #endregion

    }
}