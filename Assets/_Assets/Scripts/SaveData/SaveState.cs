using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

/// <summary>
/// Represents a snapshot of saved ScriptableObject data and their runtime rehydrated instances.
/// </summary>
[Serializable]
public class SaveState
{
    [OdinSerialize]
    public Dictionary<ScriptableObjectReference<SerializableScriptableObject>, Dictionary<string, object>> SavedData = new();

    [NonSerialized, ShowInInspector, ReadOnly]
    private Dictionary<ScriptableObjectReference<SerializableScriptableObject>, ScriptableObject> runtimeInstances =
        new();

    /// <summary>
    /// Rehydrates all saved field data into new runtime ScriptableObject instances.
    /// </summary>
    [Button]
    public void RehydrateToRuntimeInstances()
    {
        runtimeInstances = new();

        foreach (var kvp in SavedData)
        {
            var reference = kvp.Key;
            var fieldData = kvp.Value;

            if (reference.Value == null || fieldData == null) continue;

            var instance = UnityEngine.Object.Instantiate(reference.Value);
            instance.name = reference.Value.name + "_Runtime";

            if (instance is ISaveable saveable)
            {
                saveable.RestoreState(fieldData);
                runtimeInstances[reference] = instance;
            }
            else
            {
                Debug.LogWarning($"Reference {reference.Value.name} is not ISaveable.");
            }
        }
    }

    /// <summary>
    /// Returns the current runtime copy of a ScriptableObject for inspection or use.
    /// </summary>
    public ScriptableObject GetRuntimeInstance(ScriptableObjectReference<SerializableScriptableObject> reference)
    {
        runtimeInstances.TryGetValue(reference, out var instance);
        return instance;
    }

    /// <summary>
    /// Returns all runtime ScriptableObject instances.
    /// </summary>
    public IReadOnlyDictionary<ScriptableObjectReference<SerializableScriptableObject>, ScriptableObject> GetAllRuntimeInstances()
    {
        return runtimeInstances;
    }
}
