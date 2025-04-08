using System;
using System.Threading.Tasks;
using UnityEngine;

public class MessageListenerSetInitializer : MonoBehaviour, IInitializable
{
    [SerializeField] MessageListenerSet _runtimeSet;
    [SerializeField] private InterfaceReference<IMessageListener>[] _itemsToAdd = Array.Empty<InterfaceReference<IMessageListener>>();
    [SerializeField] bool _useChildrenComponents = false;


    private void RegisterItems(bool add = true)
    {
        if (_useChildrenComponents)
        {
            foreach (var mono in GetComponentsInChildren<MonoBehaviour>(true))
            {
                if(mono is IMessageListener)
                {
                    if (add)
                    {
                        _runtimeSet.Add(mono);
                    }
                    else
                    {
                        _runtimeSet.Remove(mono);
                    }
                }
            }
        }
        else
        {
            foreach (var item in _itemsToAdd)
            {
                if (add)
                {
                    _runtimeSet.Add(item.UnderlyingValue);
                }
                else
                {
                    _runtimeSet.Remove(item.UnderlyingValue);
                }
            }
        }
    }
    public Task Init()
    {
        RegisterItems(true);
        return Task.CompletedTask;
    }

    private void OnDisable()
    {
        RegisterItems(false);
    }
}