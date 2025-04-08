using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

public static class MonoBehaviouRegistry
{
    public static Dictionary<System.Type, Dictionary<string, MonoBehaviour>> registry = new();

    public static MonoBehaviour[] All => registry.Values.SelectMany(x => x.Values).ToArray();
    
    
    
    /// <summary>
    /// This is handled by the SceneTracker, which will call this method when a scene is loaded.
    /// Intended to maintain a register of all MonoBehaviours that implement IGuidAsset.
    /// </summary>
    public static void FindSerializableMonoBehaviours()
    {
        MonoBehaviour[] allMonoBehaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        registry.Clear();
        foreach (var monoBehaviour in allMonoBehaviours)
        {
            if (monoBehaviour is IGuidAsset guidAsset)
            {
                if (string.IsNullOrEmpty(guidAsset.Guid))
                {
                    guidAsset.AssignGuid();
                }

                // Use the asset's runtime type.
                Type type = monoBehaviour.GetType();

                // Initialize the dictionary for this type if it doesn't exist.
                if (!registry.ContainsKey(type))
                {
                    registry[type] = new Dictionary<string, MonoBehaviour>();
                }

                // Add the asset to the registry using its GUID.
                if (!registry[type].ContainsKey(guidAsset.Guid))
                {
                    registry[type][guidAsset.Guid] = monoBehaviour;
                }
                else
                {
                    Debug.LogWarning($"Duplicate GUID detected: {guidAsset.Guid} on asset {monoBehaviour.name}");
                }
            }
        }
    }


    public static void ClearRegistry()
    {
        registry.Clear();
    }

    
    /// <summary>
    /// Tries to find the first object of type T in the registry.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T FindFirstObjectByType<T>() where T : MonoBehaviour, IGuidAsset
    {
        foreach (var kvp in registry)
        {
            // Check if the stored type is T or a subclass of T.
            if (typeof(T).IsAssignableFrom(kvp.Key))
            {
                foreach (var asset in kvp.Value.Values)
                {
                    if (asset is T typedAsset)
                    {
                        return typedAsset;
                    }
                }
            }
        }

        Debug.LogError("The object of type " + typeof(T) + " was not found in the registry.");
        return null;
    }

    
    /// <summary>
    /// Tries to find the first object of type T in the registry by its GUID.
    /// </summary>
    /// <param name="guid"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetMonoByGuid<T>(string guid) where T : MonoBehaviour, IGuidAsset
    {
        foreach (var kvp in registry)
        {
            // Check if the stored type is T or a subclass of T.
            if (typeof(T).IsAssignableFrom(kvp.Key))
            {
                if (kvp.Value.TryGetValue(guid, out MonoBehaviour mono))
                {
                    return mono as T;
                }
            }
        }

        Debug.LogWarning($"Asset of type {typeof(T)} with GUID {guid} not found.");
        return null;
    }
    
    
    /// <summary>
    /// Tries to find the first object of type T in the registry by its GUID.
    /// </summary>
    /// <param name="guid">The GUID youre looking for</param>
    /// <returns></returns>
    public static MonoBehaviour GetMonoByGuid(string guid)
    {
        foreach (var kvp in registry)
        {
            if (kvp.Value.TryGetValue(guid, out MonoBehaviour mono))
            {
                return mono;
            }
        }

        Debug.LogWarning($"Asset with GUID {guid} not found.");
        return null;
    }
    
    
    /// <summary>
    /// Looks for all MonoBehaviours of type T in the registry.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] GetMonosByType<T>() where T : MonoBehaviour, IGuidAsset
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
}