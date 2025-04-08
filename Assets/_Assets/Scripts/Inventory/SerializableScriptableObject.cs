using Sirenix.OdinInspector;
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