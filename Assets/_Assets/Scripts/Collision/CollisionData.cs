using System;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CollisionData : IEquatable<CollisionData>
{
    [SerializeField] protected MonoReference<ColliderTrigger> otherCollider = new();
    public CollisionData()
    {
        
    }
    public CollisionData(ColliderTrigger otherCollider)
    {
        this.otherCollider = otherCollider;
    }

    public ColliderTrigger OtherCollider
    {
        get => otherCollider.Value;
        set => otherCollider = value.gameObject;
    }
    

    public bool Equals(CollisionData other)
    {
        return Equals(this.OtherCollider, other.OtherCollider);
    }

    public override bool Equals(object obj)
    {
        return obj is CollisionData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return otherCollider != null ? otherCollider.GetHashCode() : 0;
    }
}