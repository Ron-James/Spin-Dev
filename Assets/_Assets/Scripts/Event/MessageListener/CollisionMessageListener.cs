using System;

using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public class CollisionActionRequirement : ActionRequirement<CollisionData>
{
    [SerializeField] private MonoReference<ColliderTrigger> targetCollider = new();
    protected override void OnEventRaised(CollisionData value)
    {
        if (value.OtherCollider != targetCollider.Value) return;
        Complete();
    }
}

[Serializable]
public class ItemActionRequirement : ActionRequirement<ItemData>
{
    [SerializeField] private Item item;
    [SerializeField, Save] private int quanitityChange;
    [SerializeField, ReadOnly, Save] private int currentDelta;
    protected override void OnEventRaised(ItemData value)
    {
        if (value.ItemReference.Value != item) return;
        currentDelta += value.Quantity;
        if (currentDelta >= quanitityChange)
        {
            currentDelta = quanitityChange;
            Complete();
        }
        
    }
}