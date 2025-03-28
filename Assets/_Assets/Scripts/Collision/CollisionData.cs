using System;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public class CollisionEnteredData : CollisionData
{
    
    [SerializeField, ReadOnly] private bool hasEntered;
    
    public bool HasEntered
    {
        get => hasEntered;
        set => hasEntered = value;
    }


    public void Enter()
    {
        hasEntered = true;
    }
    
    public void Exit()
    {
        hasEntered = false;
    }
    
    
    public CollisionEnteredData(ColliderTrigger trigger, Collider other) : base(trigger, other)
    {
        HasEntered = false;
    }
}
[Serializable]
public class CollisionData : IEquatable<CollisionData>
{
    protected ColliderTrigger trigger;
    protected Collider other;
    
    public CollisionData(ColliderTrigger trigger, Collider other)
    {
        this.Trigger = trigger;
        this.Other = other;
    }

    public Collider Other
    {
        get => other;
        set => other = value;
    }

    public ColliderTrigger Trigger
    {
        get => trigger;
        protected set => trigger = value;
    }

    public bool Equals(CollisionData other)
    {
        return Equals(Trigger, other.Trigger) && Equals(this.Other, other.Other);
    }

    public override bool Equals(object obj)
    {
        return obj is CollisionData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Trigger, Other);
    }
}