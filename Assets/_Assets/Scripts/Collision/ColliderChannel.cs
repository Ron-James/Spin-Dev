using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "ColliderChannel", menuName = "Channels/ColliderChannel")]
public class ColliderChannel : SerializableScriptableObject
{
    [SerializeField] private VariableReference<CollisionData> OnTriggerEnterVR;
    [SerializeField] private VariableReference<CollisionData> OnTriggerExitVR;

    public EventSO<CollisionData> OnTriggerExit => OnTriggerExitVR.Event;
    public EventSO<CollisionData> OnTriggerEnter => OnTriggerEnterVR.Event;

    public void TriggerEntered(ColliderTrigger other)
    {
        OnTriggerEnterVR.CurrentValue = new CollisionData(other);
    }
    
    public void TriggerExited(ColliderTrigger other)
    {
        OnTriggerExitVR.CurrentValue = new CollisionData(other);
    }
    
    
    
}