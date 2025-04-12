using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "ColliderChannel", menuName = "Channels/ColliderChannel")]
public class ColliderChannel : SerializableScriptableObject
{
    [SerializeField] private ActionReference<CollisionData> OnTriggerEnterAction = new();
    [SerializeField] private ActionReference<CollisionData> OnTriggerExitAction = new();

    public ActionSO<CollisionData> OnTriggerExit => OnTriggerExitAction.ActionCasted;
    public ActionSO<CollisionData> OnTriggerEnter => OnTriggerEnterAction.ActionCasted;

    public void TriggerEntered(ColliderTrigger other)
    {
        OnTriggerEnterAction.Value = new CollisionData(other);
    }
    
    public void TriggerExited(ColliderTrigger other)
    {
        OnTriggerExitAction.Value = new CollisionData(other);
    }
    
    
    
}