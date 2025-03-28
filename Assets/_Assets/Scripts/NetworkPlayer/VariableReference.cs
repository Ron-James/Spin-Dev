using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class VariableReference<T>
{
    [SerializeField, ShowIf("_isConstant")] protected EventSO<T> _event;
    [SerializeField] private bool _isConstant = true;
    [SerializeField, HideIf("_isConstant")] protected T _constantValue;
    
    
    
    public EventSO<T> Event => _event;
    
    private bool IsValid()
    {
        return _event != null || !_isConstant;
    }
    
    [ShowInInspector, ShowIf("_isConstant")]
    public T CurrentValue
    {
        get
        {
            try
            {
                return _isConstant && IsValid() ? _event.LastValueRaised : _constantValue ;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return _constantValue;
            }
            
        }
        set
        {
            _constantValue = value;
            _event.Raise(value);
        }
    }
    
    public EventSO<T> GetEvent()
    {
        return _event;
    }
    
    public void SetEvent(EventSO<T> eventSO)
    {
        _event = eventSO;
    }
    
    
}