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
                if(mono is IMessageListener && mono is UnityEngine.Object obj)
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
                    _runtimeSet.Add(item);
                }
                else
                {
                    _runtimeSet.Remove(item);
                }
            }
        }
    }
    public Task Init()
    {
        
        return Task.CompletedTask;
    }
    
}