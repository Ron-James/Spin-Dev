using System;
using System.Threading.Tasks;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class BaseActionListener : MonoBehaviour, IInitializable
{
    [SerializeField] private BaseActionSO action;
    [SerializeField] private UnityEvent _response;


    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        action.UnsubscribeAll(this);
    }

    public virtual void OnEventRaised()
    {
        _response.Invoke();
    }

    public Task Init()
    {
        action.Subscribe(this, nameof(OnEventRaised), OnEventRaised);
        return Task.CompletedTask;
    }
}