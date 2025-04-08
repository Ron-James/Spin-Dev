using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "EventTriggeredObjective", menuName = "Objectives/EventTriggeredObjective")]
public class EventTriggeredMissionObjective : MissionObjective
{
    [SerializeField] private MessageListenerSet _messageListeners;
    
    

    public override void OnSceneStopped(Scene scene)
    {
        base.OnSceneStopped(scene);
        _messageListeners.Clear();
    }


    
    
    
}








