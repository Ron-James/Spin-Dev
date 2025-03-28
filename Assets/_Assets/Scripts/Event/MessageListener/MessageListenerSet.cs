using UnityEngine;

[CreateAssetMenu(fileName = "MessageListener", menuName = "Runtime Sets/MessageListener")]
public class MessageListenerSet : RuntimeSet<InterfaceReference<IMessageListener>>
{
    public void Add(UnityEngine.Object obj)
    {
        if(obj is IMessageListener listener)
        {
            InterfaceReference<IMessageListener> reference = new InterfaceReference<IMessageListener>();
            reference.Value = listener;
            reference.UnderlyingValue = obj;
            Add(reference);
        }
        else
        {
            throw new System.InvalidCastException("Object does not implement IMessageListener");
        }
    }
    
    
    public void Remove(UnityEngine.Object obj)
    {
        bool isInSet = items.Exists(x => x.UnderlyingValue == obj);
        if(isInSet)
        {
            items.RemoveAll(x => x.UnderlyingValue == obj);
        }
        else
        {
            throw new System.InvalidOperationException("Object is not in the set");
        }
    }
}