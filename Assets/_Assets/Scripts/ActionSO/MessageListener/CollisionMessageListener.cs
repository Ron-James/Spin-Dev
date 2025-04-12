using System;

using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;



[Serializable]
public class AreaObjective : BaseActionObjective
{
    [SerializeField, DontSave] private MonoReference<ColliderTrigger> targetCollider = new();
    [DontSave]private Object _parent;
    [SerializeField, DontSave, GenericAssetSelector(typeof(CollisionActionSO))] private CollisionActionSO collisionAction;
    public event Action callback; 
    public override void Setup(Object parent, string methodName, Action callback = null)
    {
        try
        {
            _parent = parent;
            collisionAction.Subscribe(parent, methodName, OnEventRaised);
            _parent = parent;
            this.callback = callback;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    
    private void OnEventRaised(CollisionData value)
    {
        if (value.OtherCollider != targetCollider.Value) return;
        
        Complete();
        callback?.Invoke();
        
    }

    public override void Dispose(Object parent, string methodName)
    {
        try
        {
            collisionAction.Unsubscribe(parent, methodName);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}

[Serializable]
public class ItemActionObjective : BaseActionObjective
{
    
    [SerializeField, DontSave] private Item item;
    [SerializeField, Save] private int quanitityChange;
    [SerializeField, ReadOnly, Save] private int currentDelta;

    public override void Setup(Object parent, string methodName, Action callback = null)
    {
        
    }

    public override void Dispose(Object parent, string methodName)
    {
        
    }
}