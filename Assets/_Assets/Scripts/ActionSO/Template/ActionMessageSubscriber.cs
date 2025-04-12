using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[ShowOdinSerializedPropertiesInInspector]
[Serializable]
public class ActionMessageSubscriber<T> : BaseActionSubscriber
{
    [OdinSerialize] private new UnityAction<T> _response;
    
    public ActionMessageSubscriber(Object origin, string methodName, UnityAction<T> response) 
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
    
    
    public ActionMessageSubscriber()
    {
        
    }
    

    public Object Origin => base.Origin;
}