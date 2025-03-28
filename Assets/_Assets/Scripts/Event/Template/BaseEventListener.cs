using System;
using System.Threading.Tasks;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.Events;

public class BaseEventListener : MonoBehaviour
{
    [SerializeField] string _methodName = "Response";
    [SerializeField] private BaseEventSO _event;
    [SerializeField] private UnityEvent _response;


    private void OnEnable()
    {
        _event.Subscribe(this, _methodName, OnEventRaised);
    }

    private void OnDisable()
    {
        _event.UnsubscribeAll(this);
    }

    public virtual void OnEventRaised()
    {
        _response.Invoke();
    }
}