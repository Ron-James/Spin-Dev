using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "EventTriggeredObjective", menuName = "Objectives/EventTriggeredObjective")]
public class EventTriggeredObjective : Objective
{
    [SerializeField] private MessageListenerSet _messageListeners;
    
    

    public override void OnSceneStopped(Scene scene)
    {
        base.OnSceneStopped(scene);
        _messageListeners.Clear();
    }


    
    
    
}


public class MessageListenerData
{
    InterfaceReference<IMessageListener> _messageListener;
    bool _hasReceivedMessage = false;


    public event Action OnMessageReceived;
        
    public MessageListenerData(InterfaceReference<IMessageListener> messageListener)
    {
        _messageListener = messageListener;
        _messageListener.Value.AddListener(MessageReceived);
    }
        
    private void MessageReceived()
    {
        _hasReceivedMessage = true;
    }
        
    public void Dispose()
    {
        _messageListener.Value.RemoveListener(MessageReceived);
    }
}





