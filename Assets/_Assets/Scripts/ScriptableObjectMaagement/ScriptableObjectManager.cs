using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public static class ScriptableObjectManager
{
    // Dictionary mapping type to another dictionary that maps GUID to asset.
    private static Dictionary<System.Type, Dictionary<string, ScriptableObject>> registry = new();
    public static T[] LoadScriptableObject<T>(string path = "") where T : ScriptableObject
    {
        var scriptableObjects = Resources.LoadAll<T>(path);
        return scriptableObjects;
    }
    
    
    public static T LoadFirstScriptableObject<T>(string path = "") where T : ScriptableObject
    {
        var scriptableObjects = Resources.LoadAll<T>(path);
        return scriptableObjects.FirstOrDefault();
    }
    

    public static ScriptableObject[] All => registry.Values.SelectMany(x => x.Values).ToArray();
    
    
    
    public static ISaveable[] GetSaveableScriptableObjects()
    {
        return registry.Values.SelectMany(x => x.Values).OfType<ISaveable>().ToArray();
    }
    
    
    
    

    // Automatically initialize before the first scene loads (during the splash screen).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadAllAssets();
    }

    // Load all ScriptableObject assets from Resources, check for IGuidAsset, and map them.
    public static void LoadAllAssets()
    {
        // Load all ScriptableObjects from the Resources folder.
        ScriptableObject[] allAssets = Resources.LoadAll<ScriptableObject>("");
        
        foreach (var asset in allAssets)
        {
            // Check if the asset implements IGuidAsset.
            if (asset is IGuidAsset guidAsset)
            {
                // Ensure the asset has a GUID assigned.
                if (string.IsNullOrEmpty(guidAsset.Guid))
                {
                    guidAsset.AssignGuid();
                }
                
                // Use the asset's runtime type.
                Type assetType = asset.GetType();

                // Initialize the dictionary for this type if it doesn't exist.
                if (!registry.ContainsKey(assetType))
                {
                    registry[assetType] = new Dictionary<string, ScriptableObject>();
                }

                // Add the asset to the registry using its GUID.
                if (!registry[assetType].ContainsKey(guidAsset.Guid))
                {
                    registry[assetType][guidAsset.Guid] = asset;
                }
                else
                {
                    Debug.LogWarning($"Duplicate GUID detected: {guidAsset.Guid} on asset {asset.name}");
                }
            }
        }
    }

    // Retrieve asset by GUID for a specified type.
    public static T GetAssetByGuid<T>(string guid) where T : ScriptableObject, IGuidAsset
    {
        foreach (var kvp in registry)
        {
            // Check if the stored type is T or a subclass of T.
            if (typeof(T).IsAssignableFrom(kvp.Key))
            {
                if (kvp.Value.TryGetValue(guid, out ScriptableObject asset))
                {
                    return asset as T;
                }
            }
        }
        Debug.LogWarning($"Asset of type {typeof(T)} with GUID {guid} not found.");
        return null;
    }

    public static ScriptableObject GetAssetByGuid(string guid)
    {
        foreach (var kvp in registry)
        {
            if (kvp.Value.TryGetValue(guid, out ScriptableObject asset))
            {
                return asset;
            }
        }
        Debug.LogWarning($"Asset with GUID {guid} not found.");
        return null;
    }
    
    public static T[] GetAssetsByType<T>() where T : ScriptableObject, IGuidAsset
    {
        List<T> assets = new List<T>();
        foreach (var kvp in registry)
        {
            // Check if the asset type is T or a subclass of T.
            if (typeof(T).IsAssignableFrom(kvp.Key))
            {
                foreach (var asset in kvp.Value.Values)
                {
                    if (asset is T typedAsset)
                    {
                        assets.Add(typedAsset);
                    }
                }
            }
        }
        if (assets.Count == 0)
        {
            Debug.LogWarning($"No assets of type {typeof(T)} found.");
        }
        return assets.ToArray();
    }
    
    public static T GetFirstAssetOfType<T>() where T : ScriptableObject
    {
        var assetType = typeof(T);
        if (registry.TryGetValue(assetType, out Dictionary<string, ScriptableObject> assetDict))
        {
            return assetDict.Values.FirstOrDefault() as T;
        }
        Debug.LogWarning($"No assets of type {assetType} found.");
        return ScriptableObject.CreateInstance<T>();
    }
}