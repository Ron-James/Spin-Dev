using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public interface IActivatable
{
    bool IsActive { get; }
    void SetActive(bool value);
}
[CreateAssetMenu(fileName = "Objective", menuName = "Scriptable Objects/Objective")]
public abstract class Objective : SerializableScriptableObject, ISceneCycleListener, IActivatable
{
    
    [SerializeReference, ReadOnly] protected Mission attachedMission;
    

    [SerializeField, ReadOnly, Tooltip("The current state of the objectives progress")] protected ObjectiveProgressState progressState = null;
    [OdinSerialize, ReadOnly, Tooltip("True if the objective has prerequisites")] public bool startLocked => prerequisites.Length > 0;
    
    
    [SerializeField, Tooltip("The objectives which the user must complete to unlock this one.")] private Objective[] prerequisites = Array.Empty<Objective>();
    
    
    [SerializeField, Tooltip("Variable reference to the last completed Obj. This should autopopulate to the correct event.")] VariableReference<Objective> lastCompletedObjectiveVR;
    [ShowInInspector, ReadOnly, Tooltip("Whether the objective is active or not in this current context.")]
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Use to detemine if the objective should be active in the current context.
    /// </summary>
    /// <returns>True if it has an attatched mission and is in progress</returns>
    public virtual bool ValidateContext()
    {
        return attachedMission != null && IsInProgress();
    }

    /// <summary>
    /// Getter/Setter for the progress state of the objective
    /// </summary>
    public ObjectiveProgressState ProgressState
    {
        get => progressState;
        set => progressState = value;
    }


    public void OnEnable()
    {
        if (lastCompletedObjectiveVR.GetEvent() == null)
        {
            //If the event is null, load the event from the resources folder
            ObjectiveEvent objectiveEvent = Resources.Load<ObjectiveEvent>("Event/Objective/OnObjectiveCompleted");
            lastCompletedObjectiveVR.SetEvent(objectiveEvent);
        }
    }
    
    
    /// <summary>
    /// Call when the cconditions have been met to complete the objective. Sets the progress state to completed and notifies it's attatched mission.
    /// </summary>
    [Button]
    public virtual void Complete()
    {
        try
        {
            ProgressState = ScriptableObjectManager.GetFirstAssetOfType<CompletedObjective>();
            attachedMission.CompleteObjective(this);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
   /// <summary>
   /// Initialize the objective with the mission it is attached to. This generally gets called when the mission is started or setup in the players missions log.
   /// </summary>
   /// <param name="mission"></param>
    public virtual void Init(Mission mission)
    {
        attachedMission = mission;
        if (startLocked)
        {
            IsActive = false;
            ProgressState = ScriptableObjectManager.GetFirstAssetOfType<LockedObjective>();
        }
        else
        {
            ProgressState = ScriptableObjectManager.GetFirstAssetOfType<InProgressObjective>();
            IsActive = true;
            attachedMission.StartObjective(this);
            
        }
    }
    
   
   
   /// <summary>
   /// Call to unlock a locked mission.
   /// </summary>
   /// <exception cref="NullReferenceException">Thrown if objective has not been initialised</exception>
   /// <exception cref="InvalidOperationException">Thrown if objective is not locked</exception>
    [Button]
    public virtual void Unlock()
    {
        if (progressState == null || attachedMission == null)
        {
            throw new NullReferenceException("Cannot unlock objective with null progress state or mission");
        }
        if(ProgressState.GetType() != typeof(LockedObjective))
        {
            throw new InvalidOperationException("Objective is not locked");
        }
        
        try
        {
            
            ProgressState = ScriptableObjectManager.GetFirstAssetOfType<InProgressObjective>();
            attachedMission.StartObjective(this);
            IsActive = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
   
   /// <summary>
   /// Check if the objective is in progress
   /// </summary>
   /// <returns>True if in progress</returns>
    public bool IsInProgress()
    {
        return ProgressState.GetType() == typeof(InProgressObjective);
    }
    
   
    /// <summary>
    /// Update function, called every frame when the objective is in progress. Not implemented yet.
    /// </summary>
    public virtual void Update()
    {
       if(ProgressState.GetType() != typeof(InProgressObjective)) return;
    }
    
    
    /// <summary>
    /// Dispose of anything neccessary when the application is closed.
    /// </summary>
    public virtual void OnDisable()
    {
        lastCompletedObjectiveVR.GetEvent().UnsubscribeAll(this);
    }
    
    
    /// <summary>
    /// Scene start callback, called when the scene is started. Checks if the objective is active and if it is locked, subscribes to the last completed objective event.
    /// </summary>
    /// <param name="scene"></param>
    public virtual void OnSceneStarted(Scene scene)
    {
        //Check if the objective is active in this context
        IsActive = ValidateContext();
        //If the objective is locked, subscribe to the last completed objective event
        if(startLocked) lastCompletedObjectiveVR.GetEvent().Subscribe(this, "Check Prereqs",OnObjectiveCompleted);
    }

    public virtual void OnSceneStopped(Scene scene)
    {
        IsActive = false;
        
    }
    
    
    /// <summary>
    /// function called when the editor is stopped. Disposes of any editor based dependencies. SOs are not reset upon exiting play mode.
    /// </summary>
    public virtual void OnEditorStopped()
    {
        IsActive = false;
        progressState = null;
        attachedMission = null;
        lastCompletedObjectiveVR.GetEvent().UnsubscribeAll(this);
        
    }
    
    
    /// <summary>
    /// Check if the objective is completed
    /// </summary>
    /// <returns>True if the objective is complete</returns>
    public bool IsCompleted()
    {
        return ProgressState.GetType() == typeof(CompletedObjective);
    }
    
    
    
    public virtual void SetActive(bool value)
    {
        IsActive = value;
    }
    
    
    /// <summary>
    /// Check if the objective is locked
    /// </summary>
    /// <returns></returns>
    public bool IsLocked()
    {
        return ProgressState.GetType() == typeof(LockedObjective);
    }
    
    /// <summary>
    /// Function which is when any objective is completed. Checks if the objective is locked and if the prerequisites are completed. If they are, the objective is unlocked.
    /// </summary>
    /// <param name="objective"></param>
    public virtual void OnObjectiveCompleted(Objective objective)
    {
        if (IsLocked() && ValidatePrerequisites())
        {
            Unlock();
        }
    }
    
    
    /// <summary>
    /// Check if prerequisites objectives are completed.
    /// </summary>
    /// <returns>True if all prerequesits have been marked as complete</returns>
    public bool ValidatePrerequisites()
    {
        if(!IsLocked()) return true;
        foreach (var prerequisite in prerequisites)
        {
            if (!prerequisite.IsCompleted())
            {
                return false;
            }
        }

        return true;
    }
}