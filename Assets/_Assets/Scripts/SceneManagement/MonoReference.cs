using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class MonoReference<T> : ObjectReference, IEquatable<T>, IEquatable<MonoReference<T>> where T : MonoBehaviour, IGuidAsset
{
    
    protected T _cachedValue;

    public MonoReference() { }

    public MonoReference(GameObject obj) : base(obj)
    {
        if (obj.GetComponent<T>() != null)
        {
            _cachedValue = obj.GetComponent<T>();
        }
        else
        {
            throw new Exception($"Object does not implement IGuidAsset or missing component of type {typeof(T)}");
        }
    }

    protected override void UpdateGUID()
    {
        if(_dropInReference == null) return;
        try
        {
            if (_dropInReference.GetComponent<T>() != null)
            {
                _cachedValue = _dropInReference.GetComponent<T>();
                base.UpdateGUID();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating GUID: {e.Message}");
            guid = string.Empty;
            _cachedValue = null;
        }

        
    }

    // Retrieves the value from the _dropInReference or finds it using the GUID
    public T RetrieveValue()
    {
        if (_dropInReference is T value)
        {
            _cachedValue = value;
        }

        if (_cachedValue != null)
        {
            return _cachedValue;
        }

        try
        {
            _cachedValue = ObjectFinder.FindObjectByGuid(guid) as T;
            return _cachedValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving object by GUID: {e.Message}");
            throw;
        }
    }

    public static implicit operator T(MonoReference<T> objRef) => objRef.Value;
    
    public static implicit operator MonoReference<T>(T obj) => new MonoReference<T>(obj.gameObject);

    public static implicit operator MonoReference<T>(GameObject obj) => new MonoReference<T>(obj);

    public static implicit operator Object(MonoReference<T> objRef) => objRef.Value;

    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    public T Value
    {
        get
        {
            if (_cachedValue != null)
            {
                return _cachedValue;
            }

            if (_dropInReference is T value)
            {
                _cachedValue = value;
                return value;
            }
            try
            {
                _cachedValue = ObjectFinder.FindObjectByGuid(guid) as T; 
            }
            catch (Exception e)
            {
                Debug.LogError($"Error retrieving object by GUID: {e.Message}");
                throw;
            }
            
            return _cachedValue;
        }
    }

    public bool Equals(MonoReference<T> other)
    {
        if (other is null) return false;
        return guid == other.guid;
    }

    public override bool Equals(object obj) => obj is MonoReference<T> other && Equals(other);

    public bool Equals(T other) => Value.Equals(other);

    public override int GetHashCode() => guid != null ? guid.GetHashCode() : 0;
}
