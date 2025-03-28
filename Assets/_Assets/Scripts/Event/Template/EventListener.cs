using UnityEngine;
using UnityEngine.Events;

public class EventListener<T> : MonoBehaviour
{
    [SerializeField] protected string _methodName = "Response";
    [SerializeField] private EventSO<T> _event;
    [SerializeField] private UnityEvent<T> _response;
    
    private void OnEnable()
    {
        _event.Subscribe(this, name, OnEventRaised);
    }

    private void OnDisable()
    {
        _event.Unsubscribe(this, name);
    }
    
    
    public virtual void OnEventRaised(T value)
    {
        _response.Invoke(value);
    }
}