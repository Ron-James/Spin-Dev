using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class ScriptableObjectReference<T> : IEquatable<ScriptableObjectReference<T>>, IEquatable<T> where T : ScriptableObject, IGuidAsset
{
    [SerializeField, DontSave] SerializableScriptableObject _dropInReference;
    [SerializeField, Save, HideInInspector] private string guid;

    [SerializeField, HideInInspector]
    private string typeName;

    [ShowInInspector, OdinSerialize]
    public T Value => ScriptableObjectManager.GetAssetByGuid<T>(guid);

    public Type Type => Type.GetType(typeName);
    
    public ScriptableObjectReference()
    {
        guid = string.Empty;
        typeName = typeof(T).AssemblyQualifiedName;
    }
    
    
    private void UpdateGUID()
    {
        if (_dropInReference == null) return;

        try
        {
            guid = _dropInReference.Guid;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating GUID: {e.Message}");
            guid = string.Empty;
        }
    }
    public ScriptableObjectReference(T value)
    {
        guid = value.Guid;
        typeName = value.GetType().AssemblyQualifiedName;
    }

    public bool Equals(ScriptableObjectReference<T> other)
    {
        return guid == other.guid;
    }

    public bool Equals(T other)
    {
        return Value == other;
    }

    public override bool Equals(object obj)
    {
        return obj is ScriptableObjectReference<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (guid != null ? guid.GetHashCode() : 0);
    }

    public override string ToString() => $"{typeof(T).Name}({guid})";
}
