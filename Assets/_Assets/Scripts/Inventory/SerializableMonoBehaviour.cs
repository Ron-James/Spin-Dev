using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class SerializableMonoBehaviour : MonoBehaviour, IGuidAsset
{
    [SerializeField, ReadOnly] private string guid;
    public string Guid => guid;

    public void AssignGuid()
    {
        guid = System.Guid.NewGuid().ToString();
    }

    public virtual void OnValidate()
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
