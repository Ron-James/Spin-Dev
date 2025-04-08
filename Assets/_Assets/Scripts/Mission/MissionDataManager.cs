using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "MissionDataManager", menuName = "Data Managers/MissionDataManager")]
public class MissionDataManager : SaveableSO<MissionDataManager>, IInitializable, ISceneCycleListener
{
    [SerializeField, ReadOnly, Save] List<ScriptableObjectReference<MissionObjective>> _ongoingMissions = new();
    [SerializeField, ReadOnly, Save] List<ScriptableObjectReference<MissionObjective>> _completedMissions = new();


    [Title("Last Completed/Started Variable References")] [SerializeField]
    private VariableReference<MissionObjective> lastCompletedObjectiveVR;

    [SerializeField] private VariableReference<MissionObjective> lastStartedObjectiveVR;
    public List<ScriptableObjectReference<MissionObjective>> OngoingMissions => _ongoingMissions;


    /// <summary>
    /// Send the message that the mission has been completed and move it to the completed missions list
    /// </summary>
    /// <param name="mission">The mission which was completed</param>
    public void CompleteObjective(MissionObjective missionObjective)
    {
        ScriptableObjectReference<MissionObjective> mission = _ongoingMissions.FirstOrDefault(x => x.Value == missionObjective);
        
        try
        {
            _ongoingMissions.Remove(mission);
            _completedMissions.Add(mission);
            lastCompletedObjectiveVR.CurrentValue = missionObjective;
            
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        
    }


    /// <summary>
    /// Add a new mission to the ongoing missions list. Initialize the mission and set the last started mission variable reference
    /// </summary>
    /// <param name="mission">The mission which was started</param>
    public void AddMission(MissionObjective mission)
    {
        ScriptableObjectReference<MissionObjective> missionObjectReference = new(mission);
        if (!OngoingMissions.Contains(missionObjectReference))
        {
            OngoingMissions.Add(missionObjectReference);
            mission.Init(this);
            lastStartedObjectiveVR.CurrentValue = mission;
        }
    }
    

    /// <summary>
    /// Initialise the objectives array. Other initializations can go here.
    /// </summary>
    /// <returns></returns>
    public Task Init()
    {
        return Task.CompletedTask;
    }

    public void OnSceneStarted(Scene scene)
    {
    }

    public void OnSceneStopped(Scene scene)
    {
    }

    public void OnEditorStopped()
    {
        OngoingMissions.Clear();
        _completedMissions.Clear();
    }

    public string GetUniqueIdentifier()
    {
        return Guid;
    }


    

    public override void OnSave()
    {
    }

    public override void OnLoad()
    {
        foreach (var ongoing in _ongoingMissions)
        {
            //Initialize the mission to set in progress
            ongoing.Value.Init(this);
        }
        
        foreach (var completed in _completedMissions)
        {
            //Mark as completed
            completed.Value.MarkAsCompleted();
        }
    }
}

