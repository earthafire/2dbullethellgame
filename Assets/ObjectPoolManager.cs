using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

    public static GameObject SpawnObject(GameObject spawnObj, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        PooledObjectInfo pool = ObjectPools.Find(pool => pool.lookupString == spawnObj.name);

        // If there isn't an existing pool, create one
        if(pool == null)
        {
            pool = new PooledObjectInfo() { lookupString = spawnObj.name};
            ObjectPools.Add(pool);
        }

        // Check if there are any inactive objects in the pool
        GameObject spawnableObj = null;
        spawnableObj = pool.inactiveObjects.FirstOrDefault();

        if(spawnableObj == null)
        {
            spawnableObj = Instantiate(spawnObj, spawnPosition, spawnRotation);
        }
        else
        {
            spawnableObj.transform.position = spawnPosition;
            spawnableObj.transform.rotation = spawnRotation;
            pool.inactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }
    public static GameObject SpawnObject(GameObject spawnObj, Transform parentTransform)
    {
        PooledObjectInfo pool = ObjectPools.Find(pool => pool.lookupString == spawnObj.name);

        // If there isn't an existing pool, create one
        if (pool == null)
        {
            pool = new PooledObjectInfo() { lookupString = spawnObj.name };
            ObjectPools.Add(pool);
        }

        // Check if there are any inactive objects in the pool
        GameObject spawnableObj = null;
        spawnableObj = pool.inactiveObjects.FirstOrDefault();

        if (spawnableObj == null)
        {
            spawnableObj = Instantiate(spawnObj, parentTransform);
        }
        else
        {
            pool.inactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string goName = obj.name.Substring(0,obj.name.Length - 7); // Remove (Clone) to find pool by name

        PooledObjectInfo pool = ObjectPools.Find(pool =>pool.lookupString == goName);

        if( pool == null )
        {
            Debug.LogWarning("Trying to relase an object that has not been pooled: " + obj.name);
        }
        else
        {
            obj.SetActive(false);
            pool.inactiveObjects.Add(obj);
        }
    }
}
public class PooledObjectInfo
{
    public string lookupString;
    public List<GameObject> inactiveObjects = new List<GameObject>();
}

