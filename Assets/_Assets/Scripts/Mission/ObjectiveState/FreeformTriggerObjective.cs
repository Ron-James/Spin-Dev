using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "FreeformTriggerEnterObjective", menuName = "Objectives/Freeform Trigger Objective")]
public class FreeformTriggerObjective : Objective
{
    
    [SerializeField, Tooltip("The runtime set of colliders we want to listen for")] private ColliderRuntimeSet triggerSet;
    [SerializeField, Tooltip("The collider event channel which sends the collision events relevant to this obj.")] private ColliderChannel channel;
    [SerializeField, Tooltip("The tag of the collider which is looked for when collisions occur. Generally we check to see if the player has entered.")] 
    private string otherTag = "Player";
    
    [SerializeField, Tooltip("If we want to disable the colliders after entering them")] bool disableTriggersOnEnter = false;
    
    
    [ShowInInspector]
    public bool HasEnteredAllObjectiveTriggers => HasEnteredAllTriggers();
    
    [Title("Tracked Collisions")]
    [SerializeReference, ReadOnly, Tooltip("These are the collisions we are currently tracking")] private CollisionEnteredData[] trackingCollisions;

    public override void OnSceneStarted(Scene scene)
    {
        base.OnSceneStarted(scene);
        
        //if the necessary trigger is not present, deactivate the objective
        if (!IsActive) return;
        InitializeTrackingCollisions();
    }

    public override bool ValidateContext()
    {
        //Check length of triggerSet and make sure we are in progress
        return triggerSet.Any() && base.ValidateContext();
    }

    public override void Init(Mission mission)
    {
        base.Init(mission);
        InitializeTrackingCollisions();
    }


    public override void OnSceneStopped(Scene scene)
    {
        base.OnSceneStopped(scene);
        //Dispose events we subscribed to at the beginning of the scene
        SceneDispose();
    }

    /// <summary>
    /// Initialize the tracking collisions array with the triggers from the runtime set
    /// </summary>
    private void InitializeTrackingCollisions()
    {
        trackingCollisions = null;
        trackingCollisions = new CollisionEnteredData[triggerSet.Count()];
        for (int i = 0; i < triggerSet.Count(); i++)
        {
            trackingCollisions[i] = new CollisionEnteredData(triggerSet.ElementAt(i), null);
        }
        
        channel.OnTriggerEnter.Subscribe(this, "OnTriggerEnterMessage", OnTriggerEnterMessage);
        
    }
    
    
    
    /// <summary>
    /// Dispose of scene based dependencies
    /// </summary>
    [Button]
    private void SceneDispose()
    {
        
        trackingCollisions = Array.Empty<CollisionEnteredData>();
        if(!IsActive) return;
        channel.OnTriggerEnter.UnsubscribeAll(this);
    }
    
    /// <summary>
    /// Handled when the collider channel has sent a message that a collider enter event has been fired.
    /// Checks if the collider is part of the trigger set and if the other collider is the one we are looking for.
    /// </summary>
    /// <param name="data"></param>
    private void OnTriggerEnterMessage(CollisionData data)
    {
        //If the Main Trigger of the collision entry is not part of our trigger set or we are not currently ongoing, we ignore it
        if (!triggerSet.Contains(data.Trigger) || !IsActive) return;
        
        //If the other collider is not the one we are looking for, we ignore it
        if(!data.Other.CompareTag(otherTag)) return;
        
        CollisionEnteredData trackedData = trackingCollisions.ToList().Find(x => x.Trigger == data.Trigger);
        trackedData.Enter();
        trackedData.Other = data.Other;
        
        if(disableTriggersOnEnter) data.Trigger.gameObject.SetActive(false);
        
        
        //if all triggers have been entered, we complete the objective
        if (HasEnteredAllTriggers())
        {
            Complete();
        }
    }

    
    /// <summary>
    /// returns true if all triggers have been entered
    /// </summary>
    /// <returns></returns>
    private bool HasEnteredAllTriggers()
    {
        if (!IsActive) return false;
        return trackingCollisions.All(x => x.HasEntered);
    }
    
    
    
    
    
}


