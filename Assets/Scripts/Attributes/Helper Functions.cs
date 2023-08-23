using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
    public static List<T> GetScriptableObjects<T>(string path) where T : ScriptableObject
    {
#if UNITY_EDITOR

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).ToString(), new[] { path });
        List<T> scriptableObjects = new List<T>();

        foreach (var guid in guids)
        {
            UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            scriptableObjects.Add(UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T);
        }

        return scriptableObjects;
#else
        return null;
#endif
    }

    public static List<T> GetUpgradeAttributes<T>(string path) where T : UpgradeAttribute
    {
    #if UNITY_EDITOR

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t" + typeof(T).ToString(), new[] { path });
        List<T> upgrade_attributes = new List<T>();

        foreach(var guid in guids)
        {
            UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            upgrade_attributes.Add(UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T);
        }
        return upgrade_attributes;
    #else
        return null;
    #endif    
    }
}

