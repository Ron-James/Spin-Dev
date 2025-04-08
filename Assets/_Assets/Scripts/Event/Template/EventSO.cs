using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CustomSceneReference;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public interface IEvent
{
    void Raise();
    void Subscribe(UnityEngine.Object origin, string methodName, UnityAction response);
    void UnsubscribeAll(UnityEngine.Object origin);
    void Unsubscribe(UnityEngine.Object origin, string methodName);
    
    bool HasRaised {get;}
}
public interface IEvent<T> : IEvent
{
    void Raise(T value);
    
    void Subscribe(UnityEngine.Object origin, string methodName, UnityAction<T> response);
    
}


[Serializable]
public class BaseEventSubscriber : IEquatable<BaseEventSubscriber>
{
    [SerializeField] protected Object _origin;
    [SerializeField, ValueDropdown(nameof(GetValidMethodNames))]
    [OnValueChanged(nameof(UpdateResponse))]
    protected string _methodName;
    [SerializeField] protected UnityAction _response;
    
    /// <summary>
    /// Populates the method dropdown with valid UnityAction-compatible methods.
    /// </summary>
    private IEnumerable<string> GetValidMethodNames()
    {
        if (_origin == null) return Enumerable.Empty<string>();

        return _origin.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
            .Select(m => m.Name);
    }

    /// <summary>
    /// Dynamically generates the UnityAction delegate from method info.
    /// </summary>
    protected virtual void UpdateResponse()
    {
        if (_origin == null || string.IsNullOrEmpty(_methodName))
        {
            _response = null;
            return;
        }

        MethodInfo method = _origin.GetType().GetMethod(_methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method != null && method.GetParameters().Length == 0 && method.ReturnType == typeof(void))
        {
            try
            {
                _response = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), _origin, method);
            }
            catch
            {
                Debug.LogWarning($"Could not bind method '{_methodName}' on {_origin.name}");
                _response = null;
            }
        }
        else
        {
            _response = null;
        }
    }
    
    public UnityAction Response => _response;

    public Object Origin
    {
        get => _origin;
        set => _origin = value;
    }
    
    public void SetNull()
    {
        _origin = null;
        UnityAction response = null;
        _methodName = null;
    }
    


    public BaseEventSubscriber()
    {
        
    }
    
    public BaseEventSubscriber(Object origin, string methodName, UnityAction response)
    {
        Origin = origin;
        _methodName = methodName;
        _response = response;
    }


    public bool Equals(BaseEventSubscriber other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(_origin, other._origin) && _methodName == other._methodName && Equals(_response, other._response);
    }

    
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((BaseEventSubscriber)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_origin, _methodName, _response);
    }
}

[ShowOdinSerializedPropertiesInInspector]
[Serializable]
public class EventSubscriber<T> : BaseEventSubscriber
{
    [OdinSerialize] private new UnityAction<T> _response;
    
    public EventSubscriber(Object origin, string methodName, UnityAction<T> response) 
    {
        base.Origin = origin;
        _methodName = methodName;
        this.response = new(response);
    }

    public UnityAction<T> response
    {
        get => _response;
        protected set => _response = value;
    }
    
    
    public EventSubscriber()
    {
        
    }
    

    public Object Origin => base.Origin;
}

[CreateAssetMenu(fileName = "EventSO", menuName = "Scriptable Objects/EventSO")]
public abstract class BaseEventSO : SerializableScriptableObject, IEvent, ISceneCycleListener
{
    [SerializeReference] protected List<BaseEventSubscriber> _subscribers = new List<BaseEventSubscriber>();
    public void OnSceneStarted(Scene scene)
    {
        Debug.Log("Stopwatch started" + name);
    }

    public virtual void OnSceneStopped(Scene scene)
    {
        
    }

    public virtual void OnEditorStopped()
    {
        _subscribers.Clear();
    }

    
    
    
    [Button]
    public abstract void Raise();


    public abstract void Subscribe(Object origin, string methodName, UnityAction response);


    public abstract void UnsubscribeAll(Object origin);

    public abstract void Unsubscribe(Object origin, string methodName);
    
    [OdinSerialize]
    public bool HasRaised { get; protected set; }
    
    [Button]
    public virtual void RemoveNullSubscribers()
    {
        for(int loop = _subscribers.Count - 1; loop >= 0; loop--)
        {
            if(_subscribers[loop].Origin == null || _subscribers[loop] == null)
            {
                _subscribers.RemoveAt(loop);
            }
        }
    }

    
}



public abstract class EventSO<T> : BaseEventSO, IEvent<T>
{
    [SerializeField] private T _defaultValue;
    [SerializeReference] protected List<EventSubscriber<T>> _messageSubscribers = new();
    
    [ShowInInspector]
    public T LastValueRaised {get; protected set;}

    public override void OnEditorStopped()
    {
        base.OnEditorStopped();
        _messageSubscribers.Clear();
        LastValueRaised = _defaultValue;
    }

    public void OnDisable()
    {
        LastValueRaised = _defaultValue;
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
        LastValueRaised = value;

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
            _messageSubscribers[index].response?.Invoke(_defaultValue);
        }
        HasRaised = true;
        LastValueRaised = _defaultValue;
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
        EventSubscriber<T> subscriber = new EventSubscriber<T>(origin, method, (value) => response());
        _messageSubscribers.Add(subscriber);
    }
    /// <summary>
    /// Add a new subscriber to the event callback
    /// </summary>
    /// <param name="origin">The origin of the subscriber</param>
    /// <param name="method">The method we are adding</param>
    /// <param name="response">The action to call when the event is raised</param>
    public virtual void Subscribe(Object origin, string methodName, UnityAction<T> response)
    {
        EventSubscriber<T> subscriber = new EventSubscriber<T>(origin, methodName, response);
        _messageSubscribers.Add(subscriber);
    }

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

