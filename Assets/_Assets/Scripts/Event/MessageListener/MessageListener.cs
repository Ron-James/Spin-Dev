using System;using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;


public interface IMessageListener 
{
    UnityAction OnMessageReceived { get; }

    void AddListener(UnityAction action);
    void RemoveListener(UnityAction action);
    
    bool HasReceived { get; set; }
    
    
}

public interface IMessageListener<T> : IMessageListener where T : IEquatable<T>
{
    UnityAction<T> OnMessageReceivedTyped { get; }
    void AddListener(UnityAction<T> action);
    void RemoveListener(UnityAction<T> action);
}
[Serializable]
public abstract class BaseActionRequirement
{ 
    
    public bool HasCompleteAction { get => _isComplete; set => _isComplete = value; }
    
    [GUIColor("@HasCompleteAction ? Color.green : Color.red")]
    [SerializeField, ReadOnly, Save]
    protected bool _isComplete;

    public BaseActionRequirement()
    {
    }
    public abstract void Setup(Object parent, string methodName, Action callback = null);
    public abstract void Dispose(Object parent, string methodName);

    
    [Button]
    public virtual void Reset()
    {
        HasCompleteAction = false;
    }
    
    
}
[Serializable]

public class  ActionRequirement<T> : BaseActionRequirement where T : IEquatable<T>
{
    
    [SerializeField] EventSO<T> _event;
    protected Object _parent; 
    [SerializeField] protected UnityEvent _onMessageRaised = new();
    public UnityAction OnMessageReceived => _onMessageRaised.Invoke;
    
    public event Action messageCallback;
    private string GetColorForListenerValue(T value)
    {
        return HasCompleteAction ? "green" : "red";
    }

    public ActionRequirement()
    {
    }
    

    public override void Setup(Object parent, string methodName, Action callback = null)
    {
        try
        {
            messageCallback = callback;
            _event.Subscribe(parent, methodName, OnEventRaised);
            _parent = parent;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    
    public override void Dispose(Object parent, string methodName)
    {
        try
        {
            _event.Unsubscribe(parent, methodName);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    public virtual void Complete()
    {
        HasCompleteAction = true;
        _onMessageRaised?.Invoke();
        messageCallback?.Invoke();
    }
    protected virtual void OnEventRaised(T value)
    {
        Complete();
    }
}