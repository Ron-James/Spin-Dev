using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "VoidEventSO", menuName = "Custom Events/VoidEventSO")]
public class VoidActionSo : BaseActionSO
{
    [SerializeReference] private List<BaseActionSubscriber> _subscribers = new List<BaseActionSubscriber>();
    
    public override void Raise()
    {
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            _subscribers[index].Response?.Invoke();
        }
        HasRaised = true;
        
    }

    public override void Subscribe(Object origin, string methodName, UnityAction response)
    {
        BaseActionSubscriber subscriber = new BaseActionSubscriber(origin, methodName, response);
        _subscribers.Add(subscriber);
    }

    public override void UnsubscribeAll(Object origin)
    {
        for(int loop = _subscribers.Count - 1; loop >= 0; loop--)
        {
            if(_subscribers[loop].Origin == origin)
            {
                _subscribers[loop] = null;
            }
        }
    }

    public override void Unsubscribe(Object origin, string methodName)
    {
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            if(_subscribers[index].Origin == origin && _subscribers[index].Origin.name == methodName)
            {
                _subscribers[index] = null;
            }
        }
    }
}