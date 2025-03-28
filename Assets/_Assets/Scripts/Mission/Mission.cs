using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Mission", menuName = "Mission")]
public class Mission : SerializableScriptableObject, IEnumerable<Objective>
{
    [SerializeField, ReadOnly, Tooltip("Reference to the mission manager which stores ongoing/completed missions")] private MissionDataManager missionDataManager;
    [SerializeField, Tooltip("References to the objectives which the user must satisfy to complete this mission")] private Objective[] objectives = Array.Empty<Objective>();

    
    public void CompleteObjective(Objective objective)
    {
        missionDataManager.CompleteObjective(objective);
        CheckRemainingObjectives();
    }
    
    
    /// <summary>
    /// Check if the remaining objectives are completed
    /// </summary>
    private void CheckRemainingObjectives()
    {
        if(!IsCompleted()) return;
        CompleteMission();

    }


    /// <summary>
    /// Sends a message to the mission manager that this mission has been completed
    /// </summary>
    public virtual void CompleteMission()
    {
        try
        {
            missionDataManager.CompleteMission(this);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    
    /// <summary>
    /// Check is all objectives are completed
    /// </summary>
    /// <returns>True if all objs have been completed</returns>
    public bool IsCompleted()
    {
        foreach (var objective in objectives)
        {
            if (!objective.IsCompleted())
            {
                return false;
            }
        }

        return true;
    }
    
    public void StartObjective(Objective objective)
    {
        missionDataManager.StartObjective(objective);
    }
    
    /// <summary>
    /// Initialize the mission with the mission data manager. Also initializes the objectives.
    /// </summary>
    /// <param name="missionDataManager">The data manager which tracks ongoing missions</param>
    public virtual void Init(MissionDataManager missionDataManager)
    {
        this.missionDataManager = missionDataManager;
        foreach (var objective in objectives)
        {
            objective.Init(this);
        }
    }

    public IEnumerator<Objective> GetEnumerator()
    {
        return objectives.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}