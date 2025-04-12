using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
[CreateAssetMenu(fileName = "Mission Objective", menuName = "Mission Objective")]
public class MissionObjective : SaveableSO<MissionObjective>, ISceneCycleListener
{
    public enum Progress
    {
        Locked,
        InProgress,
        Finished
    }
    
    
    [SerializeField, Tooltip("The delay before notifying the complete in seconds.")] private float _delay;
    [SerializeReference, ReadOnly] protected MissionDataManager dataManager;
    [SerializeField, ReadOnly, Save, Tooltip("The current state of the objectives progress")] protected Progress progress = Progress.Locked;

    
    [SerializeReference, Save] private List<BaseActionObjective> messageRequirements = new();

    [SerializeField] private UnityEvent OnComplete;
    public Progress CurrentProgress => progress;
    
    public void SetProgress(Progress value)
    {
        progress = value;
    }
    
    
    /// <summary>
    /// Call when the cconditions have been met to complete the objective. Sets the progress state to completed and notifies it's attatched mission.
    /// </summary>
    [Button]
    public virtual void Complete()
    {
        DisposeActions();
        DelayedActionCmd delayedActionCmd = new DelayedActionCmd("Complete " + name,CompleteMessage, _delay);
        CommandRunner.ExecuteCommand(delayedActionCmd);
        void CompleteMessage()
        {
            try
            {
                progress = Progress.Finished;
                dataManager.CompleteObjective(this);
                OnComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }
    }
    
   /// <summary>
   /// Initialize the objective with the mission it is attached to. This generally gets called when the mission is started or setup in the players missions log.
   /// </summary>
   /// <param name="mission">Mission manager to track ongoing missions</param>
    public virtual void Init(MissionDataManager mission)
    {
        dataManager = mission;
        progress = Progress.InProgress;
        SetupActions();
    }
   
    /// <summary>
    /// Checks if all necessary Actions have been fullfilled. If so, it will call the complete function.
    /// </summary>
    protected virtual void CheckRemainingObjectives()
    {
        int count = 0;
        foreach (var messageListener in messageRequirements)
        {
            if(messageListener.HasCompleteAction)
            {
                count++;
                
            }
        }
        
        if (count == messageRequirements.Count)
        {
            Complete();
        }
    }
    
   
    
   
   /// <summary>
   /// Check if the objective is in progress
   /// </summary>
   /// <returns>True if in progress</returns>
    public bool IsInProgress()
    {
        return CurrentProgress == Progress.InProgress;
    }
   
    
    /// <summary>
    /// Scene start callback, called when the scene is started. Checks if the objective is active and if it is locked, subscribes to the last completed objective event.
    /// </summary>
    /// <param name="scene"></param>
    public virtual void OnSceneStarted(Scene scene)
    {

    }

    public virtual void OnSceneStopped(Scene scene)
    {
        
    }
    
    
    
    /// <summary>
    /// function called when the editor is stopped. Disposes of any editor based dependencies. SOs are not reset upon exiting play mode.
    /// </summary>
    public virtual void OnEditorStopped()
    {
        progress = Progress.Locked;
        dataManager = null;
        DisposeActions();
        ResetEverything();
        progress = Progress.Locked;
        
    }
    
    
    private void SetupActions()
    {
        foreach (var action in messageRequirements)
        {
            action.Setup(this, nameof(CheckRemainingObjectives), CheckRemainingObjectives);
        }
    }
    
    private void DisposeActions()
    {
        foreach (var action in messageRequirements)
        {
            action.Dispose(this, nameof(CheckRemainingObjectives));
        }
    }
    
    
    
    /// <summary>
    /// Resets the objective to the initial state. This is used when the player fails the mission or when the mission is reset.
    /// </summary>
    [Button]
    public virtual void ResetEverything()
    {
        foreach (var action in messageRequirements)
        {
            action.Reset();
            if (progress == Progress.Locked)
            {
                return;
            }
            progress = Progress.InProgress;
        }
    }
    
    
    /// <summary>
    /// For when loading completed objectives from save data. This is used to mark the objective as completed when loading the game.
    /// </summary>
    public void MarkAsCompleted()
    {
        progress = Progress.Finished;
        foreach (var action in messageRequirements)
        {
            action.HasCompleteAction = true;
        }
    }
    
    /// <summary>
    /// Check if the objective is completed
    /// </summary>
    /// <returns>True if the objective is complete</returns>
    public bool IsCompleted()
    {
        return CurrentProgress == Progress.Finished;
    }
    
    
    /// <summary>
    /// Check if the objective is locked
    /// </summary>
    /// <returns></returns>
    public bool IsLocked()
    {
        return CurrentProgress == Progress.Locked;
    }


    public override void OnSave()
    {
        
    }

    public override void OnLoad()
    {
        
    }
}





