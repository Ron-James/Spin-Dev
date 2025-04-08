using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;


/// <summary>
/// Helper class to store a reference to an object that implements IGuidAsset.
/// </summary>
[Serializable]
public class ObjectReference : IEquatable<Object>, IEquatable<IGuidAsset>
{
    [SerializeField, OnValueChanged("UpdateGUID")] protected GameObject _dropInReference;
    [SerializeField, ShowIf("ValidateGUID"), ReadOnly, Save] protected string _name;
    [SerializeField, ReadOnly, Save] protected string guid;

    protected bool ValidateGUID() => !string.IsNullOrEmpty(guid);
    
    
    public ObjectReference()
    {
        guid = string.Empty;
        
    }
    
    protected virtual void UpdateGUID()
    {
        if (_dropInReference == null) return;

        try
        {
            if (_dropInReference.TryGetComponent<IGuidAsset>(out var guidAsset))
            {
                guid = guidAsset.Guid;
                _name = _dropInReference.name;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating GUID: {e.Message}");
            guid = string.Empty;
        }
    }

    public ObjectReference(GameObject obj)
    {
        if (obj.TryGetComponent<IGuidAsset>(out var guidAsset))
        {
            guid = guidAsset.Guid;
            _dropInReference = obj;
        }
        else
        {
            throw new Exception("Object does not implement IGuidAsset");
        }
    }
    
    [ShowInInspector, ReadOnly]
    public virtual GameObject Value => _dropInReference ? _dropInReference : ObjectFinder.FindObjectByGuid(guid) as GameObject;

    public bool Equals(Object other) => other is IGuidAsset guidAsset && guid == guidAsset.Guid;

    public bool Equals(IGuidAsset other) => guid == other.Guid;
}
