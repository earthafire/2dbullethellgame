using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    static readonly Dictionary<string, PooledObjectInfo> ObjectPools = new();
    static Transform _poolsRoot;

    public static GameObject SpawnObject(GameObject spawnObj, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        PooledObjectInfo pool = GetOrCreatePool(spawnObj.name);

        GameObject spawnableObj;
        int lastIndex = pool.inactiveObjects.Count - 1;
        if (lastIndex >= 0)
        {
            spawnableObj = pool.inactiveObjects[lastIndex];
            pool.inactiveObjects.RemoveAt(lastIndex);
            spawnableObj.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            spawnableObj.SetActive(true);
        }
        else
        {
            // Parented under a container shared by every instance of this prefab type
            // (see GetOrCreatePool) purely for Hierarchy organization - active and
            // pooled instances of the same type stay grouped together instead of
            // scattered loose at the scene root. Reused instances (the branch above)
            // never leave their pool's container in the first place, since nothing
            // else reparents them.
            spawnableObj = Instantiate(spawnObj, spawnPosition, spawnRotation, pool.parent);
        }

        return spawnableObj;
    }
    public static GameObject SpawnObject(GameObject spawnObj, Transform parentTransform)
    {
        PooledObjectInfo pool = GetOrCreatePool(spawnObj.name);

        GameObject spawnableObj;
        int lastIndex = pool.inactiveObjects.Count - 1;
        if (lastIndex >= 0)
        {
            spawnableObj = pool.inactiveObjects[lastIndex];
            pool.inactiveObjects.RemoveAt(lastIndex);
            spawnableObj.SetActive(true);
        }
        else
        {
            spawnableObj = Instantiate(spawnObj, parentTransform);
        }

        return spawnableObj;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string goName = obj.name.Substring(0, obj.name.Length - 7); // Remove (Clone) to find pool by name

        if (!ObjectPools.TryGetValue(goName, out PooledObjectInfo pool))
        {
            Debug.LogWarning("Trying to relase an object that has not been pooled: " + obj.name);
            return;
        }

        obj.SetActive(false);
        pool.inactiveObjects.Add(obj);
    }

    static PooledObjectInfo GetOrCreatePool(string lookupString)
    {
        if (!ObjectPools.TryGetValue(lookupString, out PooledObjectInfo pool))
        {
            if (_poolsRoot == null)
            {
                _poolsRoot = new GameObject("Object Pools").transform;
            }

            var poolParent = new GameObject(lookupString).transform;
            poolParent.SetParent(_poolsRoot);

            pool = new PooledObjectInfo { lookupString = lookupString, parent = poolParent };
            ObjectPools[lookupString] = pool;
        }
        return pool;
    }
}
public class PooledObjectInfo
{
    public string lookupString;
    public Transform parent;
    public List<GameObject> inactiveObjects = new List<GameObject>();
}
