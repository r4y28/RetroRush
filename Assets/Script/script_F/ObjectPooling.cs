using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // ✅ Spawn function
    public GameObject SpawnFromPool(string tag, Vector2 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist!");
            return null;
        }

        Queue<GameObject> poolQueue = poolDictionary[tag];

        if (poolQueue.Count == 0)
        {
            Debug.LogWarning("Pool " + tag + " is empty!");
            return null;
        }

        GameObject objectToSpawn = poolQueue.Dequeue();

        objectToSpawn.transform.SetParent(parent);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    // ✅ Return object to pool manually
    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (!poolDictionary.ContainsKey(tag))
        {
            Destroy(obj);
            return;
        }

        poolDictionary[tag].Enqueue(obj);
    }
}