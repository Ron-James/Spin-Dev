using Sirenix.OdinInspector;
using UnityEngine;

public class MissionVendor : MonoBehaviour
{
    [SerializeField] MissionObjective objective;
    [SerializeField] MissionDataManager dataManager;
    
    
    [Button]
    public void StartMission()
    {
        dataManager.AddMission(objective);
    }
    
}
