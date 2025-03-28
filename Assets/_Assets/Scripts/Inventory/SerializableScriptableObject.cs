using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
public interface IGuidAsset
{
    string Guid { get; }
    void AssignGuid();
}
public abstract class SerializableScriptableObject : SerializedScriptableObject, IGuidAsset
{
    [SerializeField, ReadOnly] private string guid;
    public string Guid => guid;

    public void AssignGuid()
    {
        guid = System.Guid.NewGuid().ToString();
    }


    public virtual void OnValidaete()
    {
        if(string.IsNullOrEmpty(guid))
        {
            AssignGuid();
        }
    }


    public virtual void Reset()
    {
        if(string.IsNullOrEmpty(guid))
        {
            AssignGuid();
        }
    }
}


[Serializable]
public class ScriptableObjectReference<T> : IEquatable<ScriptableObjectReference<T>>, IEquatable<T> where T : ScriptableObject, IGuidAsset
{
    [SerializeField, ReadOnly] string guid;
    
    [ShowInInspector, OdinSerialize]
    public T Value => ScriptableObjectManager.GetAssetByGuid<T>(guid);

    public ScriptableObjectReference(T value)
    {
        guid = value.Guid;
        
    }


    public bool Equals(ScriptableObjectReference<T> other)
    {
        return guid == other.guid;
    }

    public override bool Equals(object obj)
    {
        return obj is ScriptableObjectReference<T> other && Equals(other);
    }
    
    public bool Equals(T other)
    {
        return Value == other;
    }

    public override int GetHashCode()
    {
        return (guid != null ? guid.GetHashCode() : 0);
    }
}