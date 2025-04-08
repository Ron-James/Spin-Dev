using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

/// <summary>
/// Stores saved data for a collection of ScriptableObjects.
/// </summary>
[Serializable]
public class SaveState
{
    [OdinSerialize]
    public Dictionary<ScriptableObjectReference<SerializableScriptableObject>, object> savedData = new();
    
    /// <summary>
    /// Rehydrates all saved POCO data into new runtime ScriptableObject instances for preview or future use.
    /// </summary>
    [Button]
    public void RehydrateToRuntimeInstances()
    {
        var newDict = new Dictionary<ScriptableObjectReference<SerializableScriptableObject>, object>();

        foreach (var kvp in savedData)
        {
            var reference = kvp.Key;
            var savedPOCO = kvp.Value;

            if (reference.Value == null || savedPOCO == null) continue;

            var type = reference.Value.GetType();
            var runtimeInstance = ScriptableObject.CreateInstance(type);
            runtimeInstance.name = reference.Value.name + "_Runtime";

            try
            {
                if (runtimeInstance is ISaveable saveable)
                {
                    saveable.RestoreState(savedPOCO); // POCO gets applied to blank SO
                }

                newDict[reference] = runtimeInstance;
            }
            catch (Exception e)
            {
                Debug.LogError($"Rehydration failed for {reference.Value.name}: {e.Message}");
            }
        }

        savedData = newDict;
    }


}



