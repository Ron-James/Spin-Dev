using UnityEngine;
using UnityEngine.Events;

public class ActionListener<T> : MonoBehaviour
{
    [SerializeField] private ActionSO<T> action;
    [SerializeField] private UnityEvent<T> _response;
    
    private void OnEnable()
    {
        action.Subscribe(this, nameof(OnEventRaised), OnEventRaised);
    }

    private void OnDisable()
    {
        action.Unsubscribe(this, nameof(OnEventRaised));
    }
    
    
    public virtual void OnEventRaised(T value)
    {
        _response?.Invoke(value);
    }
}