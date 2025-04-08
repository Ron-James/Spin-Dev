using UnityEngine;
using Sirenix.Serialization;
using System;
using System.Reflection;

/// <summary>
/// Base class for ScriptableObjects that can save and restore POCO state.
/// </summary>
public abstract class SaveableSO<T> : SerializableScriptableObject, ISaveable where T : class, new()
{
    public virtual object GetSaveData()
    {
        return SaveUtility.GetFilteredState<T>(this);
    }

    public virtual void RestoreState(object state)
    {
        if (state is T typed)
            SaveUtility.RestoreFilteredState(typed, this);
        else
            Debug.LogError($"[SaveableSO] Invalid restore type. Expected {typeof(T)}, got {state?.GetType()}");
    }

    public abstract void OnSave();
    public abstract void OnLoad();
}