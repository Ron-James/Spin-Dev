using System;using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[Serializable]
public abstract class BaseActionObjective
{ 
    public bool HasCompleteAction { get => _isComplete; set => _isComplete = value; }
    [GUIColor("@HasCompleteAction ? Color.green : Color.red")]
    [SerializeField, ReadOnly, Save]
    protected bool _isComplete;

    public BaseActionObjective()
    {
    }
    
    public abstract void Setup(Object parent, string methodName, Action callback = null);
    public abstract void Dispose(Object parent, string methodName);
    public virtual void Complete()
    {
        HasCompleteAction = true;
    }
    
    [Button]
    public virtual void Reset()
    {
        HasCompleteAction = false;
    }
    
    
}
