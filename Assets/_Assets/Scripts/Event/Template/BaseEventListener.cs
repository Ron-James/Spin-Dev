using System;
using System.Threading.Tasks;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.Events;

public class BaseEventListener : MonoBehaviour, IInitializable
{
    [SerializeField] private BaseEventSO _event;
    [SerializeField] private UnityEvent _response;


    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        _event.UnsubscribeAll(this);
    }

    public virtual void OnEventRaised()
    {
        _response.Invoke();
    }

    public Task Init()
    {
        _event.Subscribe(this, nameof(OnEventRaised), OnEventRaised);
        return Task.CompletedTask;
    }
}