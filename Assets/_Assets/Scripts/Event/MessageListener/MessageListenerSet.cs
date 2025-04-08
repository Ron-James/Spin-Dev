using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageListener", menuName = "Runtime Sets/MessageListener")]
public class MessageListenerSet : RuntimeSet<IMessageListener>
{

    [SerializeField] private InterfaceReference<IMessageListener> test;
    [Button]
    public void Add(UnityEngine.Object obj)
    {
        IMessageListener listener = obj.GetComponent<IMessageListener>();
        if(listener != null)
        {
            Add(listener);
        }
        else
        {
            throw new System.InvalidCastException("Object does not implement IMessageListener");
        }
    }
    
    [Button]
    public void Remove(UnityEngine.Object obj)
    {
        IMessageListener listener = obj.GetComponent<IMessageListener>();
        bool isInSet = items.Exists(listener.Equals);
        if(isInSet)
        {
            items.RemoveAll(x => listener.Equals(x));
        }
        else
        {
            throw new System.InvalidOperationException("Object is not in the set");
        }
    }
}