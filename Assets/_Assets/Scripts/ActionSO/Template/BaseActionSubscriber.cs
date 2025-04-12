using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[Serializable]
public class BaseActionSubscriber : IEquatable<BaseActionSubscriber>
{
    [SerializeField] protected Object _origin;
    [SerializeField, ValueDropdown(nameof(GetValidMethodNames))]
    [OnValueChanged(nameof(UpdateMethodName))]
    protected string _methodName;
    protected UnityAction _response;
    
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
    protected virtual void UpdateMethodName()
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
    

    /// <summary>
    /// Empty constructor for serialization purposes.
    /// </summary>
    public BaseActionSubscriber()
    {
        
    }
    
    
    public BaseActionSubscriber(Object origin, string methodName, UnityAction response)
    {
        Origin = origin;
        _methodName = methodName;
        _response = response;
    }


    public bool Equals(BaseActionSubscriber other)
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
        return Equals((BaseActionSubscriber)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_origin, _methodName, _response);
    }
}