using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ObjectFinder
{
    public static Object FindObjectByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid))
        {
            return null;
        }
        try
        {
            ScriptableObject scriptableObject = ScriptableObjectManager.GetAssetByGuid(guid);
            if (scriptableObject != null)
            {
                return scriptableObject;
            }

            MonoBehaviour monoBehaviour = MonoBehaviouRegistry.GetMonoByGuid(guid);
            if (monoBehaviour != null)
            {
                return monoBehaviour;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error finding object by GUID: " + e.Message);
            
        }
        return null;


    }
}