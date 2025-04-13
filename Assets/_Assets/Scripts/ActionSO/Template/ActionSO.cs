using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CustomSceneReference;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

/// <summary>
/// Base class for all ActionSOs. This allows us to group ActionSOs regardless of return type and create actions which don't have return type.
/// </summary>
[CreateAssetMenu(fileName = "EventSO", menuName = "Scriptable Objects/EventSO")]
public abstract class BaseActionSO : SerializableScriptableObject, ISceneCycleListener
{
    [SerializeReference] protected List<BaseActionSubscriber> _subscribers = new List<BaseActionSubscriber>();
    
    /// <summary>
    /// Thes objects subscribe to the SceneTracker so they are notified when a new scene has been loaded in.
    /// </summary>
    /// <param name="scene">The scene which has been loaded</param>
    public void OnSceneStarted(Scene scene)
    {
        Debug.Log("Stopwatch started" + name);
    }
    
    
    
    /// <summary>
    /// Called when the current scene is unloaded.
    /// </summary>
    /// <param name="scene">The scene which has been unloaded </param>
    public virtual void OnSceneStopped(Scene scene)
    {
        
    }
    
    
    
    /// <summary>
    /// Called in editor when playmode is exited.
    /// This is to simulate how scriptable objects behave when application is closed.
    /// </summary>
    public virtual void OnEditorStopped()
    {
        _subscribers.Clear();
    }

    
    
    /// <summary>
    /// Raise the event and notify subs.
    /// </summary>
    [Button]
    public abstract void Raise();

    
    
    
    /// <summary>
    /// Subscribe function to listner list
    /// </summary>
    /// <param name="origin">The unity object to track back the subscribe</param>
    /// <param name="methodName">Name of the method, should match the name</param>
    /// <param name="response"></param>
    public abstract void Subscribe(Object origin, string methodName, UnityAction response);


    public abstract void UnsubscribeAll(Object origin);

    public abstract void Unsubscribe(Object origin, string methodName);
    
    
    
    
    [OdinSerialize, Tooltip("If the action has been raised at least once this session")]
    public bool HasRaised { get; protected set; }
    
    
    /// <summary>
    /// removes all null subs
    /// </summary>
    [Button]
    public virtual void RemoveNullSubscribers()
    {
        for(int loop = _subscribers.Count - 1; loop >= 0; loop--)
        {
            if(_subscribers[loop] == null)
            {
                _subscribers.RemoveAt(loop);
            }
            else if(_subscribers[loop].Origin == null || _subscribers[loop] == null)
            {
                _subscribers.RemoveAt(loop);
            }
        }
    }

    
}



public abstract class ActionSO<T> : BaseActionSO, IValueAsset<T>
{
    [SerializeField, Tooltip("The default value this action stores before being raised for the first time.")] private T _defaultCallValue;
    [SerializeReference, ReadOnly, Tooltip("List of objects that have subscribed to he message published though this action.")] protected List<ActionMessageSubscriber<T>> _messageSubscribers = new();

    [SerializeField, ReadOnly, Tooltip("The value stored in this action. Last value raised")] private T _storedValue;
    
    
    
    /// <summary>
    /// The last value passed in the raise function is stored via this property
    /// Setting it from outside raises the event
    /// </summary>
    public T StoredValue
    {
        get => _storedValue;
        set => Raise(value);
    }

    
    /// <summary>
    /// Function called when play mode has been exited to simulate build behaviour
    /// </summary>
    public override void OnEditorStopped()
    {
        base.OnEditorStopped();
        _messageSubscribers.Clear();
        _storedValue = _defaultCallValue;
    }

    /// <summary>
    /// Called when app is closed
    /// </summary>
    public void OnDisable()
    {
        _storedValue = _defaultCallValue;
    }
    
    
    /// <summary>
    /// Raise the event and pass the value to the subscribers
    /// </summary>
    /// <param name="value">The message to pass to the subscribers</param>
    public virtual void Raise(T value)
    {
        
        RemoveNullSubscribers();
        for(int index = _messageSubscribers.Count - 1; index >= 0; index--)
        {
            try
            {
                _messageSubscribers[index].response?.Invoke(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error raising event: {e.Message}");
            }
             
        }
        HasRaised = true;
        _storedValue = value;

    }
    
    
    /// <summary>
    /// Go through the list of subscribers and raise the event. Uses the default value as the parameter
    /// </summary>
    public override void Raise()
    {
        //Remove any null subscribers
        RemoveNullSubscribers();
        for(int index = _messageSubscribers.Count - 1; index >= 0; index--)
        {
            _messageSubscribers[index].response?.Invoke(_defaultCallValue);
        }
        HasRaised = true;
        StoredValue = _defaultCallValue;
    }


    /// <summary>
    /// Remove any subscribers that do not have a valid reference
    /// </summary>
    public override void RemoveNullSubscribers()
    {
        base.RemoveNullSubscribers();
        for (int i = 0; i < _messageSubscribers.Count; i++)
        {
            if (_messageSubscribers[i] ==null)
            {
                _messageSubscribers.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Add a new subscriber to the event callback. Us this when you don't want to pass a message through the event
    /// </summary>
    /// <param name="origin">The origin of the subscriber</param>
    /// <param name="method">The method we are adding</param>
    /// <param name="response">The action to call when the event is raised</param>
    public override void Subscribe(Object origin, string method, UnityAction response)
    {
        ActionMessageSubscriber<T> messageSubscriber = new ActionMessageSubscriber<T>(origin, method, (value) => response());
        _messageSubscribers.Add(messageSubscriber);
    }
    /// <summary>
    /// Add a new subscriber to the event callback
    /// </summary>
    /// <param name="origin">The origin of the subscriber</param>
    /// <param name="method">The method we are adding</param>
    /// <param name="response">The action to call when the event is raised</param>
    public virtual void Subscribe(Object origin, string methodName, UnityAction<T> response)
    {
        ActionMessageSubscriber<T> messageSubscriber = new ActionMessageSubscriber<T>(origin, methodName, response);
        _messageSubscribers.Add(messageSubscriber);
    }
    
    
    /// <summary>
    /// Unsubscribes all functions from the same origin
    /// </summary>
    /// <param name="origin"></param>
    public override void UnsubscribeAll(Object origin)
    {
        //loop through subscribers and remove all subscribers with the same origin
        for(int i = _messageSubscribers.Count - 1; i >= 0; i--)
        {
            if(_messageSubscribers[i].Origin == origin)
            {
                _messageSubscribers[i] = null;
            }
            RemoveNullSubscribers();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="methodName"></param>
    public override void Unsubscribe(Object origin, string methodName)
    {
        for(int index = _messageSubscribers.Count - 1; index >= 0; index--)
        {
            if(_messageSubscribers[index].Origin == origin && _messageSubscribers[index].Origin.name == methodName)
            {
                _messageSubscribers[index] = null;
            }
        }
        
        RemoveNullSubscribers();
    }
    
}

