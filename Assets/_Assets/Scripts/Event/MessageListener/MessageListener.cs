using System;using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public interface IMessageListener
{
    UnityAction OnMessageReceived { get; }

    void AddListener(UnityAction action);
    
    void RemoveListener(UnityAction action);
    
    
}


public abstract class MessageListener<T> : MonoBehaviour, IInitializable, IMessageListener where T : IEquatable<T>
{
    
    [SerializeField] EventSO<T> _event;
    [SerializeField] protected List<T> _listenerValues = new List<T>();

    [SerializeField] protected UnityEvent OnMessageRaised;
    
    public UnityAction OnMessageReceived => OnMessageRaised.Invoke;
    public void AddListener(UnityAction action)
    {
        OnMessageRaised.AddListener(action);
    }

    public void RemoveListener(UnityAction action)
    {
        OnMessageRaised.RemoveListener(action);
    }

    public void OnEventRaised(T value)
    {
        if(_listenerValues.Contains(value))
        {
            OnMessageRaised.Invoke();
        }
    }

    public Task Init()
    {
        _event.Subscribe(this, name, OnEventRaised);
        return Task.CompletedTask;
    }


    private void OnDisable()
    {
        _event.UnsubscribeAll(this);
    }
}


public abstract class MessageListenerSO<T> : SerializableScriptableObject, IMessageListener, ISceneCycleListener where T : IEquatable<T>
{
    [SerializeField] EventSO<T> _event;
    [SerializeField] protected List<T> _listenerValues = new List<T>();

    [SerializeField] protected UnityEvent OnMessageRaised;
    public UnityAction OnMessageReceived => OnMessageRaised.Invoke;
    public void AddListener(UnityAction action)
    {
        OnMessageRaised.AddListener(action);
    }

    public void RemoveListener(UnityAction action)
    {
        OnMessageRaised.RemoveListener(action);
    }

    public void OnEventRaised(T value)
    {
        if(_listenerValues.Contains(value))
        {
            OnMessageRaised.Invoke();
        }
    }


    public void OnSceneStarted(Scene scene)
    {
        _event.Subscribe(this, "Listen for messages", OnEventRaised);
    }

    public void OnSceneStopped(Scene scene)
    {
        _event.UnsubscribeAll(this);
    }

    public void OnEditorStopped()
    {
        
    }
}