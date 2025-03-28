using Sirenix.OdinInspector;
using UnityEngine;

public class MissionVendor : MonoBehaviour
{
    [SerializeField] private MissionDataManager _missionDataManager;
    [SerializeField] Mission _mission;
    
    
    [Button]
    public void StartMission()
    {
        _missionDataManager.AddMission(_mission);
    }
    
}
