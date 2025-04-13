using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "ColliderChannel", menuName = "Channels/ColliderChannel")]
public class ColliderChannel : SerializableScriptableObject
{
    [SerializeField, HideReferenceObjectPicker] private ActionReference<CollisionData> OnTriggerEnterAction = new();
    [SerializeField, HideReferenceObjectPicker] private ActionReference<CollisionData> OnTriggerExitAction = new();

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