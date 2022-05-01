using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Towerino
{
    public class PoolingSystem : MonoBehaviour
    {
        public Transform PoolingWrapper { get; private set; }
        private Dictionary<string, List<GameObject>[]> _data;

        public PoolingSystem StartUp()
        {
            _data = new Dictionary<string, List<GameObject>[]>();
            PoolingWrapper = new GameObject("Unparented Pool").transform;
            return this;
        }

        public GameObject GetObject(GameObject prefab, Transform parent = null)
        {
            string label = $"pool_{prefab.name}";
            GameObject go = null;

            if (_data.ContainsKey(label) && _data[label][1].Count > 0)
            {
                go = _data[label][1][0]; // PICKS FIRST FROM INACTIVE POOL
                _data[label][1].RemoveAt(0);
                if (label.Equals("pool_Ballista") || label.Equals("pool_Squire")) Debug.Log($"Reusing [{label}]: {go.GetInstanceID()}");
            }
            else if (!_data.ContainsKey(label))
            {
                _data[label] = new List<GameObject>[2];
                _data[label][0] = new List<GameObject>(); // ACTIVE
                _data[label][1] = new List<GameObject>(); // INACTIVE
            }

            if (go == null)
            {
                if (parent == null) parent = PoolingWrapper;

                go = Instantiate(prefab, parent);
                go.name = prefab.name;
                go.SetActive(false);
                if (label.Equals("pool_Ballista") || label.Equals("pool_Squire")) Debug.Log($"Create/Add [{label}]: {go.GetInstanceID()}");
            }

            _data[label][0].Add(go);

            return go;
        }

        public void ReturnObject(GameObject go)
        {
            string label = $"pool_{go.name}";

            if (_data.ContainsKey(label))
            {
                int index = _data[label][0].FindIndex(x => x.Equals(go));
                _data[label][0].RemoveAt(index);
                _data[label][1].Add(go);
                if (label.Equals("pool_Ballista") || label.Equals("pool_Squire")) Debug.Log($"Returning [{label}]: {go.GetInstanceID()}");
            }
        }

        public void TurnOffPooledObjects()
        {
            foreach (KeyValuePair<string, List<GameObject>[]> poolCollection in _data)
            {
                for (int i = 0; i < poolCollection.Value[0].Count; i++)
                {
                    if (poolCollection.Value[0][i])
                    {
                        Debug.Log($"Turning off [{poolCollection.Key}]: {poolCollection.Value[0][i].GetInstanceID()}");
                        poolCollection.Value[0][i].SendMessage("TurnOff", true, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        public void FlushData()
        {
            foreach (KeyValuePair<string, List<GameObject>[]> poolCollection in _data)
            {
                DestroyGameObjects(poolCollection, 0);
                DestroyGameObjects(poolCollection, 1);
            }
            _data = null;
            Resources.UnloadUnusedAssets();
        }

        private void DestroyGameObjects(KeyValuePair<string, List<GameObject>[]> poolCollection, int listIndex)
        {
            string debugText = listIndex == 0 ? "ACTIVE" : "INACTIVE";
            for (int i = 0; i < poolCollection.Value[listIndex].Count; i++)
            {
                if (poolCollection.Value[listIndex][i])
                {
                    Debug.Log($"Destroying {debugText} [{poolCollection.Key}]: {poolCollection.Value[listIndex][i].GetInstanceID()}");
                    Destroy(poolCollection.Value[listIndex][i]);
                }
            }
        }
    }
}
