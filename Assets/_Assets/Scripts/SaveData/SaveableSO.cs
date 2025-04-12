using UnityEngine;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Base class for ScriptableObjects that can save and restore POCO state.
/// </summary>
public abstract class SaveableSO<T> : SerializableScriptableObject, ISaveable where T : SaveableSO<T>
{
    public virtual Dictionary<string, object> CaptureState()
    {
        return SaveUtility.ExtractSaveData(this);
    }

    public virtual void RestoreState(Dictionary<string, object> state)
    {
        SaveUtility.ApplySaveData(this, state);
    }

    public abstract void OnSave();
    public abstract void OnLoad();
}