using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "MissionDataManager", menuName = "Data Managers/MissionDataManager")]
public class MissionDataManager : SerializableScriptableObject, IEnumerable<Mission>, IInitializable, ISceneCycleListener, ISaveable
{
    [SerializeField, ReadOnly] List<Mission> _ongoingMissions = new();
    [SerializeField, ReadOnly] List<Mission> _completedMissions = new();
    
    
    [Title("Last Completed/Started Variable References")]
    [SerializeField] private  VariableReference<Objective> lastCompletedObjectiveVR;
    [SerializeField] private VariableReference<Objective> lastStartedObjectiveVR;
    [SerializeField] private VariableReference<Mission> lastStartedMissionVR;
    [SerializeField] private VariableReference<Mission> lastCompletedMissionVR;
    
    
    
    [SerializeField] private Objective[] allObjectives;

    public List<Mission> CompletedMissions
    {
        get => _completedMissions;
        set => _completedMissions = value;
    }

    public List<Mission> OngoingMissions => _ongoingMissions;


    public IEnumerator<Mission> GetEnumerator()
    {
        return OngoingMissions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    
    /// <summary>
    /// Send a message when an objective is completed
    /// </summary>
    /// <param name="objective">The objective which was completed</param>
    public void CompleteObjective(Objective objective)
    {
        lastCompletedObjectiveVR.CurrentValue = objective;
    }
    
    
    /// <summary>
    /// Send a message when an objective is started
    /// </summary>
    /// <param name="objective">The objective which was started</param>
    public void StartObjective(Objective objective)
    {
        lastStartedObjectiveVR.CurrentValue = objective;
    }
    
    
    
    /// <summary>
    /// Add a new mission to the ongoing missions list. Initialize the mission and set the last started mission variable reference
    /// </summary>
    /// <param name="mission">The mission which was started</param>
    public void AddMission(Mission mission)
    {
        if (!OngoingMissions.Contains(mission))
        {
            OngoingMissions.Add(mission);
            mission.Init(this);
            lastStartedMissionVR.CurrentValue = mission;
        }
    }
    
    
    
    /// <summary>
    /// Send the message that the mission has been completed and move it to the completed missions list
    /// </summary>
    /// <param name="mission">The mission which was completed</param>
    public void CompleteMission(Mission mission)
    {
        if (OngoingMissions.Contains(mission))
        {
            CompletedMissions.Add(mission);
            OngoingMissions.Remove(mission);
            lastCompletedMissionVR.CurrentValue = mission;
        }
    }

    /// <summary>
    /// Initialise the objectives array. Other initializations can go here.
    /// </summary>
    /// <returns></returns>
    public Task Init()
    {
        allObjectives = ScriptableObjectManager.GetAssetsByType<Objective>();
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
        CompletedMissions.Clear();
    }

    public string GetUniqueIdentifier()
    {
        return Guid;
    }
    
    
    [Button]
    public SaveDataContainer GetSaveData()
    {
        return new MissionSaveData(this);
    }

    public void LoadSaveData(SaveDataContainer data)
    {
        try
        {
            MissionSaveData missionData = (MissionSaveData) data;
            missionData.ApplyTo(this);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
        
    }

    public void OnSave()
    {
        
    }

    public void OnLoad()
    {
        
    }
}



/// <summary>
/// SaveData class to store mission data. Currently stores the ongoing and completed missions
/// </summary>
[Serializable]
public class MissionSaveData : SaveDataContainer
{
    
    //Lists contatining data for ongoing and completed missions
    [SerializeReference] List<MissionData> ongoignMissionData = new();
    [SerializeReference] List<MissionData> completedMissionData = new();
    
    
    
    public MissionSaveData()
    {
        
    }
    public MissionSaveData(MissionDataManager missionDataManager)
    {
        foreach (Mission mission in missionDataManager.OngoingMissions)
        {
            ongoignMissionData.Add(new MissionData(mission));
        }
        
        foreach (Mission mission in missionDataManager.CompletedMissions)
        {
            completedMissionData.Add(new MissionData(mission));
        }
    }
    
    //Apply the data to the mission data manager
    public void ApplyTo(MissionDataManager missionDataManager)
    {
        foreach (var data in ongoignMissionData)
        {
            Mission mission = data.Mission;
            //Add the mission to the ongoing missions list from save date
            missionDataManager.AddMission(mission);
            data.Apply();
        }
        
        foreach (var data in completedMissionData)
        {
            Mission mission = data.Mission;
            //Complete the mission from save data
            missionDataManager.CompleteMission(mission);
            data.Apply();
        }
    }
}


/// <summary>
/// Helper class to store objective data, stores the objective and it's progress state as a reference
/// </summary>
[Serializable]
public class ObjectiveData
{
    [SerializeField] ScriptableObjectReference<Objective> objective;
    [SerializeField] ScriptableObjectReference<ObjectiveProgressState> progressState;
        
    
    
    public ObjectiveData(){}
    public ObjectiveData(Objective objective)
    {
        //Get the references to the objective and it's progress state
        this.objective = new ScriptableObjectReference<Objective>(objective);
        if (objective.ProgressState != null)
        {
            progressState = new ScriptableObjectReference<ObjectiveProgressState>(objective.ProgressState);
        }
        
    }
    
    
    /// <summary>
    /// Apply the data to the objective we stored in the constructor
    /// </summary>
    public void Apply()
    {
        objective.Value.ProgressState = progressState.Value;
    }

    
}

/// <summary>
/// Helper class to store mission data, stores the mission and a list of objective data
/// </summary>
[Serializable]
public class MissionData 
{
    [SerializeField] ScriptableObjectReference<Mission> mission;
    
    //Data for the missions objectives
    [SerializeField] List<ObjectiveData> objectivesData = new();
    
    
    public Mission Mission => mission.Value;
    
    
    public MissionData(){}
    public MissionData(Mission mission)
    {
        //Get the reference to the mission and store the objective data
        this.mission = new ScriptableObjectReference<Mission>(mission);
        foreach (var objective in mission)
        {
            ObjectiveData objData = new(objective);
            objectivesData.Add(objData);
        }
    }
    
    /// <summary>
    /// Apply the data to the mission we stored in the constructor
    /// </summary>
    public void Apply()
    {
        foreach (var objectiveData in objectivesData)
        {
            objectiveData.Apply();
        }
    }
}


