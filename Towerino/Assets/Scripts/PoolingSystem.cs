using System.Collections.Generic;
using UnityEngine;

namespace Towerino
{
    // Class that watch unactive and active gameobjects to reuse them in order to
    // avoid instantiating and destroying new objects.
    public class PoolingSystem : MonoBehaviour
    {
        public Transform PoolingWrapper { get; private set; }
        private Dictionary<string, List<GameObject>[]> _data;

        // Initializes our pooling object reference dictionary
        // as well as creating a pooling object wrapper (optional usage)
        public PoolingSystem StartUp()
        {
            _data = new Dictionary<string, List<GameObject>[]>();
            PoolingWrapper = new GameObject("Unparented Pool").transform;
            return this;
        }

        // Search for an available object of an specific type and returns it
        // Or creates a new instance of it in case no available inactive objects are found.
        public GameObject GetObject(GameObject prefab, Transform parent = null)
        {
            string label = $"pool_{prefab.name}";
            GameObject go = null;

            // To avoid iteration we check if Key exist in dictionay for object type
            // If Key exists we look into list index 1 where we store only inactive objects
            // We retrieve this reference and remove it from our inactive list
            if (_data.ContainsKey(label) && _data[label][1].Count > 0)
            {
                go = _data[label][1][0]; // PICKS FIRST FROM INACTIVE POOL
                _data[label][1].RemoveAt(0);
                Debug.Log($"Reusing [{label}]: {go.GetInstanceID()}");
            }
            else if (!_data.ContainsKey(label))
            {
                _data[label] = new List<GameObject>[2];
                _data[label][0] = new List<GameObject>(); // ACTIVE
                _data[label][1] = new List<GameObject>(); // INACTIVE
            }

            if (go == null)
            {
                // In case we provide a parenting transform we respect that
                // if now we use our general purpose pooling wrapper :)
                if (parent == null) parent = PoolingWrapper;

                go = Instantiate(prefab, parent);
                go.name = prefab.name;
                go.SetActive(false);
                Debug.Log($"Create/Add [{label}]: {go.GetInstanceID()}");
            }

            // Now that we have an existing or new instance we add it to the end of our
            // list index 0 "active" objects
            _data[label][0].Add(go);

            return go;
        }

        // To return inactive objects we remove it from active list in its key category
        // and add it to the end of our inactive list.
        // We avoid iterating through lists by using FindIndex
        public void ReturnObject(GameObject go)
        {
            string label = $"pool_{go.name}";

            if (_data.ContainsKey(label))
            {
                int index = _data[label][0].FindIndex(x => x.Equals(go));
                _data[label][0].RemoveAt(index);
                _data[label][1].Add(go);
                Debug.Log($"Returning [{label}]: {go.GetInstanceID()}");
            }
        }

        // We destroy all objects BOOM! And clean our dictionary reference holder
        public void FlushData()
        {
            foreach (KeyValuePair<string, List<GameObject>[]> poolCollection in _data)
            {
                DestroyGameObjects(poolCollection, 0);
                DestroyGameObjects(poolCollection, 1);
            }
            _data = new Dictionary<string, List<GameObject>[]>();
            Resources.UnloadUnusedAssets();
        }

        // Private method used by FlushData, this is in charge actually destroying gameobjects
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
